# Dashy.Net

<p align="center">
  <img width="200" src="https://raw.githubusercontent.com/Lissy93/dashy/master/docs/assets/logo.png" alt="Dashy Logo">
</p>
<p align="center">
  A self-hosted, personal dashboard built for you. This project is a .NET-based clone inspired by the original <a href="https://github.com/Lissy93/dashy">Dashy by Lissy93</a>.
</p>

## About Dashy.Net

Dashy.Net helps you organize your applications, services, and bookmarks into a single dashboard. Built entirely on the .NET platform using Blazor for the frontend and .NET Aspire for orchestration, it aims to provide a fast, secure, and highly customizable dashboarding experience.
‼️‼️NOTE: This is still in an early development phase. The docker images are available, but as the license says it's delivered "as is"‼️‼️

## Key Features

- **✨ Customizable UI** - Easily change layouts, item sizes, and themes directly from the user interface.
- **🎨 Theming** - Comes with a beautiful dark and light theme out of the box.
- **🧩 Widget System** - Display dynamic information with widgets like a real-time clock, weather forecasts, and more. New widgets can be easily added.
- **🔧 UI Editor** - Configure everything directly through the UI. Add, remove, and edit sections and items in a simple, intuitive way.
- **🚀 Built with .NET** - A modern, performant, and cross-platform backend and frontend powered by .NET and Blazor.
- **☁️ .NET Aspire Orchestration** - Simplified development and deployment setup using the .NET Aspire framework.

## Getting Started

To get Dashy.Net up and running on your local machine, you'll need the .NET 9 SDK installed.

1.  **Clone the repository:**

    ```bash
    git clone https://github.com/Zalmez/Dashy.NET.git
    cd Dashy.NET
    ```

2.  **Run the application using .NET Aspire:**
    ```bash
    dotnet run --project Dashy.Net.AppHost
    ```

This will launch the .NET Aspire dashboard, which will start the Blazor frontend, the backend API service, and the PostgreSQL database. You can then navigate to the `webfrontend` URL shown in the dashboard.

## Configuration

Unlike the original Dashy's YAML-based configuration, Dashy.Net is configured entirely through the UI in **Edit Mode**. All configuration data is stored in a PostgreSQL database, which is automatically managed by the application.

## Documentation

📚 **[View Full Documentation](https://zalmez.github.io/Dashy.NET/)**

The complete documentation includes:
- Installation guides
- Docker deployment instructions
- Configuration tutorials
- Widget development guides
- API references

## License

Dashy.Net is free and open-source software licensed under the **GNU AGPLv3**. The full license text is available in the `LICENSE` file in the root of this repository.

This license ensures that the project and any derivatives will always remain free and open for the community to use, modify, and contribute to.

## Acknowledgements

This project would not exist without the incredible work done by **Alicia Sykes (Lissy93)** and it's contributors on the original [Dashy](https://github.com/Lissy93/dashy). Dashy.Net is proud to be inspired by dashy's original vision and contributions to the open-source community. The original Dashy project is licensed under the MIT license.
