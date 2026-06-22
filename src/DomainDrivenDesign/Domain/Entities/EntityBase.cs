namespace DomainDrivenDesign.Domain.Entities;

public abstract class EntityBase
{
  public Guid Id { get; init; } = Guid.CreateVersion7();

  public bool Deleted { get; private set; }
  public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
  public DateTimeOffset LastModifiedAt { get; internal set; }

  public void MarkAsDeleted() => Deleted = true;
  public void RestoreEntity() => Deleted = false;
}
