# DI Lifetimes

## Objective

Understand the core distinction between:

- **Singleton:** One instance for the entire application lifetime.
- **Scoped:** One instance per HTTP request (or per EF Core operation context).
- **Transient:** A new instance on every resolution.

Build a minimal API that registers the same interface three times under three lifetimes, resolve each twice per request, and observe the GUID behavior directly.

## Configuration

- [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- Default port: `http://localhost:5000`

## How to run

```bash
dotnet watch run --project src/DILifetimes
```

## How to test

Hit the endpoint twice and compare the responses:

```powershell
Invoke-RestMethod http://localhost:5000/data
Invoke-RestMethod http://localhost:5000/captive
Invoke-RestMethod http://localhost:5000/dbcontext
```

```bash
curl -s http://localhost:5000/data | jq
curl -s http://localhost:5000/captive | jq
curl -s http://localhost:5000/dbcontext | jq
```

Expected behavior:

- `singletonInjected` and `singletonLocal` are identical across **both** calls.
- `scopedInjected` matches `scopedLocal` within one call, but changes between calls.
- All four transient IDs (`transientInjected`, `transientLocal` across both calls) are different every time.

## Exercises

### Prove the three lifetimes with a GUID

**What I built:**

A parent interface `IMarker`, three child interfaces each representing one DI lifetime (`ISingletonMarker`, `IScopedMarker`, `ITransientMarker`), and one `Marker` implementation class used for all three registrations. A single `GET /data` endpoint resolves each marker twice — once via parameter injection and once via `IServiceProvider` — and returns all six IDs.

**What I observed:**

| Lifetime  | Same within request? | Same across requests? |
| --------- | -------------------- | --------------------- |
| Singleton | Yes                  | Yes                   |
| Scoped    | Yes                  | No                    |
| Transient | No                   | No                    |

**In my own words:**

A singleton is created once and reused for the entire lifetime of the application. A scoped instance is created once per HTTP request, so all resolutions within the same request share the same object. A transient is created fresh on every resolution — even two resolutions within the same request return different instances.

### Reproduce the captive dependency bug

**What I built:**

An `ICaptor` singleton whose `Captor` implementation takes `IScopedMarker` via constructor injection. A `GET /captive` endpoint returns `ICaptor.Id` so the captured GUID is visible across requests.

Scope validation was explicitly enabled via `builder.Host.UseDefaultServiceProvider(options => { options.ValidateScopes = true; options.ValidateOnBuild = true; })` — required because the app was not running in the Development environment.

**The error ASP.NET Core throws:**

```text
Some services are not able to be constructed (Error while validating the service descriptor
'ServiceType: DILifetimes.Services.ICaptor Lifetime: Singleton ImplementationType:
DILifetimes.Services.Captor': Cannot consume scoped service
'DILifetimes.Services.IScopedMarker' from singleton 'DILifetimes.Services.ICaptor'.)
```

**Why scope validation exists:**

It prevents a scoped service from being trapped inside a singleton, which would make the scoped service behave like a singleton and silently defeat its intended per-request lifetime.

**The silent failure mode:**

When bypassed via `IServiceProvider.GetRequiredService` inside the singleton's constructor, the app starts without error but the scoped instance is resolved once from the root provider and frozen forever — `captorInjected` returns the same GUID on every request regardless of how many requests are made.

### DbContext lifetime in practice

**Why injecting DbContext into a Singleton is dangerous:**

At scope which is the normal behavior, requests get their own `DbContext` instance with its own isolated change tracker, as we can see by the changing `OrderDbContext.ContextId` when we hit `/dbcontext`.

On the contrary, a Singleton is dangerous because:

1. `DbContext` is not **thread-safe**, concurrent requests sharing one singleton instance will corrupt the change tracker or throw concurrency exceptions.
2. Keep the connection open so we risk of connection pool exhaustion.

**How I'd handle a multi-tenant DbContext per request:**

We register `OrderDbContext` itself as a scoped service, `AddDbContext<T>` registers `T` as scoped by default, which means each HTTP request gets its own instance.

For multi-tenant scenarios we can register `OrderDbContext` using `AddDbContext` overload that provides `IServiceProvider` and internally int the action use `DbContextOptionsBuilder`, from here we can resolve `IHttpContextAccessor`, read a tenant identifier from the current request (header, claim, subdomain), and use it to pick the right connection string, all inside that single scoped options lambda. One `OrderDbContext` registration, N tenants handled at runtime.
