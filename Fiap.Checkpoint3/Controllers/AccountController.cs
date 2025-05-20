using Fiap.Checkpoint3.Model;
using Fiap.Checkpoint3.Repository.Interfaces;
using Fiap.Checkpoint3.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fiap.Checkpoint3.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepo;

        public AccountController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(vm);

            bool valid = await _userRepo.ValidateUserAsync(vm.Username, vm.Password);
            if (!valid)
            {
                ModelState.AddModelError("", "Usuário ou senha inválidos.");
                return View(vm);
            }

            // busca dados do usuário para claims
            var user = await _userRepo.GetByUsernameAsync(vm.Username);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // opcional: verificar se já existe
            var existing = await _userRepo.GetByUsernameAsync(vm.Username);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(vm.Username), "Esse usuário já está em uso.");
                return View(vm);
            }

            // mapear para a entidade de domínio
            var user = new User
            {
                Username = vm.Username,
                PasswordHash = vm.Password,    // CreateAsync fará o hash
                FullName = vm.FullName,
                Email = vm.Email,
                Role = "vendedor",     // ou outro perfil padrão
            };

            await _userRepo.CreateAsync(user);

            // após criar, redireciona para login
            TempData["Message"] = "Cadastro concluído! Faça login para continuar.";
            return RedirectToAction("Login");
        }
    }
}
