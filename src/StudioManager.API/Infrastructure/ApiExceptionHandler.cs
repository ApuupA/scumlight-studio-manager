using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudioManager.ServiceLayer.Exceptions;

namespace StudioManager.API.Infrastructure
{
    /// <summary>
    /// Turns the known service exceptions into proper HTTP responses, so controllers don't need try/catch:
    /// EntityNotFoundException -> 404, BusinessRuleException -> 409 (with "errorCode" for the frontend translation).
    /// Every other exception stays a 500 and is logged.
    /// </summary>
    public class ApiExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;

        public ApiExceptionHandler(IProblemDetailsService problemDetailsService)
        {
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            ProblemDetails? problem = exception switch
            {
                EntityNotFoundException => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = exception.Message
                },
                BusinessRuleException businessRule => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = businessRule.Message,
                    Extensions = { ["errorCode"] = businessRule.ErrorCode }
                },
                // Fallback for references the services don't check themselves (e.g. a song for a project that
                // was deleted in the meantime): the database rejects it with a foreign key violation
                DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation } } => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "The data refers to an entry that does not exist (anymore).",
                    Extensions = { ["errorCode"] = "common.invalidReference" }
                },
                _ => null
            };

            if (problem == null)
            {
                return false;
            }

            httpContext.Response.StatusCode = problem.Status!.Value;
            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
                Exception = exception
            });
        }
    }
}
