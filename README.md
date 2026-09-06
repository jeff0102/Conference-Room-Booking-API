# Conference Room Booking & Analytics API

[![.NET 9](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![Dapper](https://img.shields.io/badge/ORM-Dapper-orange.svg)](https://github.com/DapperLib/Dapper)
[![SQL Server 2022](https://img.shields.io/badge/Database-SQL%20Server%202022-red.svg)](https://www.microsoft.com/sql-server/)
[![Docker](https://img.shields.io/badge/Container-Docker%20Compose-2496ED.svg)](https://www.docker.com/)
[![Tests](https://img.shields.io/badge/Tests-30%20Passing-brightgreen.svg)]()

Production-grade RESTful Web API engineered in **.NET 9** for conference room inventory management, dynamic hourly reservation pricing, overlap conflict prevention, and operational business intelligence analytics. Designed following **Clean Architecture**, **Repository Pattern**, and **SOLID principles**, leveraging high-performance micro-ORM **Dapper** with raw parameterized SQL queries.

---

## Table of Contents
- [1. Executive Summary & Tech Stack](#1-executive-summary--tech-stack)
- [2. Dynamic Pricing Engine](#2-dynamic-pricing-engine)
- [3. Data Architecture & Seeding](#3-data-architecture--seeding)
- [4. Analytics & Reporting Suite](#4-analytics--reporting-suite)
- [5. Evaluation & Setup Options](#5-evaluation--setup-options)
  - [Option A (Docker Compose - Recommended)](#option-a-docker-compose---recommended)
  - [Option B (Local Development / Debugging)](#option-b-local-development--debugging)
- [6. Test Coverage](#6-test-coverage)
- [7. API Reference](#7-api-reference)
- [8. Future Improvements & Production Readiness](#8-future-improvements--production-readiness)

---

## 1. Executive Summary & Tech Stack

This solution provides a complete reservation engine and analytical layer for conference room facilities. It delivers sub-millisecond query execution through Dapper, automated database provisioning, idempotent seed data loading, and Swagger documentation at the root endpoint.

### Technology Stack
| Layer / Component | Technology | Description |
| :--- | :--- | :--- |
| **Runtime & SDK** | .NET 9.0 (C# 13) | High-performance modern backend runtime |
| **Architecture** | Clean Architecture | Separation of Concerns: Domain, Application, Infrastructure, Presentation |
| **Micro-ORM** | Dapper 2.1.79 | High-speed data mapper with parameterized raw SQL |
| **Database Provider** | Microsoft.Data.SqlClient 7.0.2 | Native Microsoft SQL Server driver |
| **Database Engine** | SQL Server 2022 | Relational storage with ACID transactional guarantees |
| **Documentation** | Swashbuckle OpenAPI 6.6.2 | Swagger UI served at root (`/`) with XML comments |
| **Testing** | xUnit 2.9.2 | Unit testing suite covering calculation engine and boundary cases |
| **Containerization** | Docker & Docker Compose | Multi-stage build container with orchestrated database networking |

### Architectural Structure
```
ConferenceRoomBookingApi/
├── Application/
│   ├── DTOs/                      # Request & Response Contracts (Room, Service, Booking, Analytics)
│   └── Services/                  # Business Logic (IBookingPriceCalculator, BookingPriceCalculator)
├── Controllers/                   # Web API REST Controllers (Rooms, Services, Bookings, Analytics)
├── Domain/
│   ├── Entities/                  # Core Business Entities (Room, Service, Booking, BookingServiceItem)
│   └── Interfaces/                # Repository Abstractions (IRoomRepository, IBookingRepository, etc.)
├── Infrastructure/
│   ├── Data/                      # Connection Factory & DatabaseInitializer
│   ├── Repositories/              # Dapper Concrete Repositories with Parameterized SQL
│   └── Scripts/                   # InitDatabase.sql schema migration and seeding script
├── Properties/                    # LaunchSettings.json configured for direct Swagger root routing
├── Tests/                         # xUnit Test Suite (30 unit tests covering pricing and intervals)
├── docker-compose.yml             # SQL Server 2022 + Web API orchestration
└── Dockerfile                     # Multi-stage build image targeting .NET 9
```

---

## 2. Dynamic Pricing Engine

Conference room rental fees follow time-of-day demand bands. The pricing engine (`BookingPriceCalculator`) applies dynamic adjustments depending on the reservation time window.

### Time-of-Day Multiplier Bands
| Interval | Band | Rate Adjustment | Multiplier |
| :--- | :--- | :--- | :--- |
| **06:00 – 09:00** | Morning hours | **10% discount** | `0.90x` |
| **09:00 – 12:00** | Standard hours | **Base hourly rate** | `1.00x` |
| **12:00 – 14:00** | Peak hours | **15% surcharge** | `1.15x` |
| **14:00 – 18:00** | Standard hours | **Base hourly rate** | `1.00x` |
| **18:00 – 23:00** | Evening / Night hours | **20% discount** | `0.80x` |

### Minute-Level Proration Mechanics
When a booking spans across multiple intervals (or begins at fractional hours such as `08:30`), the calculation engine prorates minute-by-minute without precision loss:

$$\text{WeightedMinutes} = \sum_{m=0}^{\text{TotalMinutes}-1} \text{Multiplier}\big(\text{TimeOfDay}(t_0 + m)\big)$$

$$\text{RoomRentalPrice} = \text{Round}\left(\frac{\text{BasePricePerHour} \times \text{WeightedMinutes}}{60}, 2\right)$$

$$\text{TotalPrice} = \text{RoomRentalPrice} + \sum_{s \in \text{SelectedServices}} \text{Price}(s)$$

- **Zero Floating-Point Error:** Computations are executed strictly using C# `decimal` arithmetic. Multiplication precedes division to eliminate truncation artifacts.
- **Flat-Rate Add-ons:** Selected services (Projector, Wi-Fi, Sound) are charged once per booking and added to the prorated room cost.

---

## 3. Data Architecture & Seeding

### Automated Database Provisioning (`DatabaseInitializer`)
On application startup:
1. Connects to the SQL Server `master` catalog using the configured connection string.
2. Checks:
   ```sql
   IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ConferenceRoomBookingDb')
   BEGIN
       CREATE DATABASE [ConferenceRoomBookingDb];
   END;
   ```
3. Switches connection context to `ConferenceRoomBookingDb` to ensure tables and constraints are created idempotently.

### Idempotent Seed Data
All table definitions and seed inserts use `IF NOT EXISTS` guards, allowing safe restarts without duplicate key conflicts.

#### Seeded Room Inventory
| Room Name | Capacity | Base Hourly Price |
| :--- | :--- | :--- |
| **Room A** | 50 seats | 2,000.00 UAH / hour |
| **Room B** | 100 seats | 3,500.00 UAH / hour |
| **Room C** | 30 seats | 1,500.00 UAH / hour |

#### Seeded Add-on Services Catalog
| Service Name | Flat Price |
| :--- | :--- |
| **Projector** | 500.00 UAH |
| **Wi-Fi** | 300.00 UAH |
| **Sound System** | 700.00 UAH |

### Transactional Overlap Conflict Detection (HTTP 409 Conflict)
Before creating a reservation, `BookingRepository.HasConflictAsync` validates room availability using the standard non-overlapping interval formula:

```sql
SELECT COUNT(1)
FROM Bookings
WHERE RoomId = @RoomId
  AND (@ExcludeBookingId IS NULL OR Id <> @ExcludeBookingId)
  AND BookingDate < DATEADD(hour, @DurationHours, @BookingDate)
  AND DATEADD(hour, DurationHours, BookingDate) > @BookingDate;
```
If an overlap is detected, the API rejects the request with `HTTP 409 Conflict`. When saving, an `IDbTransaction` persists both the `Bookings` entity and junction entries in `BookingServices`.

---

## 4. Analytics & Reporting Suite

The analytics engine provides operational intelligence through `AnalyticsController` and `AnalyticsRepository`:

### 1. Room Revenue & Booking Counts (`/api/analytics/room-revenue`)
Aggregates total reservations and financial volume per room over an optional date range (`startDate`, `endDate`).
```sql
SELECT 
    r.Id AS RoomId, r.Name AS RoomName,
    COUNT(b.Id) AS TotalBookings,
    COALESCE(SUM(b.TotalPrice), 0) AS TotalRevenue
FROM Rooms r
LEFT JOIN Bookings b ON r.Id = b.RoomId
    AND (@StartDate IS NULL OR b.BookingDate >= @StartDate)
    AND (@EndDate IS NULL OR b.BookingDate <= @EndDate)
GROUP BY r.Id, r.Name
ORDER BY TotalRevenue DESC;
```

### 2. Room Utilization Rate (`/api/analytics/room-utilization`)
Measures efficiency by evaluating hours booked against total operational capacity ($17\text{ available hours/day}$ between 06:00 and 23:00):

$$\text{Utilization Percentage} = \frac{\text{Total Booked Hours}}{\text{Evaluated Days} \times \text{Daily Operating Hours}} \times 100$$

### 3. Popular Add-on Services (`/api/analytics/popular-services`)
Ranks add-on items by customer selection count and total revenue contribution using parameterized `TOP (@Top)` queries.

### 4. Executive Summary Dashboard (`/api/analytics/summary`)
Returns high-level KPIs including total system revenue, total bookings placed, and per-room breakdowns in a single payload.

---

## 5. Evaluation & Setup Options

### Option A (Docker Compose - Recommended)
This option launches both Microsoft SQL Server 2022 and the .NET 9 API inside an isolated container network.

#### 1. Start the containers
```bash
docker compose up --build
```

#### 2. Access the Application
- **Swagger UI:** [http://localhost:7192/](http://localhost:7192/)
- **SQL Server:** Accessible on `localhost:1433` (`sa` / `YourStrong@Passw0rd`)

#### 3. Stop the containers
```bash
docker compose down
```

---

### Option B (Local Development / Debugging)
Run the database via Docker while developing or debugging the API locally in your IDE.

#### 1. Start only SQL Server
```bash
docker compose up -d sqlserver
```

#### 2. Run the Web API
```bash
dotnet run
```
*Note:* The application automatically reads `DefaultConnection` from `appsettings.json` (`Server=localhost,1433;Database=ConferenceRoomBookingDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;`), provisions the database, applies tables, and seeds initial data.

#### 3. Access Swagger UI
- Direct URL: [http://localhost:5027/](http://localhost:5027/) or [https://localhost:7192/](https://localhost:7192/)

*(For Windows-only environments without Docker, a LocalDB connection string alternative is available in `appsettings.Development.json`)*.

---

## 6. Test Coverage

A complete test suite is implemented in [`Tests/BookingPriceCalculatorTests.cs`](file:///C:/Users/jeffe/OneDrive/Escritorio/Conference-Room-Booking-API-main/ConferenceRoomBookingApi/Tests/BookingPriceCalculatorTests.cs) targeting .NET 9 using **xUnit**.

### Running Tests
Execute the following command in the solution root:
```bash
dotnet test
```

### Test Suite Summary (30 Tests)
- **Single Interval Tests:** Validates 10% morning discount, standard base rate, 15% peak surcharge, and 20% evening discount.
- **Spanning Multi-Interval Tests:** Validates bookings that cross rate boundaries:
  - Morning $\rightarrow$ Standard (`08:00 – 10:00`)
  - Standard $\rightarrow$ Peak (`11:00 – 13:00`)
  - Peak $\rightarrow$ Standard (`13:00 – 15:00`)
  - Standard through Peak to Standard (`11:00 – 15:00`)
  - Standard $\rightarrow$ Evening (`17:00 – 19:00`)
  - Full-Day Booking (`08:00 – 20:00`, 12-hour multi-band span)
- **Proration Tests:** Verifies half-hour offset calculation (`08:30 – 09:30`).
- **Add-on Services Tests:** Validates flat-rate service sum additions to the calculated room fee.
- **Domain Integration Tests:** Verifies `Booking.CalculateTotalPrice` with injected calculator.
- **Boundary Tests:** 13 parameterized `[Theory]` test cases checking exact interval limits (`06:00`, `08:59`, `09:00`, `11:59`, `12:00`, `13:59`, `14:00`, `17:59`, `18:00`, `22:59`, `23:00`).
- **Validation Guards:** Asserts `ArgumentOutOfRangeException` on negative prices or zero/negative durations.

```text
Aprovado!  – Com falha: 0, Aprovado: 30, Ignorado: 0, Total: 30, Duração: 25 ms
```

---

## 7. API Reference

All endpoints return and consume `application/json`. Detailed request/response schemas and example models are visible in the Swagger UI.

### Conference Rooms (`/api/rooms`)
| Method | Endpoint | Status Codes | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/rooms` | `200 OK` | Retrieves all conference rooms. |
| `GET` | `/api/rooms/{id}` | `200 OK`, `404 Not Found` | Retrieves a room by ID. |
| `POST` | `/api/rooms` | `201 Created`, `400 Bad Request` | Creates a new conference room. |

### Add-on Services (`/api/services`)
| Method | Endpoint | Status Codes | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/services` | `200 OK` | Retrieves all add-on services. |
| `GET` | `/api/services/{id}` | `200 OK`, `404 Not Found` | Retrieves a service by ID. |
| `POST` | `/api/services` | `201 Created`, `400 Bad Request` | Registers a new add-on service. |

### Reservations (`/api/bookings`)
| Method | Endpoint | Status Codes | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/bookings` | `200 OK` | Retrieves all reservations with room and service details. |
| `GET` | `/api/bookings/{id}` | `200 OK`, `404 Not Found` | Retrieves a reservation by ID. |
| `POST` | `/api/bookings` | `201 Created`, `400 Bad Request`, `409 Conflict` | Creates a booking with dynamic pricing and overlap validation. |

### Analytics & Reporting (`/api/analytics`)
| Method | Endpoint | Status Codes | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/analytics/summary` | `200 OK` | Executive summary (revenue, bookings, utilization, top services). |
| `GET` | `/api/analytics/room-revenue` | `200 OK` | Total revenue and booking counts grouped per room. |
| `GET` | `/api/analytics/room-utilization`| `200 OK` | Room utilization rate percentage vs. operating hours. |
| `GET` | `/api/analytics/popular-services`| `200 OK` | Most requested add-on services ranked by volume. |

---

## 8. Future Improvements & Production Readiness

For enterprise production scaling, the following enhancements can be incorporated:

1. **Authentication & Authorization (JWT / OAuth2 / RBAC):**
   - Integrate ASP.NET Core Identity with JWT bearer tokens.
   - Enforce role-based policies (`Admin` for room/service catalog management, `Staff`/`Customer` for reservations).
2. **Distributed Concurrency Control:**
   - Implement distributed locking via Redis (RedLock) or SQL Server `ROWVERSION` / snapshot isolation to prevent race conditions during high-volume concurrent booking submissions.
3. **Event-Driven Architecture & Messaging:**
   - Dispatch integration events (`BookingCreatedEvent`, `BookingCancelledEvent`) over RabbitMQ or Azure Service Bus.
   - Decouple asynchronous tasks such as email/SMS confirmation dispatches and calendar invites.
4. **Caching Layer:**
   - Cache room and service catalogs using Redis or in-memory output caching with sliding expirations to minimize database roundtrips.
5. **Observability & Health Checks:**
   - Integrate ASP.NET Core Health Checks (`/healthz`, `/ready`) monitoring SQL Server connectivity.
   - Export OpenTelemetry distributed traces and Prometheus metrics to Grafana or Datadog.
