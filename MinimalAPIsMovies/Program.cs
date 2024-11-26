using Microsoft.AspNetCore.Cors;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Repositories;

var builder = WebApplication.CreateBuilder(args);

// services zone begin

builder.Services.AddScoped<IGenresRepository, GenresRepository>();
// example below to access a configuration property called lastName
var lastName = builder.Configuration.GetValue<string>("lastName");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(configuration =>
    {
        configuration.WithOrigins(builder.Configuration["allowedOrigins"]!).AllowAnyMethod().AllowAnyHeader();
    });

    options.AddPolicy("free", configuration =>
    {
        configuration.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddOutputCache();

// used to allow endpoints to emit metadata
builder.Services.AddEndpointsApiExplorer();
// used to set up swagger
builder.Services.AddSwaggerGen();

// services zone end

var app = builder.Build();

// middleware zone begin

app.UseSwagger();
app.UseSwaggerUI();

// CORS middleware
app.UseCors();

//output cache remember to put this middleware before the endpoint
app.UseOutputCache();

// example to access lastName
app.MapGet("/", () => lastName);

//app.MapGet("/", () => "Hello, world");

app.MapGet("/genres", () =>
{
    var genres = new List<Genre>()
    {
        new Genre { Id = 1, Name = "Drama"},
        new Genre { Id = 2, Name = "Action" },
        new Genre { Id = 3, Name = "Comedy" }

    };
    return genres;
}).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(15)));

app.MapPost("/genres", async (Genre genre, IGenresRepository genresRepository) =>
{
    await genresRepository.Create(genre);
    return TypedResults.Ok();
});
// middleware zone end

app.Run();

