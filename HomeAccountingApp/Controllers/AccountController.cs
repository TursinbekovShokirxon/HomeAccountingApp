﻿using Application.Interfaces.Services;
using Domain.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    public AccountController(IAccountService accountService)
    {
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
        var normalizedInput = NormalizeUsername(userRegisterDTO.Username);

        var result = _accountService.Register(normalizedInput, userRegisterDTO.Password);
        if (result.Result)
            return RedirectToAction("Login");

        ViewBag.Error = "Пользователь с таким именем уже существует";
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        string normalizedInput = username.Trim().ToLower();

        var result = await _accountService.Login(normalizedInput, password);
        if (result != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("UserId",result.Id.ToString()),
                new Claim("Role", "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);

            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Неверные учетные данные";
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
    public string NormalizeUsername(string username) =>
    username?.Trim().ToLowerInvariant();
}
