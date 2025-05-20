using System.ComponentModel.DataAnnotations;

namespace Fiap.Checkpoint3.Web.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "{0} é obrigatório.")]
        [Display(Name = "Usuário")]
        public string Username { get; set; }

        [Required(ErrorMessage = "{0} é obrigatório.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter ao menos {2} caracteres.")]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [Required(ErrorMessage = "{0} é obrigatório.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "As senhas não conferem.")]
        [Display(Name = "Confirme a Senha")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "{0} é obrigatório.")]
        [Display(Name = "Nome Completo")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "{0} é obrigatório.")]
        [EmailAddress(ErrorMessage = "O campo {0} não é um e-mail válido.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }
}
