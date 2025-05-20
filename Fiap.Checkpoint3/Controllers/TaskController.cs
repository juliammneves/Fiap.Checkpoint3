using Fiap.Checkpoint3.Model;
using Fiap.Checkpoint3.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fiap.Checkpoint3.Web.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskRepository _taskRepo;

        public TaskController(ITaskRepository taskRepo)
        {
            _taskRepo = taskRepo;
        }

        // GET: /Task
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var tasks = await _taskRepo.GetAllByUserAsync(userId);
            return View(tasks);
        }

        // GET: /Task/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var task = await _taskRepo.GetByIdAsync(id, userId);
            if (task == null) return NotFound();
            return View(task);
        }

        // GET: /Task/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Task/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Regra 2: duplicata no mesmo dia?
            if (await _taskRepo.ExistsWithTitleOnDateAsync(model.Title, userId, DateTime.Now))
            {
                ModelState.AddModelError(nameof(model.Title),
                    "Você já criou uma tarefa com esse título hoje.");
            }

            // Regra 3: prioridade Alta precisa de descrição ≥ 20 chars
            if (model.Priority == "Alta" &&
                (string.IsNullOrWhiteSpace(model.Description) || model.Description.Length < 20))
            {
                ModelState.AddModelError(nameof(model.Description),
                    "Tarefas de prioridade 'Alta' devem ter descrição com pelo menos 20 caracteres.");
            }

            if (!ModelState.IsValid)
                return View(model);

            model.UserId = userId;
            await _taskRepo.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Task/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var task = await _taskRepo.GetByIdAsync(id, userId);
            if (task == null) return NotFound();
            return View(task);
        }

        // POST: /Task/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem model)
        {
            // 1) recupera o userId dos claims
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // 2) garante que o model.Id bate com o route-id
            if (id != model.Id)
                return BadRequest();

            // 3) atribui o userId ao model para que o UpdateAsync funcione
            model.UserId = userId;

            // 4) regra de duplicata
            if (await _taskRepo.ExistsWithTitleOnDateAsync(model.Title, userId, DateTime.Now))
            {
                var existing = await _taskRepo.GetAllByUserAsync(userId);
                if (existing.Count(t =>
                        t.Title.Equals(model.Title, StringComparison.OrdinalIgnoreCase) &&
                        t.Id != model.Id &&
                        t.CreatedAt.Date == DateTime.Today) > 0)
                {
                    ModelState.AddModelError(nameof(model.Title),
                        "Já existe outra tarefa com esse título hoje.");
                }
            }

            // 5) regra de prioridade Alta
            if (model.Priority == "Alta" &&
                (string.IsNullOrWhiteSpace(model.Description) || model.Description.Length < 20))
            {
                ModelState.AddModelError(nameof(model.Description),
                    "Tarefas de prioridade 'Alta' devem ter descrição com pelo menos 20 caracteres.");
            }

            // 6) se houver erros de validação, voltar ao form
            if (!ModelState.IsValid)
                return View(model);

            // 7) finalmente, atualiza a tarefa
            await _taskRepo.UpdateAsync(model);

            return RedirectToAction(nameof(Index));
        }


        // GET: /Task/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var task = await _taskRepo.GetByIdAsync(id, userId);
            if (task == null) return NotFound();
            return View(task);
        }

        // POST: /Task/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _taskRepo.DeleteAsync(id, userId);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Conclude(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var task = await _taskRepo.GetByIdAsync(id, userId);
            if (task == null) return NotFound();

            // Regra 1: só conclui se descrição existir
            if (string.IsNullOrWhiteSpace(task.Description))
            {
                TempData["Error"] = "Não é possível concluir uma tarefa sem descrição.";
                return RedirectToAction(nameof(Index));
            }

            task.Status = "Concluído";
            await _taskRepo.UpdateAsync(task);
            return RedirectToAction(nameof(Index));
        }

    }
}
