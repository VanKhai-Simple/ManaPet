using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petshop_frontend.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int? OrderId { get; set; }

        public int? ProductId { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        // Liên kết đến đơn hàng tổng
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        // Liên kết đến sản phẩm để lấy tên, ảnh...
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}