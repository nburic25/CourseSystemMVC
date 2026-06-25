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
    public class CoursesController : Controller
    {
        private CourseSystemDBEntities db = new CourseSystemDBEntities();

        // =========================
        // HELPER METODE
        // =========================
        private bool IsAdmin()
        {
            return Session["RoleID"] != null && (int)Session["RoleID"] == 1;
        }

        private bool IsLoggedIn()
        {
            return Session["UserID"] != null;
        }

        // =========================
        // LISTA - svi mogu vidjeti
        // =========================

        public ActionResult Index()
        {
                int? userId = Session["UserID"] as int?;

                var courses = db.Courses.ToList();

                if (userId != null)
                {
                    var enrolledCourseIds = db.Enrollments
                        .Where(e => e.UserID == userId)
                        .Select(e => e.CourseID)
                        .ToList();

                    ViewBag.EnrolledCourses = enrolledCourseIds;
                }

                return View(courses);
        }


        // GET: Courses
        //       public ActionResult Index()
        //      {
        //           var courses = db.Courses.Include(c => c.Category).Include(c => c.Instructor);
        //           return View(courses.ToList());
        //       }

        // GET: Courses/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var cours = db.Courses
                .Include("Category")
                .Include("Instructor")
                .Include("Lessons")
                .Include("Lessons.Materials")
                .Include("Exams")
                .Include("Reviews")
                .FirstOrDefault(c => c.CourseID == id);

            if (cours == null)
                return HttpNotFound();


            //if (Session["UserID"] == null)
            //{
            //    ViewBag.Error = "You must be logged in to view course details.";
            //    return RedirectToAction("Login", "Account");
            //}

            int? userId = Session["UserID"] as int?;
            int? roleId = Session["RoleID"] as int?;

            if (userId == null)
            {
                ViewBag.Error = "You must be logged in to continue.";
                return View(cours); // ili Redirect na login ako želiš
            }

            // =========================
            // ENROLLMENT CHECK
            // =========================
            bool isEnrolled = db.Enrollments
                .Any(e => e.CourseID == id && e.UserID == userId);

            ViewBag.IsEnrolled = isEnrolled;

            // =========================
            // RESULTS (ONLY FOR STUDENT)
            // =========================
            if (roleId == 2)
            {
                var completed = db.Database.SqlQuery<int>(
                    "SELECT CourseID FROM Courses c WHERE dbo.fn_IsCourseCompleted(@p0, c.CourseID) = 1",
                    userId
                ).ToList();

                ViewBag.CompletedCourses = completed;

                var avgScore = db.Database.SqlQuery<double>(
                    "SELECT dbo.fn_GetAverageScore(@p0)",
                    userId
                ).FirstOrDefault();

                ViewBag.AverageScore = avgScore;

                ViewBag.UserResults = db.Results
                    .Include(r => r.Exam)
                    .Where(r => r.UserID == userId)
                    .ToList();
            }
            else
            {
                // ADMIN ili drugi role → nema student logike
                ViewBag.IsCompleted = false;
                ViewBag.AverageScore = null;
                ViewBag.UserResults = null;
            }

            return View(cours);
        }

        // GET: Courses/Create
        public ActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("Index");
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name");
            var instructors = db.Instructors
                .Select(i => new
                {
                    i.InstructorID,
                     FullName = i.FirstName + " " + i.LastName
                }).ToList();

            ViewBag.InstructorID = new SelectList(instructors, "InstructorID", "FullName");
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CourseID,Title,Description,Price,CategoryID,InstructorID")] Cours cours)
        {
            if (!IsAdmin())
                return RedirectToAction("Index");
            if (ModelState.IsValid)
            {
                db.Courses.Add(cours);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name", cours.CategoryID);
            ViewBag.InstructorID = new SelectList(db.Instructors, "InstructorID", "FirstName", cours.InstructorID);
            return View(cours);
        }

        // GET: Courses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cours cours = db.Courses.Find(id);
            if (cours == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name", cours.CategoryID);
            var instructors = db.Instructors
                .Select(i => new
                {
                    i.InstructorID,
                    FullName = i.FirstName + " " + i.LastName
                }).ToList();

            ViewBag.InstructorID = new SelectList(instructors, "InstructorID", "FullName", cours.InstructorID);
            return View(cours);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CourseID,Title,Description,Price,CategoryID,InstructorID")] Cours cours)
        {
            if (!IsAdmin())
                return RedirectToAction("Index");
            if (ModelState.IsValid)
            {
                db.Entry(cours).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name", cours.CategoryID);
            ViewBag.InstructorID = new SelectList(db.Instructors, "InstructorID", "FirstName", cours.InstructorID);
            return View(cours);
        }

        // GET: Courses/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cours cours = db.Courses.Find(id);
            if (cours == null)
            {
                return HttpNotFound();
            }
            return View(cours);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index");
            Cours cours = db.Courses.Find(id);
            db.Courses.Remove(cours);
            db.SaveChanges();
            return RedirectToAction("Index");
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
