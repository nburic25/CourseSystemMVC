using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CourseSystemMVC.Models;

namespace CourseSystemMVC.Controllers
{
    public class PaymentsController : Controller
    {
        private CourseSystemDBEntities db = new CourseSystemDBEntities();

        // GET: Payments
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int CourseID)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserID"];

            // ✔ već kupljen?
            bool alreadyEnrolled = db.Enrollments.Any(e =>
                e.UserID == userId &&
                e.CourseID == CourseID);

            if (alreadyEnrolled)
            {
                TempData["Error"] = "You already purchased this course.";
                return RedirectToAction("Details", "Courses", new { id = CourseID });
            }

            // ✔ uzmi cijenu kursa
            var course = db.Courses.Find(CourseID);

            // ✔ napravi payment
            Payment payment = new Payment
            {
                UserID = userId,
                CourseID = CourseID,
                Amount = course.Price ?? 0,
                PaymentDate = DateTime.Now
            };

            db.Payments.Add(payment);

            // ✔ napravi enrollment automatski
            Enrollment enrollment = new Enrollment
            {
                UserID = userId,
                CourseID = CourseID,
                EnrollmentDate = DateTime.Now,
                Status = "Active"
            };

            db.Enrollments.Add(enrollment);

            db.SaveChanges();

            TempData["Success"] = "Payment successful! You are now enrolled.";

            return RedirectToAction("Details", "Courses", new { id = CourseID });
        }
    }
}