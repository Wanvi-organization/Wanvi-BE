using System.Reflection;
using System.Text.Json.Serialization;
using Wanvi.API.Middleware;
using WanviBE.API;
using WanviBE.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Config appsettings by env
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// C?u hình JSON Serializer
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddConfig(builder.Configuration);
builder.Services.AddHttpClient();

var app = builder.Build();

// C?u hình Middleware

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();  // ? Xác th?c ng??i dùng tr??c
app.UseAuthorization();   // ? Ki?m tra quy?n truy c?p API tr??c

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<PermissionMiddleware>();  // ? Ki?m tra quy?n sau khi token ?ã ???c xác th?c

app.MapControllers();

app.Run();
