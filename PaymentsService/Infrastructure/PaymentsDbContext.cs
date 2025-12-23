using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain;
using PaymentsService.Infrastructure.Inbox;
using PaymentsService.Infrastructure.Outbox;

namespace PaymentsService.Infrastructure;

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.UserId).IsUnique();
            entity.Property(a => a.Balance).IsRequired();
            entity.Property(a => a.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Type).IsRequired();
            entity.Property(i => i.ReceivedAt).IsRequired();
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Type).IsRequired();
            entity.Property(o => o.Payload).IsRequired();
            entity.Property(o => o.OccurredAt).IsRequired();
        });
    }
}
