using BackEnd.Application.Common.ResponseFormat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using IResult = BackEnd.Application.Common.ResponseFormat.IResult;

namespace BackEnd.Api.Filters
{
    public sealed class ResultActionFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(
            ResultExecutingContext context,
            ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult &&
                objectResult.Value is IResult result)
            {
                context.Result = CreateActionResult(result, objectResult.StatusCode);
            }

            await next();
        }

        private IActionResult CreateActionResult(IResult result, int? currentStatusCode)
        {
            if (result.IsSuccess)
            {
                var statusCode = currentStatusCode ?? (int)HttpStatusCode.OK;
                if (statusCode < 200 || statusCode > 299) statusCode = (int)HttpStatusCode.OK;

                return new ObjectResult(
                    ApiResponse<object>.Success(result.Value, (HttpStatusCode)statusCode, result.Message))
                {
                    StatusCode = statusCode
                };
            }

            var errorStatusCode = result.ErrorType switch
            {
                ErrorType.NotFound => HttpStatusCode.NotFound,
                ErrorType.BadRequest => HttpStatusCode.BadRequest,
                ErrorType.Conflict => HttpStatusCode.Conflict,
                ErrorType.UnprocessableEntity => HttpStatusCode.UnprocessableEntity,
                ErrorType.Unauthorized => HttpStatusCode.Unauthorized,
                ErrorType.Forbidden => HttpStatusCode.Forbidden,
                _ => HttpStatusCode.InternalServerError
            };

            return new ObjectResult(
                ApiResponse<object>.Fail(result.Message, errorStatusCode))
            {
                StatusCode = (int)errorStatusCode
            };
        }
    }
}
