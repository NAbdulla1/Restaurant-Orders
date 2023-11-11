using Microsoft.AspNetCore.Builder;
using System.Globalization;
using System.Text;

namespace Restaurant_Orders.Middlewares
{
    public class ServerErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _serverErrorLogFile;

        public ServerErrorHandlerMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _serverErrorLogFile = configuration.GetValue<string>("ServerErrorLogFile") ?? "_error.log";
            EnsuteFileExists();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode >= 500)
            {
                var exceptionDetails = "An error occurred on the server.";
                var time = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
                var text = $"[{time}]: {exceptionDetails}{Environment.NewLine}{Environment.NewLine}";

                using (var logFile = new FileStream(
                    _serverErrorLogFile,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 4096,
                    useAsync: true))
                {
                    await logFile.WriteAsync(Encoding.UTF8.GetBytes(text));
                }
            }
        }

        private void EnsuteFileExists()
        {
            if (File.Exists(_serverErrorLogFile))
            {
                return;
            }

            var fileParentDir = Directory.GetParent(_serverErrorLogFile);
            var parentPath = fileParentDir != null ?
                fileParentDir.ToString() :
                Directory.GetCurrentDirectory();

            if (!Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }

            File.Create(_serverErrorLogFile).Dispose();
        }
    }

    public static class ServerErrorHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseServerErrorHandler(this IApplicationBuilder appBuilder)
        {
            return appBuilder
                .UseMiddleware<ServerErrorHandlerMiddleware>();
        }
    }
}
