using FinOrganizer.Api.Common;
using FinOrganizer.Application.Common.Models;
using FinOrganizer.Application.Transactions;
using FinOrganizer.Domain.Enums;

namespace FinOrganizer.Api.Endpoints;

public static class TransactionsEndpoints
{
    public static void MapTransactionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/transactions").WithTags("Transactions");

        group.MapGet("/", async (
                Guid? accountId, Guid? categoryId, TransactionType? type,
                DateOnly? dateFrom, DateOnly? dateTo, string? search,
                int? page, int? pageSize,
                ITransactionsService service, CancellationToken ct) =>
            {
                var filter = new TransactionFilter(
                    accountId, categoryId, type, dateFrom, dateTo, search,
                    page ?? 1, pageSize ?? 50);
                return Results.Ok(await service.GetPagedAsync(filter, ct));
            })
            .Produces<PagedResult<TransactionDto>>();

        group.MapGet("/{id:guid}", async (Guid id, ITransactionsService service, CancellationToken ct)
                => Results.Ok(await service.GetByIdAsync(id, ct)))
            .Produces<TransactionDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateTransactionRequest request, ITransactionsService service, CancellationToken ct) =>
            {
                var transaction = await service.CreateAsync(request, ct);
                return Results.Created($"/api/v1/transactions/{transaction.Id}", transaction);
            })
            .WithRequestValidation<CreateTransactionRequest>()
            .Produces<TransactionDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", async (Guid id, UpdateTransactionRequest request, ITransactionsService service, CancellationToken ct)
                => Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithRequestValidation<UpdateTransactionRequest>()
            .Produces<TransactionDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, ITransactionsService service, CancellationToken ct) =>
            {
                await service.DeleteAsync(id, ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/import", async (ImportTransactionsRequest request, ITransactionsService service, CancellationToken ct)
                => Results.Ok(await service.ImportCsvAsync(request, ct)))
            .Produces<TransactionImportSummary>();
    }
}
