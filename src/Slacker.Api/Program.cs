using Infinity.Toolkit.FeatureModules;
using Infinity.Toolkit.LogFormatter;
using Scalar.AspNetCore;
using Slacker.Api.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddFeatureModules();
builder.Logging.AddCodeThemeConsoleFormatter();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapFeatureModules();
app.Run();
