using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace Api.Helpers
{
    /// <summary>
    /// Custom model valiation Filter
    /// </summary>
    public class ModelValidationFilter : ActionFilterAttribute
    {
        /// <summary>
        /// Add model filter before executing a controller method
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    Errors = context.ModelState.Values.SelectMany(c => c.Errors).Select(c => c.ErrorMessage).ToList(),
                    Message = $"Information Invalid, {context.ModelState.Values.SelectMany(c => c.Errors).Select(c => c.ErrorMessage).FirstOrDefault()}",
                    Code = StatusCodes.Status400BadRequest
                });
            }
        }
    }
}