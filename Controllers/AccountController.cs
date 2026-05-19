using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AliMertRestoran.Models;

namespace AliMertRestoran.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            // Eğer giriş yapmışsa direkt admin paneline yönlendir
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Email == "admin@gmail.com" && model.Password == "Admin123!")
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Email),
                        new Claim(ClaimTypes.Role, "Admin")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "AdminCookies");

                    await HttpContext.SignInAsync("AdminCookies", new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Admin");
                }

                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminCookies");
            return RedirectToAction("Index", "Home");
        }
    }
}
