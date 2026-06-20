using DILifetimes.Infrastructure.Data;
using DILifetimes.Services;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder();

builder.Services.AddSingleton<ISingletonMarker, Marker>();
builder.Services.AddScoped<IScopedMarker, Marker>();
builder.Services.AddTransient<ITransientMarker, Marker>();

builder.Services.AddSingleton<ICaptor, Captor>();

builder.Services.AddDbContext<OrderDbContext>(options =>
  options.UseInMemoryDatabase("Order"));

// By default, the captive dependency bug is silently working 
// because the app isn't running in the Development environment.
// Use builder.Host.UseDefaultServiceProvider(...) to explicitly force scope validation regardless of environment.
builder.Host.UseDefaultServiceProvider(options =>
{
  options.ValidateScopes = true;
  options.ValidateOnBuild = true;
});

WebApplication app = builder.Build();

// inject twice" trick — the endpoint must resolve each service a second time from IServiceProvider 
// within the same handler, then return all six IDs in the response body.
app.MapGet("/data", (ISingletonMarker singletonMarker, IScopedMarker scopedMarker, ITransientMarker transientMarker, IServiceProvider sp) =>
{
  ISingletonMarker singletonMarkerLocal = sp.GetRequiredService<ISingletonMarker>();
  IScopedMarker scopedMarkerLocal = sp.GetRequiredService<IScopedMarker>();
  ITransientMarker transientMarkerLocal = sp.GetRequiredService<ITransientMarker>();

  return TypedResults.Ok(
    new
    {
      SingletonInjected = singletonMarker.Id,
      ScopedInjected = scopedMarker.Id,
      TransientInjected = transientMarker.Id,
      SingletonLocal = singletonMarkerLocal.Id,
      ScopedLocal = scopedMarkerLocal.Id,
      TransientLocal = transientMarkerLocal.Id,
    }
  );
});

app.MapGet("/captive", (ICaptor captor) =>
{
  return TypedResults.Ok(
    new
    {
      CaptorInjected = captor.Id,
    }
  );
});

app.MapGet("/dbcontext", (OrderDbContext orderDbContext) =>
{
  return TypedResults.Ok(
    new
    {
      OrderDbContextId = orderDbContext.ContextId
    }
  );
});

app.Run();
