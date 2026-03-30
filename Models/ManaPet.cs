using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Petshop_frontend.Models;

public partial class ManaPet : DbContext


{
    public ManaPet()
    {
    }

    public ManaPet(DbContextOptions<ManaPet> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<PasswordReset> PasswordResets { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }



//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-5299US0;Initial Catalog=petshop;Persist Security Info=True;User ID= sa;Password=12345678;Encrypt=false;");

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.Entity<Cart>(entity =>
    //    {
    //        entity.HasKey(e => e.Id).HasName("PK__Carts__3214EC07B1D88C74");

    //        entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

    //        entity.HasOne(d => d.User).WithOne(p => p.Cart).HasConstraintName("FK__Carts__UserId__49C3F6B7");
    //    });

    //    modelBuilder.Entity<Category>(entity =>
    //    {
    //        entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC070C08AF44");
    //    });

    //    modelBuilder.Entity<Conversation>(entity =>
    //    {
    //        entity.HasKey(e => e.Id).HasName("PK__Conversa__3214EC07A3425D8D");

    //        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
    //        entity.Property(e => e.IsClosed).HasDefaultValue(false);

    //        entity.HasOne(d => d.User).WithMany(p => p.Conversations).HasConstraintName("FK__Conversat__UserI__4D94879B");
    //    });

    //    modelBuilder.Entity<Message>(entity =>
    //    {
    //        entity.HasKey(e => e.Id).HasName("PK__Messages__3214EC07A100CB60");

    //        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
    //        entity.Property(e => e.IsRead).HasDefaultValue(false);

    //        entity.HasOne(d => d.Conversation).WithMany(p => p.Messages).HasConstraintName("FK__Messages__Conver__52593CB8");
    //    });

    //    modelBuilder.Entity<PasswordReset>(entity =>
    //    {
    //        entity.HasKey(e => e.Id).HasName("PK__Password__3214EC07A3204FA9");

    //        entity.Property(e => e.IsUsed).HasDefaultValue(false);
    //    });

    //    modelBuilder.Entity<Product>(entity =>
    //    {
    //        entity.HasKey(e => e.Id).HasName("PK__Products__3214EC07458E0A8C");

    //        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
    //        entity.Property(e => e.IsDiscount).HasDefaultValue(false);
    //        entity.Property(e => e.IsPet).HasDefaultValue(false);
    //        entity.Property(e => e.StockQuantity).HasDefaultValue(0);

    //        entity.HasOne(d => d.Category).WithMany(p => p.Products).HasConstraintName("FK__Products__Catego__7D439ABD");
    //    });

    //    modelBuilder.Entity<ProductImage>(entity =>
    //    {
    //        entity.HasKey(e => e.Id).HasName("PK__ProductI__3214EC07A3653881");

    //        entity.Property(e => e.IsDefault).HasDefaultValue(false);

    //        entity.HasOne(d => d.Product).WithMany(p => p.ProductImages).HasConstraintName("FK__ProductIm__Produ__02FC7413");
    //    });

    //    modelBuilder.Entity<User>(entity =>
    //    {
    //        entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07BE587B54");

    //        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
    //        entity.Property(e => e.Provider).HasDefaultValue("Local");
    //        entity.Property(e => e.Role).HasDefaultValue("Customer");
    //        entity.Property(e => e.Status).HasDefaultValue(true);
    //    });

    //    modelBuilder.Entity<UserProfile>(entity =>
    //    {
    //        entity.HasKey(e => e.Id).HasName("PK__UserProf__3214EC073A2DCCA2");

    //        entity.HasOne(d => d.User).WithOne(p => p.UserProfile).HasConstraintName("FK__UserProfi__UserI__4222D4EF");
    //    });

    //    OnModelCreatingPartial(modelBuilder);
    //}

    //partial void OnModelCreatingPartial(ModelBuilder modelBuilder);


}
