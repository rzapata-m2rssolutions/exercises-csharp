using DomainDrivenDesign.Domain.ValueObjects;

namespace DomainDrivenDesign.Domain.Entities;

public class OrderLine : EntityBase
{
  public required Guid OrderId { get; init; }
  public required ProductSnapshot ProductSnapshot { get; init; }

  public short Quantity
  {
    get;
    init
    {
      if (value <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(value), "Must be a positive number.");
      }
      field = value;
    }
  }

  public Money LineTotal => ProductSnapshot.Price * Quantity;
}
