namespace Petshop_frontend.Models
{
    public class AdminDashboardVM
    {
        public int NewOrdersCount { get; set; }
        public decimal RevenueGrowth { get; set; } // % tăng trưởng
        public int TotalUsers { get; set; }
        public int OutOfStockCount { get; set; }

        public List<Order> LatestOrders { get; set; }
        public List<Product> LatestProducts { get; set; }
    }
}
