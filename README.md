# PipelineExtensions.EntityFrameworkCore

A library for extending the query execution process in Entity Framework Core using pipelines. It allows intercepting and modifying query expressions during their compilation and execution.

## Features

- Interception of synchronous and asynchronous queries.
- Support for `IAsyncEnumerable` and `IEnumerable`.
- Ability to inject custom logic into the query compilation process.
- Ability to inject custom logic before caching by expression tree.
- Ability to access to result without reflection.
- Support for multiple EF Core versions (3.1, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0).

## Installation

add `PipelineExtensions.EntityFrameworkCore` nuget package for your version of Entity Framework Core to your project.

## Usage

### 1. Implementation of IPipelineQueryCompiler

Create a class that implements the `IPipelineQueryCompiler` interface. This class will contain the query processing logic.

```csharp
public class MyQueryCompiler : IPipelineQueryCompiler
{
    public T Execute<T>(Expression query, Func<Expression, T> next)
    {
        // Logic before query execution
        var result = next(query);
        // Logic after query execution
        return result;
    }

    public async Task<T> ExecuteAsyncTask<T>(Expression query, CancellationToken cancellationToken, Func<Expression, CancellationToken, Task<T>> next)
    {
        // Asynchronous logic
        return await next(query, cancellationToken);
    }

    // Implement other interface methods...
}
```

### 2. Registration in DbContext

Use the `AddQueryCompiler` extension method when configuring `DbContextOptions`.

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseSqlServer("YourConnectionString")
        .AddQueryCompiler(() => new MyQueryCompiler());
}
```

Or when registering via Dependency Injection:

```csharp
services.AddDbContext<MyDbContext>(options =>
    options.UseSqlite("Data Source=mydb.db")
           .AddQueryCompiler(() => new MyQueryCompiler()));
```

## How It Works

The library replaces the standard `IQueryCompiler` in EF Core with `PipelineQueryCompiler`, which passes all queries through a chain of registered `IPipelineQueryCompiler` instances. This allows adding features such as logging, caching, auditing, or automatic expression tree modification without changing the query code itself.

## License

The project is distributed under the MIT License.