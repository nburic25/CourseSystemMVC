using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CourseSystemMVC.Models;

namespace CourseSystemMVC.Controllers
{
    public class EnrollmentsController : Controller
    {
        private CourseSystemDBEntities db = new CourseSystemDBEntities();

        private bool IsAdmin()
        {
            return Session["RoleID"] != null && (int)Session["RoleID"] == 1;
        }

        private bool IsLoggedIn()
        {
            return Session["UserID"] != null;
        }

        // GET: Enrollments
        public ActionResult Index()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Auth");

            int userId = (int)Session["UserID"];
            int roleId = (int)Session["RoleID"];

            if (roleId == 1)
            {
                // ADMIN vidi sve
                var all = db.Enrollments.ToList();
                return View(all);
            }
            else
            {
                // USER vidi samo svoje
                var mine = db.Enrollments
                    .Where(e => e.UserID == userId)
                    .ToList();

                return View(mine);
            }
        }

        //        public ActionResult Index()
        //        {
        //            var enrollments = db.Enrollments.Include(e => e.Cours).Include(e => e.User);
        //            return View(enrollments.ToList());
        //        }

        // GET: Enrollments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var enrollment = db.Enrollments
                .Include("Cours")
                .Include("Cours.Instructor")
                .Include("User")
                .FirstOrDefault(e => e.EnrollmentID == id);

            if (enrollment == null)
                return HttpNotFound();

            return View(enrollment);
        }

        // GET: Enrollments/Create
        public ActionResult Create()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Auth");

            // samo admin može na create
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1)
                return RedirectToAction("Index", "Courses");

            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title");
            ViewBag.Status = new SelectList(new[] { "Active", "Completed"});

            if (Session["RoleID"] != null && (int)Session["RoleID"] == 1)
            {
                var users = db.Users
                    .Select(u => new {
                        u.UserID,
                        FullName = u.FirstName + " " + u.LastName
                    }).ToList();

                ViewBag.UserID = new SelectList(users, "UserID", "FullName");
            }

            return View();
        }

        // POST: Enrollments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create(FormCollection form)
        {
            var enrollment = new Enrollment();

            enrollment.CourseID = int.Parse(form["CourseID"]);
            enrollment.UserID = (int)Session["UserID"];
            enrollment.Status = "Active";
            enrollment.EnrollmentDate = DateTime.Now;

            db.Enrollments.Add(enrollment);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Successfully enrolled!";
            TempData["ShowMyEnrollments"] = true;

            return RedirectToAction("Details", "Courses", new { id = enrollment.CourseID });
        }

        // GET: Enrollments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1)
                return RedirectToAction("Index");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName", enrollment.UserID);
            ViewBag.Status = new SelectList(new[] { "Active", "Completed" });
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EnrollmentID,UserID,CourseID,EnrollmentDate,Status")] Enrollment enrollment)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1)
                return RedirectToAction("Index");
            if (ModelState.IsValid)
            {
                db.Entry(enrollment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName", enrollment.UserID);
            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index");
            Enrollment enrollment = db.Enrollments.Find(id);
            db.Enrollments.Remove(enrollment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Enroll
        public ActionResult Enroll(int id)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Auth");

            int userId = (int)Session["UserID"];

            // provjera da li već postoji enrollment
            var existing = db.Enrollments
                .FirstOrDefault(e => e.CourseID == id && e.UserID == userId);

            if (existing == null)
            {
                var enrollment = new Enrollment
                {
                    CourseID = id,
                    UserID = userId,
                    EnrollmentDate = DateTime.Now,
                    Status = "Active"
                };

                db.Enrollments.Add(enrollment);
                db.SaveChanges();
            }

            return RedirectToAction("Index", "Enrollments");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
