namespace DomainDrivenDesign.Domain.ValueObjects;

public sealed class ProductSnapshot : ValueObject
{
  public required Guid ProductId { get; init; }
  public required string Name { get; init; }
  public required Money Price { get; init; }

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return ProductId;
    yield return Name;
    yield return Price;
  }
}
