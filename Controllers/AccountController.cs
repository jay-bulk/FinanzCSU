using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinanzCSU.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

using aBCryptNet = BCrypt.Net.BCrypt;

namespace FinanzCSU.Controllers
{
    public class AccountController : Controller
    {
        private readonly Team106DBContext _context;

        public AccountController(Team106DBContext context)
        {
            _context = context;
        }

        public IActionResult Login(string returnURL)
        {
            returnURL = String.IsNullOrEmpty(returnURL) ? "~/" : returnURL;

            return View(new LoginInput { ReturnURL = returnURL });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Username,UserPassword,ReturnURL")] LoginInput loginInput)
        {
            if (ModelState.IsValid)
            {
                var aUser = await _context.LoginInfos.FirstOrDefaultAsync(u => u.UName == loginInput.Username);

                if (aUser != null && aBCryptNet.Verify(loginInput.UserPassword, aUser.UPass))
                {
                    var claims = new List<Claim>();

                    claims.Add(new Claim(ClaimTypes.Name, aUser.FullName));
                    claims.Add(new Claim(ClaimTypes.Sid, aUser.UserID.ToString()));

                    string[] roles = aUser.URole.Split(",");

                    foreach (string role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return Redirect(loginInput?.ReturnURL ?? "~/");

                }
                else
                {
                    ViewData["message"] = "Invalid credentials";
                }
            }

            return View(loginInput);
        }


        // GET: Sign up for an Account
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([Bind("UName,UPass,FullName")] LoginInfo loginInfo)
        {
            if (ModelState.IsValid)
            {
                var aUser = await _context.LoginInfos.FirstOrDefaultAsync(u => u.UName == loginInfo.UName);

                if (aUser is null)
                {
                    loginInfo.UPass = aBCryptNet.HashPassword(loginInfo.UPass);

                    loginInfo.URole = "User";

                    _context.Add(loginInfo);
                    await _context.SaveChangesAsync();

                    TempData["success"] = "Account created";

                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    ViewData["message"] = "Choose a different username";
                }
            }
            return View(loginInfo);
        }
        public async Task<RedirectToActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}
