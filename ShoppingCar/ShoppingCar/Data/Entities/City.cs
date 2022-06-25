﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ShoppingCar.Data.Entities {
    public class City {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener maximo {1} caracteres")]
        [Display(Name = "Estado")]
        public string Name { get; set; }

        [JsonIgnore]
        public State State { get; set; }

        public ICollection<User> Users { get; set; }
    }
}