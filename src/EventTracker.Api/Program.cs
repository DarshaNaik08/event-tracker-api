using EventTracker.Api.Endpoints;
using EventTracker.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new() { Title = "Event Tracker API", Version = "v1" });
});

// Allow the Angular dev server to call this API.
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Ok(new { name = "Event Tracker API", status = "running" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapEventEndpoints();

app.Run();