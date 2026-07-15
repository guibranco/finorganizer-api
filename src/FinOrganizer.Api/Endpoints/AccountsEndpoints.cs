using FinOrganizer.Api.Common;
using FinOrganizer.Application.Accounts;

namespace FinOrganizer.Api.Endpoints;

public static class AccountsEndpoints
{
    public static void MapAccountsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/accounts").WithTags("Accounts");

        group.MapGet("/", async (bool includeArchived, IAccountsService service, CancellationToken ct)
            => Results.Ok(await service.GetAllAsync(includeArchived, ct)));

        group.MapGet("/{id:guid}", async (Guid id, IAccountsService service, CancellationToken ct)
            => Results.Ok(await service.GetByIdAsync(id, ct)));

        group.MapPost("/", async (CreateAccountRequest request, IAccountsService service, CancellationToken ct) =>
            {
                var account = await service.CreateAsync(request, ct);
                return Results.Created($"/api/v1/accounts/{account.Id}", account);
            })
            .WithRequestValidation<CreateAccountRequest>();

        group.MapPut("/{id:guid}", async (Guid id, UpdateAccountRequest request, IAccountsService service, CancellationToken ct)
                => Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithRequestValidation<UpdateAccountRequest>();

        group.MapDelete("/{id:guid}", async (Guid id, IAccountsService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }
}
