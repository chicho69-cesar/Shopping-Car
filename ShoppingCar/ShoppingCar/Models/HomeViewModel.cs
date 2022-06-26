using ShoppingCar.Data.Entities;

namespace ShoppingCar.Models {
    public class HomeViewModel {
        public ICollection<Product> Products { get; set; }
        public float Quantity { get; set; }
    }
}