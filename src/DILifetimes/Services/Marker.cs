namespace DILifetimes.Services;

public class Marker : ISingletonMarker, IScopedMarker, ITransientMarker
{
  public Guid Id => Guid.CreateVersion7();
}
