using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public class BodyReader : IBodyReader
    {
        private Stream? _originalResponseBodyStream;
        private MemoryStream? _memoryStream;
        private bool _originalResponseStreamReturned;

        public virtual async Task<string> ReadRequestBodyAsync(HttpContext context, int bytes, string appendix)
        {
            context.Request.EnableBuffering();

            // Leave stream open so next middleware can read it
            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 512, leaveOpen: true);

            string requestBody;

            if (bytes > 0 && bytes < context.Request.ContentLength)
            {
                var buffer = new char[bytes];
                _ = await reader.ReadAsync(buffer, 0, bytes);

                requestBody = new string(buffer);

                if (!string.IsNullOrEmpty(appendix))
                    requestBody += appendix;
            }
            else
            {
                requestBody = await reader.ReadToEndAsync();
            }

            // Reset stream position, so next middleware can read it
            context.Request.Body.Position = 0;

            return requestBody;
        }

        public virtual void PrepareResponseBodyReading(HttpContext context)
        {
            // Store original response body stream
            _originalResponseBodyStream = context.Response.Body;

            // Swap out stream with one that is buffered and supports seeking
            _memoryStream = new MemoryStream();
            context.Response.Body = _memoryStream;
        }

        public virtual async Task<string> ReadResponseBodyAsync(HttpContext context, int bytes, string appendix)
        {
            if (_memoryStream == null)
            {
                throw new ArgumentNullException(nameof(_memoryStream), "Call PrepareResponseBodyReading() before passing control to the next delegate!");
            }

            if (_originalResponseBodyStream == null)
            {
                throw new ArgumentNullException(nameof(_originalResponseBodyStream), "Call PrepareResponseBodyReading() before passing control to the next delegate!");
            }

            try
            {
                _memoryStream.Position = 0;
                var reader = new StreamReader(_memoryStream);

                string responseBody;

                if (bytes > 0 && bytes < context.Response.ContentLength)
                {
                    var buffer = new char[bytes];
                    _ = await reader.ReadAsync(buffer, 0, bytes);

                    responseBody = new string(buffer);

                    if (!string.IsNullOrEmpty(appendix))
                        responseBody += appendix;
                }
                else
                {
                    responseBody = await reader.ReadToEndAsync();
                }

                return responseBody;
            }
            finally
            {
                await RestoreOriginalResponseStream(context);
            }
        }
        
        public async Task RestoreOriginalResponseBodyStreamAsync(HttpContext context)
        {
            if (_memoryStream == null)
            {
                throw new ArgumentNullException(nameof(_memoryStream), "Call PrepareResponseBodyReading() before passing control to the next delegate!");
            }

            if (_originalResponseBodyStream == null)
            {
                throw new ArgumentNullException(nameof(_originalResponseBodyStream), "Call PrepareResponseBodyReading() before passing control to the next delegate!");
            }

            await this.RestoreOriginalResponseStream(context);
        }

        private async Task RestoreOriginalResponseStream(HttpContext context)
        {
            if (!_originalResponseStreamReturned)
            {
                // Copy back so response body is available for the user agent
                _memoryStream.Position = 0;
                await _memoryStream.CopyToAsync(_originalResponseBodyStream);
                
                context.Response.Body = _originalResponseBodyStream;
                _originalResponseStreamReturned = true;
            }
        }
    }
}
