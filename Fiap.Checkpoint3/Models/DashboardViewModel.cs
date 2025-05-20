namespace Fiap.Checkpoint3.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalTasks { get; set; }
        public int OpenTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int TasksThisMonth { get; set; }
    }
}
