using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petshop_frontend.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(200)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(15)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(500)]
        public string Address { get; set; }

        [StringLength(1000)]
        public string? Note { get; set; }

        public decimal? TotalAmount { get; set; }

        [StringLength(50)]
        public string? Status { get; set; } = "Chờ xác nhận";

        // Liên kết ngược lại bảng User
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        // Danh sách các mặt hàng trong đơn hàng này
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}