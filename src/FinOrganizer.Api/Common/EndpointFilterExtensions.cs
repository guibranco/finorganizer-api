using FluentValidation;

namespace FinOrganizer.Api.Common;

public static class EndpointFilterExtensions
{
    /// <summary>Runs the registered FluentValidation validator (if any) for <typeparamref name="TRequest"/> before the handler.</summary>
    public static RouteHandlerBuilder WithRequestValidation<TRequest>(this RouteHandlerBuilder builder)
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            var argument = context.Arguments.OfType<TRequest>().FirstOrDefault();
            if (argument is not null)
            {
                var validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
                if (validator is not null)
                {
                    var result = await validator.ValidateAsync(argument);
                    if (!result.IsValid)
                    {
                        return Results.ValidationProblem(result.ToDictionary());
                    }
                }
            }

            return await next(context);
        });

        return builder;
    }
}
