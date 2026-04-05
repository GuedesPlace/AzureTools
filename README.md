# GuedesPlace.AzureTools

[![NuGet](https://img.shields.io/nuget/v/GuedesPlace.AzureTools.svg)](https://www.nuget.org/packages/GuedesPlace.AzureTools)
[![NuGet Downloads](https://img.shields.io/nuget/dt/GuedesPlace.AzureTools.svg)](https://www.nuget.org/packages/GuedesPlace.AzureTools)
[![License: Apache-2.0](https://img.shields.io/badge/License-Apache--2.0-blue.svg)](LICENSE)
![.NET](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-512BD4)

A .NET helper library that wraps the official Azure SDK clients with fluent extension methods and higher-level typed abstractions. It reduces boilerplate when working with Azure Blob Storage, Queue Storage, Table Storage, and Azure Functions.

---

## Table of Contents

- [Installation](#installation)
- [Modules](#modules)
  - [Configuration](#configuration)
  - [Blob Storage](#blob-storage)
  - [Queue Storage](#queue-storage)
  - [Table Storage](#table-storage)
  - [Azure Functions — FunctionRunContext](#azure-functions--functionruncontext)
- [License](#license)

---

## Installation

```bash
dotnet add package GuedesPlace.AzureTools
```

Supports **.NET 8**, **.NET 9**, and **.NET 10**.

---

## Modules

### Configuration

Namespace: `GuedesPlace.AzureTools.Configuration.Extensions`

Guard your application startup against missing configuration values with two extension methods on `IConfiguration`:

```csharp
using GuedesPlace.AzureTools.Configuration.Extensions;

// Throws ArgumentNullException if "MyConnectionString" is null or empty
configuration.CheckConfigurationValueAvailable("MyConnectionString");

// Validate multiple keys at once
configuration.CheckConfigurationValuesAvailable(new[]
{
    "Azure:StorageConnection",
    "Azure:TableName",
    "ServiceBus:ConnectionString"
});
```

---

### Blob Storage

Namespace: `GuedesPlace.AzureTools.Blob.Extensions`

Extension methods on `BlobContainerClient` to store and retrieve any POCO as JSON without manually handling streams.

```csharp
using GuedesPlace.AzureTools.Blob.Extensions;

var container = new BlobContainerClient(connectionString, "my-container");

// Serialize and upload a POCO as JSON
await container.StoreAsJsonAsync("reports/2026-Q1", myReportObject);

// Download and deserialize back to T
Report? report = await container.RetreiveFromStoreAsync<Report>("reports/2026-Q1");
```

The blob name is `{filePathAndName}.json`. Content-Type is set to `application/json`.

---

### Queue Storage

Namespace: `GuedesPlace.AzureTools.Queue.Extensions`

Extension methods on `QueueClient` and `QueueMessage` to send and receive typed payloads without manual Base64/JSON encoding.

```csharp
using GuedesPlace.AzureTools.Queue.Extensions;

var queue = new QueueClient(connectionString, "my-queue");

// Send immediately
await queue.SendPayloadToQueueAsync(new OrderCreated { OrderId = 42 }, cancellationToken);

// Send with a visibility delay (e.g. retry in 30 seconds)
await queue.SendPayloadToQueueDelayed(payload, delaySeconds: 30, cancellationToken);

// Send with a custom time-to-live
await queue.SendPayloadToQueueWithDefinedLifeTime(payload, TimeSpan.FromHours(1), cancellationToken);
```

**Receiving:**

```csharp
QueueMessage message = ...; // from QueueClient.ReceiveMessageAsync()

// Deserialize the message body into a typed object
OrderCreated? order = message.DeserializeMessage<OrderCreated>();

// Or extract the raw text
string text = message.ExtractText();
```

---

### Table Storage

Namespace: `GuedesPlace.AzureTools.Tables`

Provides a typed ORM-like layer over Azure Table Storage. Nested POCOs, enums, arrays, `IEnumerable<T>`, and `TimeSpan` are automatically serialized/deserialized.

#### `TypedAzureTableClient<T>`

A strongly-typed wrapper for a single entity type per table.

```csharp
var tableClient = new TypedAzureTableClient<Product>(
    new TableClient(connectionString, "Products")
);

// Query all entities
List<TableEntityResult<Product>> all = await tableClient.GetAllAsync();

// Filter by partition key
List<TableEntityResult<Product>> byPartition = await tableClient.GetAllAsync("Electronics");

// Get by row key (uses typeof(T).ToString() as default partition key)
TableEntityResult<Product>? product = await tableClient.GetByIdAsync("product-123");

// Upsert (replace)
await tableClient.InsertOrReplaceAsync("product-123", "Electronics", myProduct);

// Delete
await tableClient.DeleteEntityAsync("product-123", "Electronics");
```

#### `MultiEntityAzureTableClient`

Stores multiple entity types in a **single Azure table** by prefixing row keys with a type identifier.

```csharp
var multiClient = new MultiEntityAzureTableClient(
    new TableClient(connectionString, "SharedTable")
);

multiClient.RegisterType<Order>();          // row key prefix: "Order"
multiClient.RegisterType<Customer>();       // row key prefix: "Customer"
multiClient.RegisterType<Invoice>("INV");   // custom prefix

// Upsert — prefix is added automatically
await multiClient.InsertOrReplaceAsync<Order>("order-99", "2026", new Order { ... });

// Row key stored in table: "Order_order-99"

// Retrieve — prefix handled transparently
TableEntityResult<Order>? result = await multiClient.GetByIdAsync<Order>("order-99", "2026");
```

#### `ExtendedAzureTableClientService`

A registry/factory that manages multiple table clients for an application, initialized from a single connection string.

```csharp
var service = new ExtendedAzureTableClientService(connectionString);

// Create table if not exists, register client
service.CreateAndRegisterTableClient<Product>("Products");
service.CreateAndRegisterTableClient<Customer>("Customers");

// Register a multi-entity client
service.CreateAndRegisterMultiEntityTableClient("SharedTable");

// Retrieve clients by type
TypedAzureTableClient<Product> products = service.GetTypedTableClient<Product>();
MultiEntityAzureTableClient shared = service.GetMultiEntityAzureTableClientByTableName("SharedTable");
```

#### Serialization Details

`ObjectSerializer` and `ObjectBuilder` handle automatic conversion:

| CLR Type | Table Storage Representation |
|---|---|
| Primitives, `string`, `byte[]` | Native |
| `enum` | String (name) |
| `TimeSpan` / `TimeSpan?` | String (`TimeSpan.ToString()`) |
| Arrays (non-`byte[]`) | JSON string |
| `IEnumerable<T>` | JSON string |
| Nested POCO | Flattened with `_` key separator (e.g. `Address_City`) |
| `DateTimeOffset` | Converted to UTC `DateTime` on read |

> **Note**: `ObjectBuilder` uses `RuntimeHelpers.GetUninitializedObject` — no constructor is invoked during deserialization. Ensure your entities do not rely on constructor-side effects.

#### `TableEntityResult<T>`

All query methods return `TableEntityResult<T>`, which bundles the deserialized entity with its table metadata:

```csharp
TableEntityResult<Product> result = ...;

string rowKey        = result.RowKey;
string partitionKey  = result.PartitionKey;
ETag etag            = result.ETag;
DateTimeOffset? ts   = result.Timestamp;
Product entity       = result.Entity;
```

---

### Azure Functions — FunctionRunContext

Namespace: `GuedesPlace.AzureTools.Functions.FunctionRunContext`

An abstraction layer for Azure Functions execution context that decouples your function code from raw `HttpRequest` parsing and Azure Easy Auth header decoding.

#### `IFunctionRunContext`

```csharp
public interface IFunctionRunContext
{
    FunctionRunContextType FunctionRunContextType { get; }
    string? GetEnvironmentVariable(string name);
    ValueTask<T?> GetPayLoad<T>();
    string? GetUserId();
    string? GetUPN();
    bool IsAuthenticated();
    bool IsDev();
    bool IsInAtLeastOneRole(params string[] rolesToCheck);
    bool IsInAllRoles(params string[] rolesToCheck);
}
```

#### `UserFunctionRunContext` — User HTTP Requests

Reads identity from Azure Easy Auth's `x-ms-client-principal` header (production) or from environment variables (development).

```csharp
[Function("MyFunction")]
public async Task<IActionResult> Run([HttpTrigger] HttpRequest req)
{
    IFunctionRunContext ctx = new UserFunctionRunContext(req);

    if (!ctx.IsAuthenticated())
        return new UnauthorizedResult();

    if (!ctx.IsInAtLeastOneRole("admin", "editor"))
        return new ForbidResult();

    var payload = await ctx.GetPayLoad<MyRequest>();
    string? userId = ctx.GetUserId();

    // ...
}
```

**Dev mode** (when `AZURE_FUNCTIONS_ENVIRONMENT == "Development"` and `IS_NOT_DEV` is absent):  
Set `DEV_USER_ID` and `DEV_USER_ROLES` (comma-separated) environment variables. Defaults to role `admin` if `DEV_USER_ROLES` is not set.

#### `AppFunctionRunContext` — App-to-App / Service Principal

For non-user callers where identity is already known (e.g., validated upstream by API Management):

```csharp
IFunctionRunContext ctx = new AppFunctionRunContext(
    applicationId: "my-service-principal-id",
    roles: new[] { "internal", "data-reader" }
);

bool canRead = ctx.IsInAtLeastOneRole("data-reader");
```

---

## License

Licensed under the [Apache License 2.0](LICENSE).
