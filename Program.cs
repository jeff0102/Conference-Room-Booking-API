using ConferenceRoomBookingApi.Application.Services;
using ConferenceRoomBookingApi.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Database connection configuration for Dapper
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=ConferenceRoomBookingDb;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddSingleton<IDbConnectionFactory>(_ => new SqlConnectionFactory(connectionString));

// Business Logic Services
builder.Services.AddScoped<IBookingPriceCalculator, BookingPriceCalculator>();

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
