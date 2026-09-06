using ConferenceRoomBookingApi.Application.Services;
using ConferenceRoomBookingApi.Domain.Interfaces;
using ConferenceRoomBookingApi.Infrastructure.Data;
using ConferenceRoomBookingApi.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Database connection configuration for Dapper
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=ConferenceRoomBookingDb;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddSingleton<IDbConnectionFactory>(_ => new SqlConnectionFactory(connectionString));

// Database Initializer
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

// Repositories (Dapper)
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// Business Logic Services
builder.Services.AddScoped<IBookingPriceCalculator, BookingPriceCalculator>();

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Run database initialization and seeding
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    await initializer.InitializeAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
