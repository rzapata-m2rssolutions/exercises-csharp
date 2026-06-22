using DomainDrivenDesign.Domain.Enums;

namespace DomainDrivenDesign.Domain.Entities;

public class StatusHistory : EntityBase
{
  public required Guid OrderId { get; init; }
  public required OrderStatus Status { get; init; }
}
