using System.Text.Json.Serialization;

namespace DomainDrivenDesign.Domain.ValueObjects;

/// <summary>
/// Value Object representing monetary amounts with currency consistency.
/// Negative amounts are allowed to support calculations like debt/balance.
/// </summary>
public sealed class Money : ValueObject
{
  public decimal Amount { get; init; }

  /// <summary>
  /// Currency code (placeholder for future multi-currency support).
  /// Not persisted to database and excluded from JSON serialization.
  /// </summary>
  [JsonIgnore]
  public string Currency { get; private set; } = "USD";

  /// <summary>
  /// Required by EF Core for materialization and JSON deserialization. Currency defaults to USD.
  /// </summary>
  [JsonConstructor]
  public Money() { }

  private Money(decimal amount, string currency = "USD")
  {
    if (string.IsNullOrWhiteSpace(currency))
      throw new ArgumentException("Currency cannot be empty", nameof(currency));

    Amount = amount;
    Currency = currency.ToUpperInvariant();
  }

  public static Money Create(decimal amount, string currency = "USD")
      => new(amount, currency);

  public static Money Zero() => new(0, "USD");

  /// <summary>
  /// Returns the money value or Zero if null.
  /// </summary>
  public static Money OrZero(Money? value) => value ?? new Money(0, "USD");

  /// <summary>
  /// Gets the amount value or zero if null. For backward compatibility with float? patterns.
  /// </summary>
  public decimal GetValueOrDefault() => Amount;

  // Arithmetic operators with currency enforcement

  public static Money operator +(Money left, Money right)
  {
    if (left.Currency != right.Currency)
      throw new InvalidOperationException($"Cannot add {right.Currency} to {left.Currency}");
    return new Money(left.Amount + right.Amount, left.Currency);
  }

  public static Money operator -(Money left, Money right)
  {
    if (left.Currency != right.Currency)
      throw new InvalidOperationException($"Cannot subtract {right.Currency} from {left.Currency}");
    return new Money(left.Amount - right.Amount, left.Currency);
  }

  public static Money operator *(Money money, int quantity)
  {
    return new(money.Amount * quantity, money.Currency);
  }

  public static Money operator *(int quantity, Money money)
  {
    return money * quantity;
  }

  public static Money operator *(Money money, decimal scalar)
  {
    return new(money.Amount * scalar, money.Currency);
  }

  public static Money operator *(decimal scalar, Money money)
  {
    return money * scalar;
  }

  public static Money operator /(Money money, int quantity)
  {
    if (quantity == 0)
      throw new DivideByZeroException("Cannot divide a monetary amount by zero.");
    return new(money.Amount / quantity, money.Currency);
  }

  public static Money operator /(Money money, decimal scalar)
  {
    if (scalar == 0)
      throw new DivideByZeroException("Cannot divide a monetary amount by zero.");
    return new(money.Amount / scalar, money.Currency);
  }

  // Comparison operators with currency enforcement

  public static bool operator >(Money left, Money right)
  {
    if (left.Currency != right.Currency)
      throw new InvalidOperationException($"Cannot compare {right.Currency} to {left.Currency}");
    return left.Amount > right.Amount;
  }

  public static bool operator <(Money left, Money right)
  {
    if (left.Currency != right.Currency)
      throw new InvalidOperationException($"Cannot compare {right.Currency} to {left.Currency}");
    return left.Amount < right.Amount;
  }

  public static bool operator >=(Money left, Money right)
  {
    if (left.Currency != right.Currency)
      throw new InvalidOperationException($"Cannot compare {right.Currency} to {left.Currency}");
    return left.Amount >= right.Amount;
  }

  public static bool operator <=(Money left, Money right)
  {
    if (left.Currency != right.Currency)
      throw new InvalidOperationException($"Cannot compare {right.Currency} to {left.Currency}");
    return left.Amount <= right.Amount;
  }

  // Comparison with int (for > 0, <= 0 patterns)

  public static bool operator >(Money? left, int right)
  {
    return left != null && left.Amount > right;
  }

  public static bool operator <(Money? left, int right)
  {
    return left == null || left.Amount < right;
  }

  public static bool operator >=(Money? left, int right)
  {
    return left != null && left.Amount >= right;
  }

  public static bool operator <=(Money? left, int right)
  {
    return left == null || left.Amount <= right;
  }

  public static bool operator ==(Money? left, int right)
  {
    return left != null && left.Amount == right;
  }

  public static bool operator !=(Money? left, int right)
  {
    return left == null || left.Amount != right;
  }

  protected override IEnumerable<object?> GetEqualityComponents()
  {
    yield return Amount;
    yield return Currency;
  }

  public override bool Equals(object? obj) => base.Equals(obj);
  public override int GetHashCode() => base.GetHashCode();

  public override string ToString() => $"{Amount:C} {Currency}";

  // Convenience conversion to float for backward compatibility with existing database
  public float ToFloat() => (float)Amount;

  public static Money FromFloat(float amount, string currency = "USD")
      => new((decimal)amount, currency);

}
