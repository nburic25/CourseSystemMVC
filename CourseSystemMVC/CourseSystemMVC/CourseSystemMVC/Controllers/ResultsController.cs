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
    public class ResultsController : Controller
    {
        private CourseSystemDBEntities db = new CourseSystemDBEntities();

        // GET: Results
        public ActionResult Index()
        {
            if (Session["RoleID"] == null)
                return RedirectToAction("Login", "Account");

            int roleId = (int)Session["RoleID"];

            if (roleId == 1) // ADMIN
            {
                var results = db.Results
                    .Include(r => r.Exam)
                    .Include(r => r.User)
                    .ToList();

                return View(results);
            }
            else // USER
            {
                int userId = (int)Session["UserID"];

                var avgScore = db.Database.SqlQuery<double>(
                    "SELECT dbo.fn_GetAverageScore(@p0)",
                    userId
                ).FirstOrDefault();

                ViewBag.AverageScore = avgScore;

                var results = db.Results
                    .Include(r => r.Exam)
                    .Where(r => r.UserID == userId)
                    .ToList();

                return View(results);
            }
        }

        // GET: Results/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Result result = db.Results.Find(id);
            if (result == null)
            {
                return HttpNotFound();
            }
            return View(result);
        }

        // GET: Results/Create
        public ActionResult Create(int? examId)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1)
                return new HttpStatusCodeResult(403);

            ViewBag.ExamID = new SelectList(db.Exams, "ExamID", "Title", examId);
            ViewBag.UserID = new SelectList(db.Users, "UserID", "Email");

            return View();
        }

        // POST: Results/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ResultID,Score,UserID,ExamID")] Result result)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1)
            {
                return new HttpStatusCodeResult(403);
            }

            if (ModelState.IsValid)
            {
                //db.Results.Add(result);
                //db.SaveChanges();
                db.Database.ExecuteSqlCommand(
                    "EXEC sp_AddResult @p0, @p1, @p2",
                    result.UserID,
                    result.ExamID,
                    result.Score
                );

                TempData["Success"] = "Result added successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.ExamID = new SelectList(db.Exams, "ExamID", "Title", result.ExamID);
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName", result.UserID);
            return View(result);
        }

        // GET: Results/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Result result = db.Results.Find(id);
            if (result == null)
            {
                return HttpNotFound();
            }
            ViewBag.ExamID = new SelectList(db.Exams, "ExamID", "Title", result.ExamID);
            ViewBag.UserID = new SelectList(db.Users, "UserID", "Email", result.UserID);
            return View(result);
        }

        // POST: Results/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ResultID,Score,UserID,ExamID")] Result result)
        {
            if (ModelState.IsValid)
            {
                db.Entry(result).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ExamID = new SelectList(db.Exams, "ExamID", "Title", result.ExamID);
            ViewBag.UserID = new SelectList(db.Users, "UserID", "FirstName", result.UserID);
            return View(result);
        }

        // GET: Results/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Result result = db.Results.Find(id);
            if (result == null)
            {
                return HttpNotFound();
            }
            return View(result);
        }

        // POST: Results/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Result result = db.Results.Find(id);
            db.Results.Remove(result);
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
