using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ArtisanHubs.Data.Entities;

public partial class ArtisanHubsDbContext : DbContext
{
    public ArtisanHubsDbContext()
    {
    }

    public ArtisanHubsDbContext(DbContextOptions<ArtisanHubsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Artistprofile> Artistprofiles { get; set; }

    public virtual DbSet<Artistwallet> Artistwallets { get; set; }

    public virtual DbSet<Artistworkshop> Artistworkshops { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Commission> Commissions { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Orderdetail> Orderdetails { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Wallettransaction> Wallettransactions { get; set; }

    public virtual DbSet<Withdrawrequest> Withdrawrequests { get; set; }

    public virtual DbSet<Workshoppackage> Workshoppackages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("account_pkey");

            entity.ToTable("account");

            entity.HasIndex(e => e.Email, "account_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "account_username_key").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .HasColumnName("avatar");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Artistprofile>(entity =>
        {
            entity.HasKey(e => e.ArtistId).HasName("artistprofile_pkey");

            entity.ToTable("artistprofile");

            entity.HasIndex(e => e.AccountId, "artistprofile_account_id_idx");

            entity.HasIndex(e => e.AccountId, "artistprofile_account_id_key").IsUnique();

            entity.Property(e => e.ArtistId).HasColumnName("artist_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.ArtistName)
                .HasMaxLength(255)
                .HasColumnName("artist_name");
            entity.Property(e => e.AvgRating)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("0.00")
                .HasColumnName("avg_rating");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.ProfileImage)
                .HasMaxLength(255)
                .HasColumnName("profile_image");
            entity.Property(e => e.ShopName)
                .HasMaxLength(255)
                .HasColumnName("shop_name");
            entity.Property(e => e.SocialLinks).HasColumnName("social_links");
            entity.Property(e => e.VerifiedStatus)
                .HasMaxLength(50)
                .HasDefaultValueSql("'unverified'::character varying")
                .HasColumnName("verified_status");

            entity.HasOne(d => d.Account).WithOne(p => p.Artistprofile)
                .HasForeignKey<Artistprofile>(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("artistprofile_account_id_fkey");
        });

        modelBuilder.Entity<Artistwallet>(entity =>
        {
            entity.HasKey(e => e.WalletId).HasName("artistwallet_pkey");

            entity.ToTable("artistwallet");

            entity.HasIndex(e => e.ArtistId, "artistwallet_artist_id_idx");

            entity.HasIndex(e => e.ArtistId, "artistwallet_artist_id_key").IsUnique();

            entity.Property(e => e.WalletId).HasColumnName("wallet_id");
            entity.Property(e => e.ArtistId).HasColumnName("artist_id");
            entity.Property(e => e.Balance)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0.00")
                .HasColumnName("balance");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.PendingBalance)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0.00")
                .HasColumnName("pending_balance");

            entity.HasOne(d => d.Artist).WithOne(p => p.Artistwallet)
                .HasForeignKey<Artistwallet>(d => d.ArtistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("artistwallet_artist_id_fkey");
        });

        modelBuilder.Entity<Artistworkshop>(entity =>
        {
            entity.HasKey(e => e.WorkshopId).HasName("artistworkshop_pkey");

            entity.ToTable("artistworkshop");

            entity.HasIndex(e => e.ArtistId, "artistworkshop_artist_id_idx");

            entity.HasIndex(e => e.PackageId, "artistworkshop_package_id_idx");

            entity.Property(e => e.WorkshopId).HasColumnName("workshop_id");
            entity.Property(e => e.ArtistId).HasColumnName("artist_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.Revenue)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0.00")
                .HasColumnName("revenue");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'scheduled'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Topic)
                .HasMaxLength(255)
                .HasColumnName("topic");
            entity.Property(e => e.ViewerCount)
                .HasDefaultValue(0)
                .HasColumnName("viewer_count");

            entity.HasOne(d => d.Artist).WithMany(p => p.Artistworkshops)
                .HasForeignKey(d => d.ArtistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("artistworkshop_artist_id_fkey");

            entity.HasOne(d => d.Package).WithMany(p => p.Artistworkshops)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("artistworkshop_package_id_fkey");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("category_pkey");

            entity.ToTable("category");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("category_parent_id_fkey");
        });

        modelBuilder.Entity<Commission>(entity =>
        {
            entity.HasKey(e => e.CommissionId).HasName("commission_pkey");

            entity.ToTable("commission");

            entity.HasIndex(e => e.ArtistId, "commission_artist_id_idx");

            entity.HasIndex(e => e.OrderId, "commission_order_id_idx");

            entity.Property(e => e.CommissionId).HasColumnName("commission_id");
            entity.Property(e => e.AdminShare)
                .HasPrecision(12, 2)
                .HasColumnName("admin_share");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.ArtistId).HasColumnName("artist_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Rate)
                .HasPrecision(5, 2)
                .HasColumnName("rate");
            entity.Property(e => e.WorkshopId).HasColumnName("workshop_id");

            entity.HasOne(d => d.Artist).WithMany(p => p.Commissions)
                .HasForeignKey(d => d.ArtistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("commission_artist_id_fkey");

            entity.HasOne(d => d.Order).WithMany(p => p.Commissions)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("commission_order_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.Commissions)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("commission_product_id_fkey");

            entity.HasOne(d => d.Workshop).WithMany(p => p.Commissions)
                .HasForeignKey(d => d.WorkshopId)
                .HasConstraintName("commission_workshop_id_fkey");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("feedback_pkey");

            entity.ToTable("feedback");

            entity.HasIndex(e => e.AccountId, "feedback_account_id_idx");

            entity.HasIndex(e => e.ProductId, "feedback_product_id_idx");

            entity.HasIndex(e => e.WorkshopId, "feedback_workshop_id_idx");

            entity.Property(e => e.FeedbackId).HasColumnName("feedback_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.WorkshopId).HasColumnName("workshop_id");

            entity.HasOne(d => d.Account).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("feedback_account_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("feedback_product_id_fkey");

            entity.HasOne(d => d.Workshop).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.WorkshopId)
                .HasConstraintName("feedback_workshop_id_fkey");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("Order_pkey");

            entity.ToTable("Order");

            entity.HasIndex(e => e.AccountId, "Order_account_id_idx");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("order_date");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.ShippingAddress).HasColumnName("shipping_address");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Account).WithMany(p => p.Orders)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Order_account_id_fkey");
        });

        modelBuilder.Entity<Orderdetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("orderdetail_pkey");

            entity.ToTable("orderdetail");

            entity.HasIndex(e => e.OrderId, "orderdetail_order_id_idx");

            entity.HasIndex(e => e.ProductId, "orderdetail_product_id_idx");

            entity.Property(e => e.OrderDetailId).HasColumnName("order_detail_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(12, 2)
                .HasColumnName("total_price");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(10, 2)
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Order).WithMany(p => p.Orderdetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orderdetail_order_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.Orderdetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orderdetail_product_id_fkey");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("payment_pkey");

            entity.ToTable("payment");

            entity.HasIndex(e => e.OrderId, "payment_order_id_idx");

            entity.HasIndex(e => e.WorkshopId, "payment_workshop_id_idx");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Method)
                .HasMaxLength(50)
                .HasColumnName("method");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("payment_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'completed'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.WorkshopId).HasColumnName("workshop_id");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("payment_order_id_fkey");

            entity.HasOne(d => d.Workshop).WithMany(p => p.Payments)
                .HasForeignKey(d => d.WorkshopId)
                .HasConstraintName("payment_workshop_id_fkey");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("product_pkey");

            entity.ToTable("product");

            entity.HasIndex(e => e.ArtistId, "product_artist_id_idx");

            entity.HasIndex(e => e.CategoryId, "product_category_id_idx");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ArtistId).HasColumnName("artist_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountPrice)
                .HasPrecision(10, 2)
                .HasColumnName("discount_price");
            entity.Property(e => e.Images).HasColumnName("images");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'available'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.StockQuantity)
                .HasDefaultValue(0)
                .HasColumnName("stock_quantity");
            entity.Property(e => e.Story).HasColumnName("story");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Artist).WithMany(p => p.Products)
                .HasForeignKey(d => d.ArtistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("product_artist_id_fkey");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("product_category_id_fkey");
        });

        modelBuilder.Entity<Wallettransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("wallettransaction_pkey");

            entity.ToTable("wallettransaction");

            entity.HasIndex(e => e.WalletId, "wallettransaction_wallet_id_idx");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CommissionId).HasColumnName("commission_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'completed'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(50)
                .HasColumnName("transaction_type");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");
            entity.Property(e => e.WithdrawId).HasColumnName("withdraw_id");

            entity.HasOne(d => d.Commission).WithMany(p => p.Wallettransactions)
                .HasForeignKey(d => d.CommissionId)
                .HasConstraintName("wallettransaction_commission_id_fkey");

            entity.HasOne(d => d.Payment).WithMany(p => p.Wallettransactions)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("wallettransaction_payment_id_fkey");

            entity.HasOne(d => d.Wallet).WithMany(p => p.Wallettransactions)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("wallettransaction_wallet_id_fkey");

            entity.HasOne(d => d.Withdraw).WithMany(p => p.Wallettransactions)
                .HasForeignKey(d => d.WithdrawId)
                .HasConstraintName("wallettransaction_withdraw_id_fkey");
        });

        modelBuilder.Entity<Withdrawrequest>(entity =>
        {
            entity.HasKey(e => e.WithdrawId).HasName("withdrawrequest_pkey");

            entity.ToTable("withdrawrequest");

            entity.HasIndex(e => e.ArtistId, "withdrawrequest_artist_id_idx");

            entity.Property(e => e.WithdrawId).HasColumnName("withdraw_id");
            entity.Property(e => e.AccountHolder)
                .HasMaxLength(255)
                .HasColumnName("account_holder");
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(50)
                .HasColumnName("account_number");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.ArtistId).HasColumnName("artist_id");
            entity.Property(e => e.BankName)
                .HasMaxLength(255)
                .HasColumnName("bank_name");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.RequestedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("requested_at");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'pending'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Artist).WithMany(p => p.Withdrawrequests)
                .HasForeignKey(d => d.ArtistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("withdrawrequest_artist_id_fkey");
        });

        modelBuilder.Entity<Workshoppackage>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("workshoppackage_pkey");

            entity.ToTable("workshoppackage");

            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.CommissionRate)
                .HasPrecision(5, 2)
                .HasColumnName("commission_rate");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.MaxViewers).HasColumnName("max_viewers");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
