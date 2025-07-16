# Riga Metro — Educational ASP.NET Core MVC Application

Riga Metro is a demonstration ASP.NET Core MVC application modeling a fictional Riga underground system. It lets you explore an interactive metro map, simulate train schedules, and manage system data through an admin panel.

## Demo

**Live Demo:**  
Available: https://rigametro.knyaz.eu/

## Overview

- **Interactive Metro Map:** Visualizes a metro network based on Riga’s unrealized metro plans.
- **Admin Management:** Administrators adjust line schedules, manage train counts, and regenerate schedules in real time.
- **Real-Time Simulation:** Train arrivals and timetables are calculated dynamically using geospatial data and operational constraints.

## Features

- **Dynamic Schedule Generation:**
    - Algorithms calculate train timetables based on true geodesic distances, time at stations, turnaround intervals, and line/train working hours.
- **Metro Statistics Dashboard:**
    - Charts and tables display system load, train utilization, and key metrics for each line.
- **Multilingual UI:**
    - Full localization in English, Russian, and Latvian, including UI, validation, and error/help messages.
- **Effortless Database Initialization:**
    - Entity Framework Core migrations seed the database with all required lines, stations, and reference data on first launch.

## Demo Gallery

![Map](https://imgur.com/1OKZzHH.png)
 
![Trains](https://i.imgur.com/C8WEKCc.png)

![Dashboard](https://imgur.com/i7kIXVx.png)

![Lines](https://imgur.com/NqHuBEv.png)

![Schedule](https://imgur.com/oXN9Vjs.png)

## Getting Started

### Requirements

- **.NET SDK 9.0**
- **PostgreSQL 17.4**
- **Docker + Docker Compose (for container-based deployment)**

### Quickstart (Docker Compose)

1. **Clone the Repository**
   ```bash
   git clone https://github.com/YourName/RigaMetro.git
   cd RigaMetro
   ```
2. **Create a `.secrets` file**  
   Insert your MapBox API key (and psql connection string) as environment variables, e.g.:
   ```
   MapBox__ApiKey=[YOUR_MAPBOX_TOKEN]
   ConnectionStrings__MetroConnection=Host=db;Database=riga_metro;Username=admin;Password=admin
   AdminCredentials__Username=[YOUR_USERNAME]
   AdminCredentials__Password=[YOUR_PASSWORD]
   ```
3. **Launch with Docker Compose**
   ```bash
   sudo docker compose up
   ```
   This will build images, create the database, apply migrations, and seed all initial data.
4. **Open the app**  
   Visit [http://localhost:8080](http://localhost:8080)

### Manual Build & Local Run

1. **Clone the Repository**
   ```bash
   git clone https://github.com/YourName/RigaMetro.git
   cd RigaMetro
   ```
2. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```
3. **Build the Project**
   ```bash
   dotnet publish -c Release -o ./publish
   ```
4. **Setup Secrets (User Secrets Provider)**
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=riga_metro;Username=admin;Password=admin"
   dotnet user-secrets set "MapBox:ApiKey" "[YOUR_MAPBOX_TOKEN]"
   dotnet user-secrets set "AdminCredentials:Username" "admin"
   dotnet user-secrets set "AdminCredentials:Password" "admin"
   ```
5. **Apply Database Migrations**
   (If running locally _outside_ Docker, you may need to create the database and apply migrations manually, depending on your configuration.)
   ```bash
   dotnet ef database update --project Infrastructure
   ```
6. **Run the Application**
   ```bash
   dotnet ./publish/RigaMetro.dll
   ```

---

## Project Structure

```
├── Infrastructure
│   ├── Data             # DbContext & EF Core setup
│   └── Migrations       # EF Core migrations
├── Properties           # Project settings (launch, assembly info)
├── Resources            # Localization (.resx) files
├── Services             # Business logic: scheduling, data seeders, etc.
├── Web
│   ├── Controllers      # MVC controllers
│   ├── Models
│   │   └── ViewModels   # Specialized view models (Account, Admin, Schedule)
│   └── Views
│       ├── Account
│       ├── Admin
│       ├── Home
│       └── Shared
└── wwwroot
    ├── css              # UI styles
    ├── images           # Icons & logos
    ├── js               # Custom JS modules (map, CRUD interfaces)
    └── lib
        ├── bootstrap
        ├── jquery-validation
        └── jquery-validation-unobtrusive
```

## Technologies Used

- **Backend:** ASP.NET Core MVC, EF Core (PostgreSQL)
- **Frontend:** Bootstrap 5, MapBox GL JS, jQuery, Chart.js, DataTables.net
- **Localization:** Resx-resource-based i18n (English, Russian, Latvian)
- **Containerization:** Docker, Docker Compose

## Notes

- _First-time run_ automatically sets up schema and initial data thanks to migrations and data seeding.
- _Admin panel_ available at `/Admin` (credentials are set via secrets).
