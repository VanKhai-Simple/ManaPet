using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Petshop_frontend.Models;

public partial class Product
{
    [Key]
    public int Id { get; set; }

    public int? CategoryId { get; set; }

    [StringLength(200)]
    public string ProductName { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    public int? StockQuantity { get; set; }

    public string? MainImage { get; set; }

    public bool? IsPet { get; set; }

    public int? AgeMonths { get; set; }

    public bool? Gender { get; set; }

    [StringLength(100)]
    public string? FatherInfo { get; set; }

    [StringLength(100)]
    public string? MotherInfo { get; set; }

    [StringLength(50)]
    public string? FurColor { get; set; }

    [StringLength(200)]
    public string? HealthStatus { get; set; }

    [StringLength(100)]
    public string? Condition { get; set; }

    public bool? Dewormed { get; set; }

    [StringLength(200)]
    public string? Origin { get; set; }

    public string? Character { get; set; }

    public string? FullDescription { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? DiscountPrice { get; set; }

    public int? DiscountPercent { get; set; }

    public bool? IsDiscount { get; set; }

    public decimal FinalPrice => IsDiscount == true
            ? (DiscountPrice ?? Price)
            : Price;

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category? Category { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
