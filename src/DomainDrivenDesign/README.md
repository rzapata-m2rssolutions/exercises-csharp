# Domain Driven Design

## Objective

The goal is to map real architecture to the vocabulary, so the next interview answer is concrete and experience-backed rather than textbook.

## Configuration

- [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

## How to run

```bash
dotnet run --project src/DomainDrivenDesign
```

## Exercises

### Aggregates and Aggregate Root

**Domain:** `Order` → `OrderLine` → `ProductSnapshot`

An **Aggregate** is a cluster of domain objects treated as a single unit of consistency. The **Aggregate Root** (`Order`) is the only entry point — no external code touches `OrderLine` or `StatusHistory` directly.

#### Key decisions and why

**Private backing field + `IReadOnlyCollection`**
`_orderLines` is a `List<OrderLine>` exposed as `IReadOnlyCollection<OrderLine>`. Callers can read lines but cannot add, remove, or replace them without going through `Order`'s methods. `ICollection` with a public setter would break aggregate encapsulation.

**`Order.Create()` factory method + private constructor**
The constructor is private so no caller can bypass the factory. `Create()` is the only sanctioned way to instantiate an `Order`, guaranteeing the initial `Draft` status is always recorded in `StatusHistory`. With a public constructor any caller could skip that step. EF Core materializes existing orders using the private constructor via reflection — no Draft entry is added on load because the DB rows already carry it.

**`ProductSnapshot` as a Value Object, not an Entity reference**
`OrderLine` does not hold a navigation to a live `Product`. It captures a `ProductSnapshot` (name + price + original product ID) at order time. If the product price changes later, the order total is unaffected. This also respects the bounded context boundary — the Catalog context owns `Product`; the Order context only keeps what it needed at the moment of purchase.

**Value Object equality via `GetEqualityComponents()`**
`ValueObject` base class implements `Equals` / `GetHashCode` / `==` / `!=` through an abstract `GetEqualityComponents()`. Two `ProductSnapshot`s with the same `ProductId`, `Name`, and `Price` are equal regardless of reference. VOs use `required init` properties — immutable after construction.

**`Money` as a Value Object instead of `decimal`**
Arithmetic operators are defined as `Money * int` and `Money * decimal` — not `Money * Money`, because multiplying two monetary amounts has no real business meaning. Currency mismatches throw at runtime rather than silently producing a wrong result.

**Status transitions as guarded methods**
`Confirm()`, `Ship()`, and `Cancel()` each validate `Status` before transitioning and throw `InvalidOperationException` on invalid calls. The enforced flow is `Draft → Confirmed → Shipped`, with `Cancelled` reachable from `Draft` or `Confirmed` only.

**`StatusHistory` auto-recorded in the `Status` setter**
Using C# 13's `field` keyword, the `Status` property setter appends a `StatusHistory` entry on every transition, recording the *to-state* (not the from-state) so history reads as a timeline of what the order became. `Draft` is recorded by `Order.Create()` because property initializers bypass setters.

**`LastModifiedAt` stamped at the infrastructure layer**
`EntityBase.LastModifiedAt` has `internal set`. The domain model never calls a `MarkModified()` method. Instead, `OrderDbContext.SaveChanges()` stamps it on every `EntityState.Modified` entry via the change tracker. Audit concerns stay out of the domain model entirely.

**Only `Order` is a `DbSet<>`**
`OrderLine` and `StatusHistory` are not registered as `DbSet<>` entries. They are queried exclusively through `context.Orders.Include(o => o.OrderLines)`. Exposing them independently would let callers bypass the aggregate root.

**`OnModelCreating` without reverse navigation**
`OrderLine` has no navigation property back to `Order` — only `OrderId` (FK). EF is configured with `.WithOne()` (no nav) and `.HasForeignKey(ol => ol.OrderId)`. Children do not reference their root; the root holds its children.

### Identify bounded contexts

### Anemic vs. behavior-rich domain model
