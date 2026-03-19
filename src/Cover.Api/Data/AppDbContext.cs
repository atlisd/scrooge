using Microsoft.EntityFrameworkCore;
using Cover.Api.Models;

namespace Cover.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<AppCredentials> AppCredentials => Set<AppCredentials>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppCredentials>(entity =>
        {
            entity.Property(c => c.Username).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Name).HasMaxLength(100).IsRequired();
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.ToTable(t => t.HasCheckConstraint("CK_Expense_Amount_Positive", "\"Amount\" > 0"));
            entity.Property(e => e.Description).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Amount).IsRequired();
            entity.Property(e => e.SplitType).HasDefaultValue(Shared.DTOs.SplitType.Equal);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.HasIndex(e => e.Date);
            entity.HasOne(e => e.PaidBy).WithMany().HasForeignKey(e => e.PaidById);
        });
    }
}
