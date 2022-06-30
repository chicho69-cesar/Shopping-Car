using System.ComponentModel.DataAnnotations;

namespace ShoppingCar.Data.Entities {
    public class Category {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener maximo {1} caracteres")]
        [Display(Name = "Categoria")]
        public string Name { get; set; }

        public ICollection<ProductCategory> ProductCategories { get; set; }

        [Display(Name = "# Productos")]
        public int ProductsNumber => 
            ProductCategories == null ? 0 : ProductCategories.Count;
    }
}