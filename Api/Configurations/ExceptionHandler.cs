using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Errors.Model;
using Service.Implementation;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Configurations
{
    /// <summary>
    /// Adds the Exception handler middle to catch general exceptions
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> logger;
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context, IWebHostEnvironment env)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await LogErrorMiddleware(context, ex);
                await HandleExceptionAsync(context, ex, env);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, IWebHostEnvironment env)
        {
            HttpStatusCode status;
            string message;
            var stackTrace = string.Empty;

            var exceptionType = exception.GetType();
            if (exceptionType == typeof(BadRequestException))
            {
                message = exception.Message;
                status = HttpStatusCode.BadRequest;
            }
            else if (exceptionType == typeof(NotFoundException))
            {
                message = exception.Message;
                status = HttpStatusCode.NotFound;
            }
            else
            {
                status = HttpStatusCode.InternalServerError;
                message = exception.Message;
                if (env.IsEnvironment("Development"))
                    stackTrace = exception.StackTrace;
            }

            var result = JsonSerializer.Serialize(new { error = message, stackTrace });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;
            return context.Response.WriteAsync(result);
        }

        public async Task LogErrorMiddleware(HttpContext context, Exception exception)
        {
            var method = context.Request.Method;
            var path = context.Request.Path;

            var trace = new StackTrace(exception, true);
            var frames = trace.GetFrames();
            if (frames != null)
            {
                var relevantFrame = frames.FirstOrDefault(frame =>
                    frame.GetMethod().DeclaringType != typeof(ErrorHandlingMiddleware)
                    && !IsSystemOrMicrosoftNamespace(frame.GetMethod().DeclaringType?.Namespace));

                if (relevantFrame != null)
                {
                    var methodName = relevantFrame.GetMethod().Name;
                    var className = relevantFrame.GetMethod().DeclaringType?.FullName;

                    // Try to get line and column numbers
                    var lineNumber = relevantFrame.GetFileLineNumber();
                    var columnNumber = relevantFrame.GetFileColumnNumber();
                    logger.LogError(exception, "Exception occurred in {Method} at {Path}. Class: {ClassName}, Method: {MethodName}, Line: {LineNumber}, Column: {ColumnNumber}.", method, path, className, methodName, lineNumber, columnNumber);

                }
                else
                {
                    logger.LogError(exception, "Exception in {Method} {Path}. Unable to determine the exact method.", method, path);
                }
            }
            else
            {
                logger.LogError(exception, "Exception in {Method} {Path}. Unable to retrieve stack trace.", method, path);
            }
        }

        private static bool IsSystemOrMicrosoftNamespace(string namespaceName)
        {
            return namespaceName != null && (namespaceName.StartsWith("System") || namespaceName.StartsWith("Microsoft"));
        }
    }
}