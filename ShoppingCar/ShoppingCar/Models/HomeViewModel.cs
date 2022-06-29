using ShoppingCar.Common;
using ShoppingCar.Data.Entities;

namespace ShoppingCar.Models {
    public class HomeViewModel {
        public PaginatedList<Product> Products { get; set; }
        public ICollection<Category> Categories { get; set; }
        public float Quantity { get; set; }
    }
}