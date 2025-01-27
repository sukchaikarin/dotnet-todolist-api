var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ApplicationName = typeof(Program).Assembly.FullName,
    ContentRootPath = AppContext.BaseDirectory,
    WebRootPath = "wwwroot",
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
});

// Add services to the Dependency Injection container
builder.Services.AddSingleton<MongoDBService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure WebHost to listen on all IP addresses for Docker
builder.WebHost.UseUrls("http://0.0.0.0:80");

var app = builder.Build();

// Configure Swagger UI
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS redirection for non-Docker environments
if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

// ✅ ทดสอบการเชื่อมต่อ MongoDB
var todoService = app.Services.GetRequiredService<MongoDBService>();
if (!todoService.TestConnection())
{
    Console.WriteLine("Failed to connect to MongoDB. Please check your connection settings.");
    return; // หยุดการทำงานของแอปพลิเคชันถ้าการเชื่อมต่อล้มเหลว
}

// ✅ หากเชื่อมต่อสำเร็จ แอปพลิเคชันจะทำงานต่อ
Console.WriteLine("Successfully connected to MongoDB.");

// Simple Weather Forecast API
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// ✅ Health Check Endpoint
app.MapGet("/health", () => Results.Ok("Healthy"));

// ✅ Task Management Endpoints
// GET: Get all tasks
app.MapGet("/api/todo", async () => await todoService.GetAsync());

// GET: Get a task by ID
app.MapGet("/api/todo/{id}", async (string id) =>
{
    var task = await todoService.GetAsync(id);
    return task is not null ? Results.Ok(task) : Results.NotFound();
});

// POST: Create a new task
app.MapPost("/api/todo", async (TodoItem newItem) =>
{
    await todoService.CreateAsync(newItem);
    return Results.Created($"/api/todo/{newItem.Id}", newItem);
});

// PUT: Update an existing task
app.MapPut("/api/todo/{id}", async (string id, TodoItem updatedItem) =>
{
    var existingItem = await todoService.GetAsync(id);
    if (existingItem is null)
    {
        return Results.NotFound();
    }

    updatedItem.Id = existingItem.Id;
    await todoService.UpdateAsync(id, updatedItem);
    return Results.NoContent();
});

// DELETE: Delete a task
app.MapDelete("/api/todo/{id}", async (string id) =>
{
    var existingItem = await todoService.GetAsync(id);
    if (existingItem is null)
    {
        return Results.NotFound();
    }

    await todoService.DeleteAsync(id);
    return Results.NoContent();
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
