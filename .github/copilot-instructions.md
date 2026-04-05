# GitHub Copilot Instructions — GuedesPlace.AzureTools

## Project Overview

`GuedesPlace.AzureTools` is a .NET class library published as a NuGet package.  
It wraps the official Azure SDK clients (Blob, Queue, Tables) with fluent extension methods and higher-level typed abstractions, and provides utilities for Azure Functions authentication and configuration validation.

- **Package ID**: `GuedesPlace.AzureTools`
- **Target Frameworks**: `net8.0`, `net9.0`, `net10.0`
- **License**: Apache-2.0
- **Repository**: https://github.com/GuedesPlace/AzureTools

---

## Module Map

| Module | Namespace | Key Types |
|---|---|---|
| Configuration | `GuedesPlace.AzureTools.Configuration.Extensions` | `IConfigurationExtensions` |
| Blob Storage | `GuedesPlace.AzureTools.Blob.Extensions` | `BlobExtensions` |
| Queue Storage | `GuedesPlace.AzureTools.Queue.Extensions` | `QueueExtensions` |
| Table Storage | `GuedesPlace.AzureTools.Tables` | `TypedAzureTableClient<T>`, `MultiEntityAzureTableClient`, `ExtendedAzureTableClientService` |
| Azure Functions | `GuedesPlace.AzureTools.Functions.FunctionRunContext` | `IFunctionRunContext`, `UserFunctionRunContext`, `AppFunctionRunContext` |

---

## Coding Conventions

### General

- All public API is in English.
- Async methods end with `Async` and return `Task<T>` or `ValueTask<T>`.
- Use `CancellationToken` parameters where the underlying Azure SDK supports them.
- **JSON serialization uses Newtonsoft.Json** (`JsonConvert`), not `System.Text.Json`. Do not introduce `System.Text.Json` unless explicitly asked.

### Extension Methods (Blob, Queue, Configuration)

- Place extension methods in a **static class** inside `ModuleName/Extensions/`.
- The extending type must be the first parameter (`this BlobContainerClient`, `this QueueClient`, `this IConfiguration`).
- Keep extension classes focused on a single Azure SDK client type per file.

### Table Storage — Serialization

The Tables module uses a reflection-based ORM to round-trip POCOs through Azure Table Storage's flat property model:

- **`ObjectSerializer`** (static) — converts any POCO to `IDictionary<string, object>` for `TableEntity`.
  - Nested objects are **flattened with `_`-delimited key paths** (e.g., `Address_City`).
  - Delegates special types to `ConverterFactory`; primitives and `string`/`byte[]` pass through directly.
- **`ObjectBuilder`** (static) — reconstructs a POCO from a `TableEntity` using reflection.
  - Uses `RuntimeHelpers.GetUninitializedObject` — **no constructor is called** during deserialization.
  - Handles `DateTimeOffset → DateTime` conversion (UTC).
  - Detects nested objects by grouped `_`-prefixed keys.

### Table Storage — Converter Pattern

To support a new CLR type in Table serialization, implement `IConverter`:

```csharp
public interface IConverter
{
    bool IsType(Type type);
    string GetValue(Type type, object value);   // serialize to string
    object? BuildValue(string? value, Type type); // deserialize from string
}
```

Register the converter in `ConverterFactory` by adding it to the static converter list (in order).  
Existing converters: `EnumConverter`, `TimeSpanConverter`, `ArrayConverter`, `EnumerableConverter`.

**Priority**: converters are evaluated in registration order; the first `IsType` match wins.

### Azure Functions — FunctionRunContext

- `FunctionRunContext` is an **abstract base class**; never instantiate it directly.
- `UserFunctionRunContext` — for user-originated HTTP requests. Reads Azure Easy Auth's `x-ms-client-principal` header in production. Reads `DEV_USER_ID` / `DEV_USER_ROLES` env vars in dev mode.
- `AppFunctionRunContext` — for app-to-app / service principal callers. Pass `applicationId` and `roles` directly via constructor.
- Dev mode is detected when `AZURE_FUNCTIONS_ENVIRONMENT == "Development"` AND `IS_NOT_DEV` env var is absent.
- Role checks: `IsInAtLeastOneRole(params string[])` / `IsInAllRoles(params string[])` use set intersection on the roles collection.

---

## How to Add a New Azure Module

1. Create a folder `ModuleName/Extensions/` under `GuedesPlace.AzureTools/`.
2. Add a `public static class ModuleNameExtensions`.
3. Write `public static async Task<T>` extension methods on the relevant Azure SDK client type.
4. Use **Newtonsoft.Json** for any serialization.
5. Follow the async/`CancellationToken` conventions above.

## How to Add a New Table Type Converter

1. Create `Tables/Converters/MyTypeConverter.cs`.
2. Implement `IConverter`.
3. Register the converter in `ConverterFactory`'s static converter list before `ArrayConverter` / `EnumerableConverter` if your type could match those.

---

## CI / Publishing

Pushing to `main` triggers `.github/workflows/publishNuget.yml`, which builds in `Release` mode and pushes `*.nupkg` to NuGet.org using the `NUGET_API_KEY` secret.  
**Do not manually bump the version without also updating `<Version>` in `GuedesPlace.AzureTools.csproj`.**
