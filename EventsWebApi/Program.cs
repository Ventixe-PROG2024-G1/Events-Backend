using EventsWebApi.Data.Context;
using EventsWebApi.Handler;
using EventsWebApi.Middleware;
using EventsWebApi.Repositories;
using EventsWebApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(option =>
    {
        option.Filters.Add<ApiKeyMiddleware>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.ExampleFilters();
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Events Web API",
        Description = "An ASP.NET Core Web API for managing Events",
    });

    var apiScheme = new OpenApiSecurityScheme
    {
        Name = "X-API-KEY",
        Description = "API Key needed to access the endpoints",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Scheme = "ApiKey",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "ApiKey"
        }
    };
    options.AddSecurityDefinition("ApiKey", apiScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { apiScheme, new List<string>() }
    });
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();


builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy",
        o => o
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

    options.AddPolicy("AllowSpecificHeaders", o => o
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .WithHeaders("Content-Type", "X-API-KEY"));
});

builder.Services.AddSingleton(typeof(ICacheHandler<>), typeof(CacheHandler<>));
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IEventService, EventService>();




var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Events Web API");
    options.RoutePrefix = string.Empty;
});

app.MapOpenApi();
app.UseHttpsRedirection();
app.UseCors("AllowSpecificHeaders");

app.UseAuthorization();
app.MapControllers();

app.Run();
