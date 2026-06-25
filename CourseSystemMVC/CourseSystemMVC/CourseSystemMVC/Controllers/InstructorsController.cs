using CourseSystemMVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CourseSystemMVC.Controllers
{
    public class InstructorsController : Controller
    {
        private CourseSystemDBEntities db = new CourseSystemDBEntities();

        // =========================
        // ADMIN CHECK
        // =========================
        private bool IsAdmin()
        {
            return Session["RoleID"] != null && (int)Session["RoleID"] == 1;
        }

        // =========================
        // INDEX (LISTA)
        // =========================
        public ActionResult Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            return View(db.Instructors.ToList());
        }

        // =========================
        // DETAILS
        // =========================
        public ActionResult Details(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Instructor instructor = db.Instructors.Find(id);

            if (instructor == null)
                return HttpNotFound();

            return View(instructor);
        }

        // =========================
        // CREATE
        // =========================
        public ActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Instructor instructor)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                db.Instructors.Add(instructor);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(instructor);
        }

        // =========================
        // EDIT
        // =========================
        public ActionResult Edit(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Instructor instructor = db.Instructors.Find(id);

            if (instructor == null)
                return HttpNotFound();

            return View(instructor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Instructor instructor)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                db.Entry(instructor).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(instructor);
        }

        // =========================
        // DELETE
        // =========================
        public ActionResult Delete(int? id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Instructor instructor = db.Instructors.Find(id);

            if (instructor == null)
                return HttpNotFound();

            return View(instructor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            Instructor instructor = db.Instructors.Find(id);
            db.Instructors.Remove(instructor);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // =========================
        // DISPOSE
        // =========================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}