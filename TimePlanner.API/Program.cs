using TimePlanner.API.Identity;
using TimePlanner.Repositories;
using TimePlanner.Services;
using TimePlanner.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddServices();
builder.Services.AddScoped<ICurrentUser, DevelopmentCurrentUser>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();