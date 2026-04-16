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

    [Required(ErrorMessage = "Vui lòng chọn danh mục sản phẩm")]
    public int CategoryId { get; set; }

    [StringLength(200, ErrorMessage = "Tên không được quá 200 ký tự")]
    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    public string ProductName { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập giá bán")]
    [Range(10000, 1000000000, ErrorMessage = "Giá phải từ 10000 đến 1 tỷ VNĐ")]
    public decimal? Price { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số lượng")]
    [Range(1, 9999, ErrorMessage = "Số lượng kho không hợp lệ")]
    public int StockQuantity { get; set; } = 0;

    public string? MainImage { get; set; }

    public bool? IsPet { get; set; } = false;

    [Range(1, 130, ErrorMessage = "Tháng tuổi không hợp lệ")]
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
    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    
    public decimal? DiscountPrice { get; set; }

    [Range(0, 100, ErrorMessage = "Phần trăm giảm giá từ 0-100%")]
    public int? DiscountPercent { get; set; }

    public bool? IsDiscount { get; set; } = false;


    [NotMapped]
    public decimal FinalPrice => IsDiscount == true
    ? (DiscountPrice ?? Price ?? 0)
    : (Price ?? 0);

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category? Category { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
