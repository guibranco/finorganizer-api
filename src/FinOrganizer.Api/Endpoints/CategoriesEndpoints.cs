using FinOrganizer.Api.Common;
using FinOrganizer.Application.Categories;

namespace FinOrganizer.Api.Endpoints;

public static class CategoriesEndpoints
{
    public static void MapCategoriesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categories").WithTags("Categories");

        group.MapGet("/", async (ICategoriesService service, CancellationToken ct)
            => Results.Ok(await service.GetAllAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, ICategoriesService service, CancellationToken ct)
            => Results.Ok(await service.GetByIdAsync(id, ct)));

        group.MapPost("/", async (CreateCategoryRequest request, ICategoriesService service, CancellationToken ct) =>
            {
                var category = await service.CreateAsync(request, ct);
                return Results.Created($"/api/v1/categories/{category.Id}", category);
            })
            .WithRequestValidation<CreateCategoryRequest>();

        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest request, ICategoriesService service, CancellationToken ct)
                => Results.Ok(await service.UpdateAsync(id, request, ct)))
            .WithRequestValidation<UpdateCategoryRequest>();

        group.MapDelete("/{id:guid}", async (Guid id, ICategoriesService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }
}
