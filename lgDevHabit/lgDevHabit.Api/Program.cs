using lgDevHabit.Api;
using lgDevHabit.Api.Extensions;
using lgDevHabit.Api.Settings;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder
    .AddApiServices()
    .AddErrorHandling()
    .AddDatabase()
    .AddObservability()
    .AddApplicationServices()
    .AddAuthenticationServices()
    .AddCorsPolicy();


WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{

    app.MapOpenApi();
    await app.ApplyMigrationsAsync();
    //app.ApplyHabitsSeed();
    await app.SeedInitialDataAsync();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseCors(CorsOptions.PolicyName);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

