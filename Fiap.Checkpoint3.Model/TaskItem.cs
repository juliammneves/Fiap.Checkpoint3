using System.ComponentModel.DataAnnotations;

namespace Fiap.Checkpoint3.Model
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório.")]
        [Display(Name = "Título")]
        public string Title { get; set; }

        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Prioridade")]
        public string Priority { get; set; } = "Média";

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Aberto";

        [Display(Name = "Criado em")]
        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
    }
}
