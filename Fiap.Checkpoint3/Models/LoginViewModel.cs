using System.ComponentModel.DataAnnotations;

namespace Fiap.Checkpoint3.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "{0} é obrigatório.")]
        [Display(Name = "Usuário")]
        public string Username { get; set; }

        [Required(ErrorMessage = "{0} é obrigatório.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }
    }
}
