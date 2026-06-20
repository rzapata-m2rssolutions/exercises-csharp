namespace DILifetimes.Services;

public class Captor : ICaptor
{
  public Guid Id { get; }

  public Captor(IServiceProvider serviceProvider)
  {
    // `Captor` to bypass scope validation when injecting IScopeMarker directly
    // BUT it will throw an error when Captor is invoked
    // Silent failure: Id is captured once at construction and frozen forever.
    // The proper fix is to inject IServiceScopeFactory and create a scope per invocation.
    IScopedMarker marker = serviceProvider.GetRequiredService<IScopedMarker>();
    Id = marker.Id;
  }
}
