namespace Petshop_frontend.Models
{
    public class OrderAdminViewModel
    {
        public Order Order { get; set; }
        public List<OrderDetail> Details { get; set; }
    }
}
