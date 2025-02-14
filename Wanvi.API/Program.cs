using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Wanvi.API.Middleware;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Repositories.Context;
using WanviBE.API;
using WanviBE.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Config appsettings by env
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

//builder.Services.AddDbContext<DatabaseContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddConfig(builder.Configuration);

var app = builder.Build();

//setting Middleware
app.UseMiddleware<ExceptionMiddleware>();
//app.UseMiddleware<PermissionMiddleware>();
app.UseMiddleware<LoggingMiddleware>();

// Configure the HTTP request pipeline.

app.UseSwagger();
    app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();




app.MapControllers();

app.Run();
