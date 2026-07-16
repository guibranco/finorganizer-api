using System.Reflection;
using FinOrganizer.Application.Accounts;
using FinOrganizer.Application.Assets;
using FinOrganizer.Application.Budgets;
using FinOrganizer.Application.Categories;
using FinOrganizer.Application.Dashboard;
using FinOrganizer.Application.Goals;
using FinOrganizer.Application.Projection;
using FinOrganizer.Application.Recurrence;
using FinOrganizer.Application.Transactions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinOrganizer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IAccountsService, AccountsService>();
        services.AddScoped<ICategoriesService, CategoriesService>();
        services.AddScoped<ITransactionsService, TransactionsService>();
        services.AddScoped<IRecurrenceRulesService, RecurrenceRulesService>();
        services.AddScoped<IRecurrencePostingService, RecurrencePostingService>();
        services.AddScoped<IAssetsService, AssetsService>();
        services.AddScoped<IBudgetsService, BudgetsService>();
        services.AddScoped<ISavingsGoalsService, SavingsGoalsService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IProjectionService, ProjectionService>();

        return services;
    }
}
