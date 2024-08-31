using distributedDeliveryBackend.Dto;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace distributedDeliveryBackend;

public class ApplicationDbSqlContext : DbContext
{
    public ApplicationDbSqlContext(DbContextOptions<ApplicationDbSqlContext> options)
        : base(options)
    {
    }

    public DbSet<OrderDb> orderDb { get; set; }
    public DbSet<RiderDb> riderDb { get; set; }
}