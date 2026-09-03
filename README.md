# FactoryX

## Overview
FactoryX is a modular backend platform for factory management, built with .NET 9. It provides a foundation for digitalizing manufacturing operations, including machine tracking, production planning, operator management, inventory, and downtime analytics. The project is structured for maintainability and extensibility, making it suitable for both learning and real-world prototyping.

## Why FactoryX?
- **Layered Structure:** Clear separation between domain, application, infrastructure, and web layers for easier maintenance.
- **Extensible:** Easily add new entities, services, or integrations as your factory grows.
- **Practical Patterns:** Uses DTOs, repository pattern, dependency injection, and mapping for a clean and testable codebase.

## Features
- Machine management (CRUD, status, capacity)
- Operator management and shift assignments
- Product and material inventory tracking
- Work order and production scheduling
- Production record logging
- Downtime tracking and reporting
- User authentication and role management
- Consistent response and request handling patterns

## Solution Structure
- **Domain:** Entity definitions
- **Application:** DTOs, validators, service abstractions, and mapping profiles 
- **Infrastructure:** EF Core DbContext, repository implementations, dependency injection helpers 
- **Web:** ASP.NET MVC Web API for UI

## Technologies Used
- .NET 9 (C#)
- ASP.NET Core Web API
- Entity Framework Core (with Npgsql for PostgreSQL)
- AutoMapper (object mapping)
- FluentValidation (input validation)
- PostgreSQL (primary database)

## Getting Started
### Prerequisites
- .NET 9 SDK
- PostgreSQL
- Visual Studio 2022, VS Code, or any C# editor

### Setup
1. **Clone the repository:**
   ```bash
   git clone https://github.com/berkanserbes/FactoryX.git
   cd FactoryX
   ```
2. **Configure the database:**
   - Edit `FactoryX.Web/appsettings.json` and set your PostgreSQL connection string.
3. **Apply migrations:**
   ```bash
   dotnet ef database update --project FactoryX.Infrastructure --startup-project FactoryX.Web
   ```
4. **Build and run:**
   ```bash
   dotnet build
   dotnet run --project FactoryX.Web
   ```

## Contributing
Contributions are welcome! To get started:
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -m 'Add new feature'`)
4. Push to your branch (`git push origin feature/your-feature`)
5. Open a Pull Request

Please follow the existing code style and add tests for new features. For major changes, open an issue first to discuss your proposal.

## License
FactoryX is released under the MIT License. See the [LICENSE](LICENSE) file for details.

---
*Built for practical factory management and learning ASP.NET MVC development.*