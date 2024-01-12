using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebSystem.Shared.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        [MaxLength(40, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string FirstName { get; set; } = null!;

        [Display(Name = "Apellido")]
        [MaxLength(40, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string LastName { get; set; } = null!;

        [Display(Name = "Email")]
        [MaxLength(40, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Email { get; set; } = null!;

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "El campo {0} debe tener entre {2} y {1} carácteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public bool IsAdmin { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public bool IsConfirm { get; set; }

        [Display(Name = "Token")]
        [MaxLength(15, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
        public string? Token { get; set; } = null!;

        public string FullName => $"{LastName} {FirstName}";
    }
}
