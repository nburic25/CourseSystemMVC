using CourseSystemMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using CourseSystemMVC.Models;
using System.Security.Cryptography;
using System.Text;

namespace CourseSystemMVC.Controllers
{
    public class AuthController : Controller
    {
        CourseSystemDBEntities db = new CourseSystemDBEntities();

        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);

                var sb = new StringBuilder();
                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        //public ActionResult SeedAdmin()
        //{
        //    var admin = new User
        //    {
        //        FirstName = "Admin",
        //        LastName = "User",
        //        Email = "admin@mail.com",
        //        Password = HashPassword("admin123"),
        //        RoleID = 1,
        //        CreatedAt = DateTime.Now
        //    };

        //    db.Users.Add(admin);
        //    db.SaveChanges();

        //    return Content("Admin created");
        //}

        // GET: Auth
        public ActionResult Index()
        {
            return View();
        }

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            var hashedPassword = HashPassword(model.Password);

            var user = db.Users
                .FirstOrDefault(u => u.Email == model.Email && u.Password == hashedPassword);

            if (user != null)
            {
                Session["UserID"] = user.UserID;
                Session["UserName"] = user.FirstName;
                Session["RoleID"] = user.RoleID;

                return RedirectToAction("Index", "Courses");
            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}