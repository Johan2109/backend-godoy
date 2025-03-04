using BackendProjectAPI.Services;
using MongoDB.Driver;
using BackendProjectAPI.Converters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    return new MongoClient("mongodb://localhost:27017");
});

builder.Services.AddSingleton<IUsuarioService, UsuarioService>();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new ObjectIdConverter());
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
    builder => builder.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();