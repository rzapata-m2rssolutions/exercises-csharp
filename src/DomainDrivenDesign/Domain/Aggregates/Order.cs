using DomainDrivenDesign.Domain.Entities;
using DomainDrivenDesign.Domain.Enums;
using DomainDrivenDesign.Domain.ValueObjects;

namespace DomainDrivenDesign.Domain.Aggregates;

/// <summary>
/// Aggregate Root for the ordering domain.
/// All mutations to <see cref="OrderLine"/>s and order status must go
/// through this class — no external code may add, remove, or modify lines directly.
/// Invariants enforced here:
/// <list type="bullet">
///   <item>Lines can only be added or removed while the order is in <see cref="OrderStatus.Draft"/>.</item>
///   <item>Status transitions are one-way: Draft → Confirmed → Shipped (or Cancelled from either).</item>
///   <item><see cref="Total"/> is always consistent with the current set of lines.</item>
/// </list>
/// </summary>
public class Order : EntityBase
{
  private readonly List<OrderLine> _orderLines = [];
  private readonly List<StatusHistory> _statusHistories = [];

  private Order() { }

  public static Order Create()
  {
    Order order = new();
    order._statusHistories.Add(new StatusHistory
    {
      OrderId = order.Id,
      Status = OrderStatus.Draft,
    });
    return order;
  }

  public IReadOnlyCollection<OrderLine> OrderLines => _orderLines.AsReadOnly();
  public IReadOnlyCollection<StatusHistory> StatusHistories => _statusHistories.AsReadOnly();
  public OrderStatus Status
  {
    get;
    private set
    {
      _statusHistories.Add(new StatusHistory
      {
        OrderId = Id,
        Status = value,
      });
      field = value;
    }
  } = OrderStatus.Draft;
  public Money Total { get; private set; } = Money.Zero();

  public void AddLine(ProductSnapshot product, short quantity)
  {
    if (Status != OrderStatus.Draft)
      throw new InvalidOperationException($"You can only add lines when in Draft Status, current Status: {Status}");

    OrderLine lineToAdd = new()
    {
      OrderId = Id,
      ProductSnapshot = product,
      Quantity = quantity,
    };

    _orderLines.Add(lineToAdd);

    Total += lineToAdd.LineTotal;
  }

  public void RemoveLine(Guid orderLineId)
  {
    if (Status != OrderStatus.Draft)
      throw new InvalidOperationException($"You can only remove lines when in Draft Status, current Status: {Status}");

    OrderLine? lineToDelete = _orderLines.FirstOrDefault(ol => ol.Id == orderLineId)
      ?? throw new InvalidOperationException($"Order line {orderLineId} not found");
    Total -= lineToDelete.LineTotal;
    _orderLines.Remove(lineToDelete);
  }

  public void Confirm()
  {
    if (Status != OrderStatus.Draft)
      throw new InvalidOperationException($"Status transition Confirmed must be from Draft, tried from {Status}");

    Status = OrderStatus.Confirmed;
  }

  public void Ship()
  {
    if (Status != OrderStatus.Confirmed)
      throw new InvalidOperationException($"Status transition Shipped must be from Confirmed, tried from {Status}");

    Status = OrderStatus.Shipped;
  }

  public void Cancel()
  {
    if (Status is not OrderStatus.Draft and not OrderStatus.Confirmed)
      throw new InvalidOperationException($"Status transition Cancelled must be from Draft or Confirmed, tried from {Status}");

    Status = OrderStatus.Cancelled;
  }
}
