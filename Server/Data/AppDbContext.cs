using Microsoft.EntityFrameworkCore;
using Server.Data.Entities;

namespace Server.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<LoginBonus> LoginBonuses => Set<LoginBonus>();
    public DbSet<LoginBonusDay> LoginBonusDays => Set<LoginBonusDay>();
    public DbSet<LoginBonusDayReward> LoginBonusDayRewards => Set<LoginBonusDayReward>();
    public DbSet<AccountLoginBonus> AccountLoginBonuses => Set<AccountLoginBonus>();
    public DbSet<AccountLoginBonusLog> AccountLoginBonusLogs => Set<AccountLoginBonusLog>();
    public DbSet<Reward> Rewards => Set<Reward>();
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

        modelBuilder.Entity<LoginBonus>(entity =>
        {
            entity.ToTable("login_bonus");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.StartDate).HasColumnName("start_date").IsRequired();
            entity.Property(e => e.EndDate).HasColumnName("end_date").IsRequired();
            entity.Property(e => e.Type).HasColumnName("type").IsRequired();
        });

        modelBuilder.Entity<LoginBonusDay>(entity =>
        {
            entity.ToTable("login_bonus_day");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LoginBonusId).HasColumnName("login_bonus_id").IsRequired();
            entity.Property(e => e.Date).HasColumnName("date").IsRequired();
            entity.HasOne(e => e.LoginBonus)
                .WithMany(e => e.Days)
                .HasForeignKey(e => e.LoginBonusId);
            entity.HasIndex(e => e.LoginBonusId).HasDatabaseName("idx_login_bonus_day_login_bonus_id");
        });

        modelBuilder.Entity<LoginBonusDayReward>(entity =>
        {
            entity.ToTable("login_bonus_day_reward");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LoginBonusDayId).HasColumnName("login_bonus_day_id").IsRequired();
            entity.Property(e => e.RewardId).HasColumnName("reward_id").IsRequired();
            entity.HasOne(e => e.LoginBonusDay)
                .WithMany(e => e.Rewards)
                .HasForeignKey(e => e.LoginBonusDayId);
            entity.HasOne(e => e.Reward)
                .WithMany()
                .HasForeignKey(e => e.RewardId);
            entity.HasIndex(e => e.LoginBonusDayId).HasDatabaseName("idx_login_bonus_day_reward_login_bonus_day_id");
        });

        modelBuilder.Entity<AccountLoginBonus>(entity =>
        {
            entity.ToTable("account_login_bonus");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.LoginBonusId).HasColumnName("login_bonus_id").IsRequired();
            entity.Property(e => e.ClaimCount).HasColumnName("claim_count").IsRequired();
            entity.Property(e => e.LastClaimedDay).HasColumnName("last_claimed_day");
            entity.HasIndex(e => new { e.AccountId, e.LoginBonusId }).IsUnique();
            entity.HasOne(e => e.Account)
                .WithMany(e => e.LoginBonuses)
                .HasForeignKey(e => e.AccountId);
            entity.HasOne(e => e.LoginBonus)
                .WithMany()
                .HasForeignKey(e => e.LoginBonusId);
            entity.HasIndex(e => e.AccountId).HasDatabaseName("idx_account_login_bonus_account_id");
            entity.HasIndex(e => e.LoginBonusId).HasDatabaseName("idx_account_login_bonus_login_bonus_id");
        });

        modelBuilder.Entity<AccountLoginBonusLog>(entity =>
        {
            entity.ToTable("account_login_bonus_log");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.LoginBonusId).HasColumnName("login_bonus_id").IsRequired();
            entity.Property(e => e.LoginBonusDayId).HasColumnName("login_bonus_day_id").IsRequired();
            entity.Property(e => e.ClaimCount).HasColumnName("claim_count").IsRequired();
            entity.Property(e => e.ClaimedAt).HasColumnName("claimed_at").IsRequired();
            entity.HasIndex(e => new { e.AccountId, e.LoginBonusDayId }).IsUnique();
            entity.HasOne(e => e.Account)
                .WithMany(e => e.LoginBonusLogs)
                .HasForeignKey(e => e.AccountId);
            entity.HasOne(e => e.LoginBonus)
                .WithMany()
                .HasForeignKey(e => e.LoginBonusId);
            entity.HasOne(e => e.LoginBonusDay)
                .WithMany()
                .HasForeignKey(e => e.LoginBonusDayId);
            entity.HasIndex(e => e.AccountId).HasDatabaseName("idx_account_login_bonus_log_account_id");
        });

        modelBuilder.Entity<Reward>(entity =>
        {
            entity.ToTable("reward");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Type).HasColumnName("type").IsRequired();
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();
            entity.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId);
            entity.HasIndex(e => e.Type).HasDatabaseName("idx_reward_type");
            entity.HasIndex(e => e.ItemId).HasDatabaseName("idx_reward_item_id");
        });

        modelBuilder.Entity<ItemMaster>(entity =>
        {
            entity.ToTable("item_master");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
        });
    }
}
