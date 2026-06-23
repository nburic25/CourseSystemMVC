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
    public class ReviewsController : Controller
    {
        private CourseSystemDBEntities db = new CourseSystemDBEntities();

        private bool IsLoggedIn()
        {
            return Session["UserID"] != null;
        }

        // GET: Reviews
        public ActionResult Index()
        {
            var reviews = db.Reviews.Include(r => r.Cours).Include(r => r.User);
            return View(reviews.ToList());
        }

        // GET: Reviews/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        // GET: Reviews/Create
        public ActionResult Create()
        {
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title");
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName");
            return View();
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Rating,Comment,CourseID")] Review review)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserID"];

            // ✔ provjera da li je enrolled
            bool isEnrolled = db.Enrollments.Any(e =>
                e.UserID == userId &&
                e.CourseID == review.CourseID);

            if (!isEnrolled)
            {
                TempData["Error"] = "You must be enrolled to review this course.";
                return RedirectToAction("Details", "Courses", new { id = review.CourseID });
            }

            // provjera da li je već ostavio review
            bool alreadyReviewed = db.Reviews.Any(r =>
                r.UserID == userId &&
                r.CourseID == review.CourseID);

            if (alreadyReviewed)
            {
                TempData["Error"] = "You can only leave one review for this course.";
                return RedirectToAction("Details", "Courses", new { id = review.CourseID });
            }

            // ✔ sve OK → dodaj review
            review.UserID = userId;

            db.Reviews.Add(review);
            db.SaveChanges();

            TempData["Success"] = "Review added successfully!";

            return RedirectToAction("Details", "Courses", new { id = review.CourseID });
        }

        // GET: Reviews/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title", review.CourseID);
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName", review.UserID);
            return View(review);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ReviewID,Rating,Comment,UserID,CourseID")] Review review)
        {
            if (ModelState.IsValid)
            {
                db.Entry(review).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title", review.CourseID);
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName", review.UserID);
            return View(review);
        }

        // GET: Reviews/Delete/5
        //[HttpGet]
        //public ActionResult Delete(int? id)
        //{
        //    return HttpNotFound(); // ili potpuno obriši metodu
        //}

        // POST: Reviews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public ActionResult Delete(int id)
        {
            var review = db.Reviews.Find(id);

            if (review == null)
                return HttpNotFound();

            int userId = (int)Session["UserID"];

            if (review.UserID != userId)
                return new HttpStatusCodeResult(403);

            int courseId = review.CourseID;

            db.Reviews.Remove(review);
            db.SaveChanges();

            return RedirectToAction("Details", "Courses", new { id = courseId });
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
