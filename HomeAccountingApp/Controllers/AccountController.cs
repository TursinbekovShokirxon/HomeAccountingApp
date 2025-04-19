using Application.Interfaces.Services;
using Application.Security;
using Application.Utilities;
using Domain.DTO;
using HomeAccountingApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly LoginProtectionService _loginProtection;
    public AccountController(IAccountService accountService, LoginProtectionService loginProtection)
    {
        _loginProtection = loginProtection;
        _accountService = accountService;
    }
    [HttpGet]
    public IActionResult Login() => View();

    [HttpGet]
    public IActionResult Register() => View();
    [HttpPost]
    public IActionResult Register(UserRegisterDTO userRegisterDTO)
    {
        if (userRegisterDTO.Password != userRegisterDTO.ConfirmPassword)
        {
            ViewBag.Error = "Пароли не совпадают";
            return View();
        }
        if (userRegisterDTO.Password.Length < 8)
        {
            ViewBag.Error = "Пароль должен содержать не менее 8 символов";
            return View();
        }
        var normalizedInput = userRegisterDTO.Username.NormalizeUsername();

        var result = _accountService.Register(normalizedInput, userRegisterDTO.Password);
        if (result.Result)
            return RedirectToAction("Login");

        ViewBag.Error = "Пользователь с таким именем уже существует";
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
            string normalizedInput = model.username.NormalizeUsername();

            if (_loginProtection.IsBlocked(normalizedInput))
            {
                ViewBag.Error = "Логин временно заблокирован. Повторите позже.";
                return View();
            }

            var result = await _accountService.Login(normalizedInput, model.password);
            if (result != null)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.username),
                new Claim("UserId",result.Id.ToString()),
                new Claim("Role", "User")
            };

                var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);
                _loginProtection.ResetAttempts(normalizedInput);
            ViewBag.Error = null;
                return RedirectToAction("Index", "Home");
            }

        ViewBag.Error = "Вы ввели не правильные данные, ";
        var RegisterFailedCount = _loginProtection.RegisterFailedAttempt(normalizedInput);

            ViewBag.Error += "количество попыток: " + (6 - RegisterFailedCount);  
        return View();

      
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("MyCookieAuth");
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

}
