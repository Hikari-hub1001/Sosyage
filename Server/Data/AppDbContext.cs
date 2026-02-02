using Microsoft.EntityFrameworkCore;
using Server.Data.Entities;

namespace Server.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<LoginBonusMonth> LoginBonusMonths => Set<LoginBonusMonth>();
    public DbSet<LoginBonusDay> LoginBonusDays => Set<LoginBonusDay>();
    public DbSet<LoginBonusDayReward> LoginBonusDayRewards => Set<LoginBonusDayReward>();
    public DbSet<AccountLoginBonus> AccountLoginBonuses => Set<AccountLoginBonus>();
    public DbSet<AccountLoginBonusLog> AccountLoginBonusLogs => Set<AccountLoginBonusLog>();
    public DbSet<ItemMaster> ItemMasters => Set<ItemMaster>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("account");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at").IsRequired();
        });

        modelBuilder.Entity<LoginBonusMonth>(entity =>
        {
            entity.ToTable("login_bonus_month");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Month).HasColumnName("month").IsRequired();
            entity.Property(e => e.StartDate).HasColumnName("start_date").IsRequired();
            entity.Property(e => e.EndDate).HasColumnName("end_date").IsRequired();
            entity.HasIndex(e => e.Month).IsUnique();
        });

        modelBuilder.Entity<LoginBonusDay>(entity =>
        {
            entity.ToTable("login_bonus_day");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MonthId).HasColumnName("month_id").IsRequired();
            entity.Property(e => e.DayNumber).HasColumnName("day_number").IsRequired();
            entity.HasOne(e => e.Month)
                .WithMany(e => e.Days)
                .HasForeignKey(e => e.MonthId);
            entity.HasIndex(e => e.MonthId).HasDatabaseName("idx_login_bonus_day_month_id");
        });

        modelBuilder.Entity<LoginBonusDayReward>(entity =>
        {
            entity.ToTable("login_bonus_day_reward");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DayId).HasColumnName("day_id").IsRequired();
            entity.Property(e => e.RewardId).HasColumnName("reward_id").IsRequired();
            entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();
            entity.HasOne(e => e.Day)
                .WithMany(e => e.Rewards)
                .HasForeignKey(e => e.DayId);
            entity.HasIndex(e => e.DayId).HasDatabaseName("idx_login_bonus_day_reward_day_id");
        });

        modelBuilder.Entity<AccountLoginBonus>(entity =>
        {
            entity.ToTable("account_login_bonus");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.MonthId).HasColumnName("month_id").IsRequired();
            entity.Property(e => e.CurrentDay).HasColumnName("current_day").IsRequired();
            entity.Property(e => e.LastClaimedDay).HasColumnName("last_claimed_day");
            entity.HasIndex(e => new { e.AccountId, e.MonthId }).IsUnique();
            entity.HasOne(e => e.Account)
                .WithMany(e => e.LoginBonuses)
                .HasForeignKey(e => e.AccountId);
            entity.HasOne(e => e.Month)
                .WithMany()
                .HasForeignKey(e => e.MonthId);
            entity.HasIndex(e => e.AccountId).HasDatabaseName("idx_account_login_bonus_account_id");
        });

        modelBuilder.Entity<AccountLoginBonusLog>(entity =>
        {
            entity.ToTable("account_login_bonus_log");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.MonthId).HasColumnName("month_id").IsRequired();
            entity.Property(e => e.DayNumber).HasColumnName("day_number").IsRequired();
            entity.Property(e => e.ClaimedAt).HasColumnName("claimed_at").IsRequired();
            entity.HasIndex(e => new { e.AccountId, e.MonthId, e.DayNumber }).IsUnique();
            entity.HasOne(e => e.Account)
                .WithMany(e => e.LoginBonusLogs)
                .HasForeignKey(e => e.AccountId);
            entity.HasOne(e => e.Month)
                .WithMany()
                .HasForeignKey(e => e.MonthId);
            entity.HasIndex(e => e.AccountId).HasDatabaseName("idx_account_login_bonus_log_account_id");
        });

        modelBuilder.Entity<ItemMaster>(entity =>
        {
            entity.ToTable("item_master");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.ItemType).HasColumnName("item_type").IsRequired();
        });
    }
}
