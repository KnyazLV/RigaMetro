# Project description | Riga Metro

An ASP.NET Core MVC application for educational purposes. The application allows you to explore a map of the non-existent Riga Metro. The map of stations and lines is based on a real metro project. 

The project includes metro management via an admin panel. The administrator can change the operating hours of the lines and the number of trains on each of them. The train arrival schedule at the station is generated automatically.

## Features
- **Automatic Schedule Generation**: Train timetables are calculated dynamically, considering the real geodesic distance between stations, time spent at stations, turnaround intervals at line terminals, and working hours of both trains and lines.
- **Comprehensive Metro Statistics**: Real-time statistics about metro line activity are generated and conveniently visualized on the admin dashboard, including charts and structured tables.
- **Multilingual Support**: The site is fully localized for three languages: Russian, Latvian, and English. All UI elements and error/help messages are presented in the selected language.
- **Automatic Database Setup**: The database is automatically initialized with all required lines, stations, and connections thanks to Entity Framework migrations and seed data.
## Technologies
Core technologies:
1. ASP.NET Core MVC
2. PostgreSQL (via Entity Framework Core)
3. MapBox GL JS

Additional libraries and tools:
- Bootstrap 5
-  jQuery
-  Chart.js
-  DataTables.net

## Gallery
Example of schedule on map
[!MapSchedule](https://github.com/user-attachments/assets/a7edfad0-d075-4279-b24b-f16a91413beb)

## Installation Guide
Also you need MapBox API Key and psql
1. Clone the Repository
```
git clone https://github.com/YourName/RigaMetro.git
cd RigaMetro
```
2. Build the Project
```
dotnet build
dotnet ef database update
```
3. Run Application
```
dotnet run
```
## Project Structure
```
├───Infrastructure
│   ├───Data             # DbContext, EF Core
│   └───Migrations       # DB Migrations
├───Properties           # Project Settings (launchSettings, AssemblyInfo etc.)
├───Resources            # .resx-files for localization
├───Services             # Business logic: schedules, seeders
├───Web
│   ├───Controllers      # MVC-controllers
│   ├───Models
│   │   └───ViewModels   # ViewModel-objects (subfolder division Account/Admin/Schedule)
│   └───Views
│       ├───Account
│       ├───Admin
│       ├───Home
│       └───Shared
└───wwwroot
    ├───css              # CSS Styles for UI
    ├───images           # Icons and logo
    ├───js               # Custom JS-modules (Mapbox, line-crud, train-crud etc.)
    └───lib
        ├───bootstrap    # Third-party libraries JS and CSS
        ├───jquery-validation
        └───jquery-validation-unobtrusive

```
