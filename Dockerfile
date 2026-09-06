# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# SDK build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["ConferenceRoomBookingApi.csproj", "./"]
RUN dotnet restore "ConferenceRoomBookingApi.csproj"

# Copy source files and build
COPY . .
WORKDIR /src
RUN dotnet build "ConferenceRoomBookingApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ConferenceRoomBookingApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final production runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConferenceRoomBookingApi.dll"]
