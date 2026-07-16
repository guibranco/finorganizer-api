using FinOrganizer.Api.BackgroundServices;
using FinOrganizer.Api.Endpoints;
using FinOrganizer.Api.Middleware;
using FinOrganizer.Application;
using FinOrganizer.Infrastructure;
using FinOrganizer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "FinOrganizer API",
        Version = "v1",
        Description = "Personal finance tracking, portfolio, and planning API.",
    });
});

builder.Services.AddHostedService<RecurrencePostingBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseHttpsRedirection();

app.MapAccountsEndpoints();
app.MapCategoriesEndpoints();
app.MapTransactionsEndpoints();
app.MapRecurrenceEndpoints();
app.MapAssetsEndpoints();
app.MapBudgetsEndpoints();
app.MapGoalsEndpoints();
app.MapDashboardEndpoints();

app.Run();

public partial class Program;
