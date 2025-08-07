## About this Repository

This repository contains the source code for Dashy.Net, a personal dashboard application. It is a .NET-based clone inspired by the original Dashy project by Lissy93.

The project is built using .NET Aspire for orchestration, with a Blazor Server frontend and a .NET backend API service.

The primary goal is to create a modern, performant, and highly customizable dashboard application that is free and open-source under the GNU AGPLv3 license.

---

## Coding Conventions and Patterns to Follow

### Architecture

-   **Always respect the .NET Aspire project structure.** Each project (`AppHost`, `ApiService`, `Web`, `Shared`, `MigrationService`) has a distinct role.
-   **Always use the Backend-for-Frontend (BFF) pattern.** The Blazor `Web` project must not call third-party APIs directly. All external API calls must be proxied through the `ApiService`.
-   **The `Shared` project is for shared models only.** This includes Entity Framework entities, DTOs, and View Models like `ItemEditModel`. Do not place business logic here.
-   **Widget Descriptors go in the `Web` project.** All `IWidgetDescriptor` implementations must be located in the `Dashy.Net.Web/Services` directory to be discovered by the `WidgetRegistryService`.

### Widget Development

-   **Every widget MUST inherit from `WidgetBase`.** This is non-negotiable as it provides essential, shared functionality.

### Data and Form Handling

-   **All item editor forms MUST bind to an `ItemEditModel` instance.** This class is the definitive view model for item creation and editing.
-   **Use the `ToCreateDto()` and `ToUpdateDto()` methods** on the `ItemEditModel` to generate the data transfer objects for the API. Do not manually construct the `Options` dictionary in the UI components.
-   **For form binding, use mutable `class` DTOs** with `{ get; set; }` properties. Do not use positional `record` types for models that will be used with `@bind-Value`.

### Asynchronous and Component Lifecycle

-   **Prevent `ObjectDisposedException` after every `await`.** Any method that uses `await` for a potentially long-running operation (API calls, JS Interop) must check the `IsDisposed` flag from its component before attempting to update state.
    ```csharp
    var result = await MyLongCallAsync();
    if (IsDisposed) return;
    MyProperty = result;
    StateHasChanged();
    ```
-   **Use `InvokeAsync` for service event handlers.** When subscribing to events from singleton services, the handler must wrap `StateHasChanged` in `InvokeAsync` to avoid threading errors.
    ```csharp
    private void OnMyServiceChange()
    {
        InvokeAsync(StateHasChanged);
    }
    ```

### Things to Avoid

-   **Do not call external APIs directly from the Blazor frontend.** All calls must go through `ApiService`.
-   **Do not use `<DynamicComponent>`.** We have identified it as a source of threading issues. Use an explicit `@switch` statement to render different child components.
-   **Do not add component-specific styles to the global `app.css` file.** Use Blazor's CSS isolation (`.razor.css` files) for component-level styling.

### Avoid as long as possible
- **Avoid using 3rd party libraries.** If you need to use a library, ensure it is well-maintained and compatible with .NET Aspire. It should also be compliant with the GNU AGPLv3 license.

### Debugging and Testing
- **Use the built-in .NET logging framework.** Do not use `Console.WriteLine` or other ad-hoc logging methods. Use `ILogger<T>` for structured logging.
- **When running the application, always use the `dotnet watch` command.** This ensures that changes are automatically reloaded without needing to restart the application manually.
- **When opening the simple browser, use `https://localhost:7025/` instead of `http://localhost:17276/`.** This ensures that the the correct page is opened instead of the dotnet aspire page.