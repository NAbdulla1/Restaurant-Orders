using Microsoft.AspNetCore.Mvc.Filters;
using System.Globalization;
using System.Text;

namespace Restaurant_Orders.Filters
{
    public class ServerExceptionFilter : IAsyncExceptionFilter
    {
        private readonly string _serverErrorLogFile;

        public ServerExceptionFilter(IConfiguration configuration)
        {
            _serverErrorLogFile = configuration.GetValue<string>("ServerErrorLogFile");
            EnsuteFileExists();
        }

        async Task IAsyncExceptionFilter.OnExceptionAsync(ExceptionContext context)
        {
            var ex = context.Exception;
            var exceptionDetails = ex.ToString();
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

        private void EnsuteFileExists()
        {
            if (File.Exists(_serverErrorLogFile))
            {
                return;
            }

            var parentPath = Directory.GetParent(_serverErrorLogFile).ToString();
            if (!Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }

            File.Create(_serverErrorLogFile).Dispose();
        }

    }
}
