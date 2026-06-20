using DILifetimes.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DILifetimes.Infrastructure.Data;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
  public DbSet<Order> Orders => Set<Order>();
}
