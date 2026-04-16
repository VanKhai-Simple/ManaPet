namespace Petshop_frontend.Models
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalMoney { get; set; }

        // Các trường này để map với Form và Orders table
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string? Note { get; set; }
        public string? Email { get; set; } // Dùng để check/update UserProfile
    }
}
