using System.ComponentModel.DataAnnotations;

namespace ShoppingCar.Data.Entities {
    public class Country {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener maximo {1} caracteres")]
        [Display(Name = "País")]
        public string Name { get; set; }

        public ICollection<State> States { get; set; }

        [Display(Name = "Estados")]
        public int StatesNumber => 
            States == null ? 0 : States.Count;

        [Display(Name = "Ciudades")]
        public int CitiesNumber => 
            States == null ? 0 : States.Sum(s => s.CitiesNumber);
    }
}