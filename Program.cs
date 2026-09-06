using System.Reflection;
using ConferenceRoomBookingApi.Application.Services;
using ConferenceRoomBookingApi.Domain.Interfaces;
using ConferenceRoomBookingApi.Infrastructure.Data;
using ConferenceRoomBookingApi.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Database connection configuration for Dapper
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost,1433;Database=ConferenceRoomBookingDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;";

builder.Services.AddSingleton<IDbConnectionFactory>(_ => new SqlConnectionFactory(connectionString));

// Database Initializer
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

// Repositories (Dapper)
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

// Business Logic Services
builder.Services.AddScoped<IBookingPriceCalculator, BookingPriceCalculator>();

// Controllers & JSON serialization
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger Documentation with metadata and operational guidance for ABP Reviewers
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Conference Room Booking & Analytics API",
        Version = "v1",
        Description = """
            ### Enterprise Conference Room Booking Management & Analytics System
            **Architecture & Tech Stack:**
            - **Framework:** .NET 9 Core Web API
            - **Persistence & Data Access:** Micro-ORM Dapper with Microsoft SQL Server & parameterized SQL queries
            - **Pattern:** Clean Architecture, Repository Pattern, and Dependency Injection

            ---

            ### Operational Guide for ABP Reviewers:

            #### 1. Dynamic Hourly Pricing Engine:
            Reservations are calculated using dynamic time-of-day multipliers:
            * **Morning Hours (06:00 - 09:00):** 10% discount (`0.90x` multiplier)
            * **Standard Hours (09:00 - 12:00, 14:00 - 18:00):** Base hourly rate (`1.00x` multiplier)
            * **Peak Hours (12:00 - 14:00):** 15% surcharge (`1.15x` multiplier)
            * **Evening / Night Hours (18:00 - 23:00):** 20% discount (`0.80x` multiplier)
            * **Multi-interval Bookings:** Prorated hour-by-hour (or per minute) with zero floating-point precision loss.

            #### 2. Seed Data Available on Startup:
            * **Rooms:**
              * Room A (Capacity: 50, Base Rate: 2,000 UAH/hr)
              * Room B (Capacity: 100, Base Rate: 3,500 UAH/hr)
              * Room C (Capacity: 30, Base Rate: 1,500 UAH/hr)
            * **Add-on Services:**
              * Projector: 500 UAH
              * Wi-Fi: 300 UAH
              * Sound System: 700 UAH

            #### 3. Core Business Capabilities:
            * **Room Inventory (`/api/rooms`):** Retrieve and register conference rooms.
            * **Add-on Services (`/api/services`):** Manage available equipment and services.
            * **Booking Management (`/api/bookings`):** Make reservations, auto-detect time overlaps (409 Conflict), and compute final price.
            * **Analytics & Intelligence (`/api/analytics`):**
              * `/api/analytics/room-revenue`: Total revenue and booking counts per room.
              * `/api/analytics/room-utilization`: % of booked time vs. available operating hours.
              * `/api/analytics/popular-services`: Most demanded add-on services.
              * `/api/analytics/summary`: Aggregated executive dashboard.
            """,
        Contact = new OpenApiContact
        {
            Name = "Conference Room Booking Engineering Team",
            Email = "engineering@conferenceroombooking.com"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Run database initialization and seeding
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    await initializer.InitializeAsync();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Conference Room Booking API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at application root ('/')
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Conference Room Booking API v1");
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
