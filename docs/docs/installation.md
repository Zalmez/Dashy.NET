---
sidebar_position: 2
---

# Installation

This guide will help you get Dashy.NET up and running on your system. There are several ways to install and run Dashy.NET depending on your needs.

## Prerequisites

### For Local Development
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/) (for cloning the repository)

### For Docker Deployment
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/) (usually included with Docker Desktop)

## Local Development Setup

Perfect for developers who want to contribute to the project or run the latest development version.

### 1. Clone the Repository

```bash
git clone https://github.com/Zalmez/Dashy.NET.git
cd Dashy.NET
```

### 2. Run with .NET Aspire

The simplest way to run the entire application stack:

```bash
dotnet run --project Dashy.Net.AppHost
```

This command will:
- Start the .NET Aspire dashboard
- Launch the Blazor frontend
- Start the backend API service
- Set up a PostgreSQL database automatically

### 3. Access the Application

After running the command, the .NET Aspire dashboard will open in your browser. From there:

1. Look for the `webfrontend` service
2. Click on the URL to access your Dashy.NET dashboard
3. The application will typically be available at `https://localhost:7025`

## Docker Deployment

The recommended approach for production deployments.

### Using Docker Compose

1. **Download the required files**:
   ```bash
   # Download docker-compose.yml and environment template
   wget https://raw.githubusercontent.com/Zalmez/Dashy.NET/main/docker-compose.yml
   wget https://raw.githubusercontent.com/Zalmez/Dashy.NET/main/.env.example
   
   # Copy and customize environment file
   cp .env.example .env
   nano .env  # Edit with your preferred settings
   ```

2. **Start the services**:
   ```bash
   docker-compose up -d
   ```

3. **Access the application**:
   Open your browser and navigate to `http://localhost:8080`

### Custom Docker Setup

You can also run the individual services manually:

```bash
# Start PostgreSQL
docker run -d \
  --name dashy-postgres \
  -e POSTGRES_DB=DashyNet \
  -e POSTGRES_USER=dashynet \
  -e POSTGRES_PASSWORD=your-secure-password \
  -p 5432:5432 \
  postgres:15

# Start Dashy.NET
docker run -d \
  --name dashy-net \
  -p 8080:80 \
  -e ConnectionStrings__DefaultConnection="Host=dashy-postgres;Database=DashyNet;Username=dashynet;Password=your-secure-password" \
  --link dashy-postgres \
  zalmez/dashy-net:latest
```

## Environment Variables

When deploying Dashy.NET, you can customize its behavior using environment variables:

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | Required for Docker |
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | Production |
| `ASPNETCORE_URLS` | URLs the app listens on | http://+:80 |

## Verification

To verify that Dashy.NET is running correctly:

1. **Check the web interface**: Navigate to your application URL
2. **Test the API**: Visit `/api/version` to see the API version
3. **Check logs**: Use `docker logs` (Docker) or check the console output (local development)

## Troubleshooting

### Common Issues

**Port conflicts**: If ports 8080 or 5432 are already in use, modify the port mappings in your docker-compose.yml or use different ports.

**Database connection issues**: Ensure PostgreSQL is running and the connection string is correct.

**Permission issues**: On Linux/macOS, you might need to run Docker commands with `sudo` or add your user to the docker group.

### Getting Help

If you encounter issues:
1. Check the [GitHub Issues](https://github.com/Zalmez/Dashy.NET/issues) for known problems
2. Review the application logs for error messages
3. Create a new issue with detailed information about your setup and the problem

## Next Steps

Once Dashy.NET is running:
- [Configure your dashboard](/docs/configuration)
- [Learn about widgets](/docs/widgets)
- [Explore theming options](/docs/theming)
