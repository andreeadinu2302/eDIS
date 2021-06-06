using EDIS_MVC.Data;
using EDIS_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("AdminIndex");
            }
            if (User.IsInRole("Organization"))
            {
                return RedirectToAction("OrganizationIndex");
            }
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminIndex()
        {

            return View();
        }

        [Authorize(Roles = "Organization")]
        public async Task<IActionResult> OrganizationIndex()
        {
            //Get organization of user
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            var user_id = user.Id;
            var organization = _context.Organizations.Where(x => x.AdminUserId == user_id).FirstOrDefault();

            ViewBag.organization = organization;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
