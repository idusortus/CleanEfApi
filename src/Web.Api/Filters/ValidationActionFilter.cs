// for normalizing sucess and error responses
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CleanEfApi.Application.Responses; // Your DTOs

namespace CleanEfApi.Web.Api.Filters;

public class ValidationActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Check if the model state is invalid
        if (!context.ModelState.IsValid)
        {
            var errors = new List<ApiError>();
            foreach (var state in context.ModelState)
            {
                if (state.Value.Errors.Any())
                {
                    foreach (var error in state.Value.Errors)
                    {
                        errors.Add(new ApiError
                        {
                            Field = state.Key, // The name of the property (e.g., "author")
                            Message = error.ErrorMessage,
                            Code = "VALIDATION_FAILED"
                        });
                    }
                }
            }

            // Return a 400 Bad Request with your custom error response wrapper
            context.Result = new BadRequestObjectResult(
                ApiResponse.Error("One or more validation errors occurred.", errors)
            );
        }
    }

    // No-op, as we only need to act before the action executes
    public void OnActionExecuted(ActionExecutedContext context) { }
}