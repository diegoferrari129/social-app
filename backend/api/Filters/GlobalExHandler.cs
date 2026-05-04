using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Filters
{
    public class GlobalExHandler : IExceptionFilter
    {
        private readonly ILogger<GlobalExHandler> _logger;
        public GlobalExHandler(ILogger<GlobalExHandler> logger) => _logger = logger;
        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Unhandled exception occurred");
            context.Result = new ObjectResult(new { message = "An internal error has occurred" })
            {
                StatusCode = 500
            };
            context.ExceptionHandled = true;
        }
    }
}
