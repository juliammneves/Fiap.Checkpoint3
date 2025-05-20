using Fiap.Checkpoint3.Models;
using Fiap.Checkpoint3.Repository.Interfaces;
using Fiap.Checkpoint3.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Fiap.Checkpoint3.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ITaskRepository _taskRepo;

        public DashboardController(ITaskRepository taskRepo)
            => _taskRepo = taskRepo;

        public async Task<IActionResult> Index()
        {
            // Obtém userId dos claims
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var tasks = await _taskRepo.GetAllByUserAsync(userId);

            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var model = new DashboardViewModel
            {
                TotalTasks = tasks.Count(),
                OpenTasks = tasks.Count(t => t.Status == "Aberto"),
                InProgressTasks = tasks.Count(t => t.Status == "Em Andamento"),
                CompletedTasks = tasks.Count(t => t.Status == "Concluído"),
                TasksThisMonth = tasks.Count(t => t.CreatedAt >= monthStart)
            };

            return View(model);
        }
    }
}
