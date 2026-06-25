using CourseSystemMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CourseSystemMVC.Controllers
{
    public class NotificationsController : Controller
    {
        private CourseSystemDBEntities db = new CourseSystemDBEntities();

        // GET: Notifications
        public ActionResult Index()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Auth");

            int userId = (int)Session["UserID"];

            var notifications = db.Notifications
                .Where(n => n.UserID == userId)
                .OrderByDescending(n => n.NotificationID)
                .ToList();

            return View(notifications);
        }

        // označi kao pročitano
        public ActionResult MarkAsRead(int id)
        {
            var notif = db.Notifications.Find(id);
            if (notif != null)
            {
                notif.IsRead = true;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
