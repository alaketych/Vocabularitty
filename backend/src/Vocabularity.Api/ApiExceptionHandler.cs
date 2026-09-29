using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Vocabularity.Core;

namespace Vocabularity.Api;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger, IProblemDetailsService problems) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is OperationCanceledException && ct.IsCancellationRequested)
        {
            return false;
        }

        var isDeadlock = exception is SqlException { Number: 1205 }
            || exception.InnerException is SqlException { Number: 1205 };
        var status = isDeadlock ? 409 : exception is ApiException api ? api.StatusCode : 500;
        var title = isDeadlock
            ? "A concurrent change interrupted this operation. Refresh and retry."
            : status == 500 ? "An unexpected error occurred." : exception.Message;
        if (status == 500)
        {
            logger.LogError(exception, "Unhandled request error");
        }

        context.Response.StatusCode = status;
        await problems.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Extensions = { ["trace_id"] = context.TraceIdentifier }
            }
        });

        return true;
    }
}

