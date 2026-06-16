# TV Show Cache API

# 

# This is an ASP.NET Core 8 Web API. It connects to the TVMaze public API to pull TV show data and stores it in a local MS SQL Server database. Once a show is saved, any future requests for that same show come straight from the database instead of hitting the external API again.

# 

# 

# How It Works

# 

# The first time you request a TV show (by ID or name), the API checks the local database. If it's not there, it fetches the data from TVMaze, saves it to the database, and returns it to you. The next time you request the same show, it comes directly from the database. The response includes a source field that tells you whether the data came from "tvmaze" or "cache" so you can verify this yourself.

# 

# 

# Why I Chose TVMaze

# 

# TVMaze is a completely free and publicly available REST API. It doesn't require registration or an API key, which means anyone can clone this repo and run it immediately without configuring any credentials. The API is well-documented, has been stable for years, and returns rich data (genres, ratings, network, summaries etc.) which made it a good fit for this kind of caching exercise.

# 

# 

# No API Key Needed

# 

# TVMaze requires no API key and no environment variables. There is nothing to configure beyond the database connection string.

# 

# 

# Libraries and Frameworks Used

# 

# I want to be transparent about every dependency and why I chose it, as the assessment asked.

# 

# ASP.NET Core 8

# This is the current Long Term Support version of .NET for building Web APIs. It has built-in dependency injection, middleware support, and minimal boilerplate. I chose it because it is the industry standard for C# web APIs right now.

# 

# ADO.NET (no ORM)

# The assessment specifically said not to use an ORM. I used raw ADO.NET with SqlConnection, SqlCommand, and SqlDataReader throughout the repository layer. Every SQL query is written by hand. I used parameterized queries everywhere to prevent SQL injection.

# 

# Microsoft.Data.SqlClient

# This is the official Microsoft SQL Server driver for ADO.NET. I chose this over the older System.Data.SqlClient because it is actively maintained, receives security updates, and has better async support.

# 

# Microsoft.Extensions.Logging.Abstractions

# This provides the ILogger<T> interface. I added it to the Application and Infrastructure projects so they can log without depending on ASP.NET Core directly. This keeps the layer boundaries clean.

# 

# Microsoft.Extensions.Http

# This provides AddHttpClient<>(), which is the correct way to register typed HTTP clients in ASP.NET Core. It handles the HttpClient lifecycle properly and avoids socket exhaustion issues that come from creating HttpClient instances manually.

# 

# Swashbuckle.AspNetCore

# This generates the Swagger UI automatically from the controller attributes. I included it because it makes the API easy to test and explore without needing a separate tool like Postman. It was also already included in the ASP.NET Core Web API project template.

# 

# System.Text.Json

# This is built into .NET 8, so it requires no extra package. I used it for deserializing the TVMaze API responses. It is faster than Newtonsoft.Json and more than sufficient for the straightforward JSON mapping this project needs.

# 

# SOLID Principles and Clean Architecture
I applied the Dependency Inversion Principle throughout the codebase. The controller depends on ITvShowService (an interface) rather than TvShowService (the concrete class directly). This means the implementation can be swapped out without touching the controller at all. The same pattern applies between the service and repository layers. I also moved the cache-check logic out of the controller and into the service layer, because that decision belongs in business logic, not in HTTP handling code. Mapping between the domain entity and the DTO was moved into a dedicated Mappers folder for the same reason — the controller's only job is to handle HTTP requests and responses. CancellationToken is passed through all layers from the controller down to the database query, so if a client disconnects mid-request, the work stops immediately rather than continuing unnecessarily. 

# 

# Project Structure

# 

# The code is organized using Clean Architecture, split into four projects:

# 

# tvshow-cache-api/

# ├── src/

# │   ├── TvShowCacheApi.Domain/          # TvShow entity and repository interface

# │   ├── TvShowCacheApi.Application/     # TvShowService, ITvShowService interface

# │   ├── TvShowCacheApi.Infrastructure/  # ADO.NET repository + TVMaze HTTP client

# │   └── TvShowCacheApi.API/             # Controllers, middleware, Program.cs, Mappers

# ├── database/

# │   └── DbSchema.sql                      # SQL Server schema (run this first)

# ├── README.md

# └── TvShowCacheApi.sln

# 

# I chose Clean Architecture because it keeps each concern separate and makes the code easier to follow. The Domain layer has zero external dependencies. The Infrastructure layer handles all I/O (database and HTTP). The API layer is thin and just maps between HTTP and the application logic.

# 

# 

# Prerequisites

# 

# Before running the project, make sure you have the following installed:

# 

# 

# .NET 8 SDK

# SQL Server or SQL Server LocalDB (LocalDB comes free with Visual Studio 2022)

# Visual Studio 2022 or any editor with C# support

# 

# 

# If you have Visual Studio 2022 installed, LocalDB is already available — you do not need a separate SQL Server installation.

# 

# 

# Setup and Run Instructions

# 

# 1\. Clone the repository

# 

# bashgit clone https://github.com/tharu-1997/tvshow-cache-api

# cd tvshow-cache-api

# 

# 2\. Set up the database

# 

# If you have SQL Server or SQL Server Express:

# 

# bashsqlcmd -S localhost -E -i database/DbSchema.sql

# 

# If you are using LocalDB (comes with Visual Studio):

# 

# bashsqlcmd -S "(localdb)\\MSSQLLocalDB" -E -i database/DbSchema.sql

# 

# This script creates the TvShowCacheDb database and the TvShows table. It uses IF NOT EXISTS checks so it is safe to run more than once.

# 

# Alternatively, open database/DbSchema.sql in Visual Studio's SQL Server Object Explorer, connect to your instance, and click Execute.

# 

# 3\. Configure the connection string

# 

# Open src/TvShowCacheApi.API/appsettings.json and update the connection string to match your setup.

# 

# For full SQL Server:

# 

# json{

# &#x20; "ConnectionStrings": {

# &#x20;   "DefaultConnection": "Server=localhost;Database=TvShowCacheDb;Trusted\_Connection=True;TrustServerCertificate=True;"

# &#x20; }

# }

# 

# For LocalDB:

# 

# json{

# &#x20; "ConnectionStrings": {

# &#x20;   "DefaultConnection": "Server=(localdb)\\\\MSSQLLocalDB;Database=TvShowCacheDb;Trusted\_Connection=True;TrustServerCertificate=True;"

# &#x20; }

# }

# 

# For SQL Express:

# 

# json{

# &#x20; "ConnectionStrings": {

# &#x20;   "DefaultConnection": "Server=.\\\\SQLEXPRESS;Database=TvShowCacheDb;Trusted\_Connection=True;TrustServerCertificate=True;"

# &#x20; }

# }

# 

# 4\. Build and run the API

# 

# bashdotnet restore src/TvShowCacheApi.sln

# dotnet build src/TvShowCacheApi.sln

# dotnet run --project src/TvShowCacheApi.API/TvShowCacheApi.API.csproj

# 

# Once running, the terminal will show the port number. Open your browser and go to:

# 

# https://localhost:{PORT}/

# 

# Swagger UI will load and you can test all endpoints from there.

# 

# 

# API Endpoints

# 

# MethodEndpointDescriptionGET/api/tvshowReturns all TV shows currently stored in the local databaseGET/api/tvshow/{id}Gets a single show by its TVMaze ID — fetches from TVMaze and caches if not already storedGET/api/tvshow/name/{name}Searches for a show by name — fetches from TVMaze and caches if not already stored

# 

# The first two endpoints (GET /api/tvshow and GET /api/tvshow/{id}) satisfy assessment requirements 2 and 3. The name search endpoint is an extra convenience.

# 

# 

# Testing the Cache

# 

# To verify the caching works:

# 

# 

# Call GET /api/tvshow/82 — the response will include "source": "tvmaze" because it was fetched from the external API

# Call GET /api/tvshow/82 again — the response will now include "source": "cache" because it came from the local database

# Call GET /api/tvshow — Game of Thrones will now appear in the list

# 

# 

# 

# Example Response

# 

# json{

# &#x20; "id": 82,

# &#x20; "name": "Game of Thrones",

# &#x20; "status": "Ended",

# &#x20; "language": "English",

# &#x20; "genres": \["Drama", "Adventure", "Fantasy"],

# &#x20; "network": "HBO",

# &#x20; "rating": 8.9,

# &#x20; "officialSite": "http://www.hbo.com/game-of-thrones",

# &#x20; "summary": "Based on the bestselling book series by George R.R. Martin...",

# &#x20; "imageUrl": "https://static.tvmaze.com/uploads/images/medium\_portrait/498/1245274.jpg",

# &#x20; "cachedAt": "2026-06-16T02:34:23.961074Z",

# &#x20; "source": "tvmaze"

# }

# 

# 

# Error Handling

# 

# All unhandled exceptions are caught by a global middleware class (GlobalExceptionMiddleware) and returned as clean JSON error responses with appropriate HTTP status codes. The controller also handles specific cases like invalid IDs, shows not found, and TVMaze being unreachable, returning 400, 404, and 503 responses respectively.

# 

# 

# Database Schema

# 

# The schema script is in database/DbSchema.sql. It creates one table:

# 

# sqlCREATE TABLE dbo.TvShows

# (

# &#x20;   Id              INT             NOT NULL PRIMARY KEY,

# &#x20;   Name            NVARCHAR(200)   NOT NULL,

# &#x20;   Status          NVARCHAR(50)    NOT NULL DEFAULT '',

# &#x20;   Language        NVARCHAR(50)    NOT NULL DEFAULT '',

# &#x20;   Genres          NVARCHAR(500)   NOT NULL DEFAULT '',

# &#x20;   Network         NVARCHAR(200)   NOT NULL DEFAULT '',

# &#x20;   Rating          FLOAT           NULL,

# &#x20;   OfficialSite    NVARCHAR(500)   NOT NULL DEFAULT '',

# &#x20;   Summary         NVARCHAR(MAX)   NOT NULL DEFAULT '',

# &#x20;   ImageUrl        NVARCHAR(500)   NOT NULL DEFAULT '',

# &#x20;   CachedAt        DATETIME2       NOT NULL DEFAULT GETUTCDATE()

# );

# 

# The Id column uses the TVMaze show ID directly as the primary key. Saves use a SQL MERGE statement (upsert) so re-fetching a show updates the cached record rather than throwing a duplicate key error.

