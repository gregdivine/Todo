using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AspNetCoreTodo.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreTodo.Controllers
{
    [Authorize(Roles = Constants.AdministratorRole)]
    public class ManageUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ManageUsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var admins = await _userManager.GetUsersInRoleAsync(Constants.AdministratorRole);

            var everyone = (await _userManager.Users.ToListAsync()).Except(admins);

            var model = new ManageUsersViewModel
            {
                Administrators = admins,
                Everyone = everyone
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (ModelState.IsValid)
            {
                var usr = _userManager.Users.FirstOrDefault(u => u.Id == id);

                if ( usr != null )
                {
                    var isAdmin = await _userManager.IsInRoleAsync(usr, Constants.AdministratorRole);

                    if (!isAdmin)
                    {
                        var res = await _userManager.DeleteAsync(usr);

                        if (res.Succeeded)
                        {
                            TempData["message"] = "Done!";
                            return RedirectToAction("Index");
                        }
                    }
                }
            }

            TempData["message"] = "Can't do this!";
            return RedirectToAction("Index");
        }
    }
}