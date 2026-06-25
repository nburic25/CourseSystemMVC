using CourseSystemMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CourseSystemMVC.Controllers
{
    public class SearchController : Controller
    {
        private CourseSystemDBEntities db = new CourseSystemDBEntities();

        // =========================
        // QUICK SEARCH (navbar)
        // =========================
        public ActionResult QuickSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index", "Courses");
            }

            var results = db.Courses
                .Where(c => c.Title.Contains(query))
                .ToList();

            ViewBag.Query = query;

            return View(results);
        }

        // =========================
        // ADVANCED SEARCH (GET)
        // =========================
        //[HttpGet]
        public ActionResult AdvancedSearch(string title, int? categoryId, decimal? priceFrom, decimal? priceTo)
        {
            var courses = db.Courses.AsQueryable();

            // TITLE filter
            if (!string.IsNullOrEmpty(title))
            {
                courses = courses.Where(c => c.Title.Contains(title));
            }

            // CATEGORY filter
            if (categoryId != null && categoryId != 0)
            {
                courses = courses.Where(c => c.CategoryID == categoryId);
            }

            // PRICE FROM
            if (priceFrom.HasValue)
            {
                courses = courses.Where(c => c.Price >= priceFrom.Value);
            }

            // PRICE TO
            if (priceTo.HasValue)
            {
                courses = courses.Where(c => c.Price <= priceTo.Value);
            }

            var result = courses.ToList();

            // dropdown data (VAŽNO za view)
            ViewBag.Categories = new SelectList(db.Categories, "CategoryID", "Name");

            return View(result);
        }

        // =========================
        // ADVANCED SEARCH (POST)
        // =========================
        [HttpPost]
        public ActionResult AdvancedSearch(string title, int[] categories, decimal? minPrice, decimal? maxPrice)
        {
            var query = db.Courses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(c => c.Title.Contains(title));
            }

            if (categories != null && categories.Length > 0)
            {
                query = query.Where(c => categories.Contains(c.CategoryID));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(c => c.Price >= minPrice);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(c => c.Price <= maxPrice);
            }

            ViewBag.Categories = db.Categories.ToList();

            return View(query.ToList());
        }
    }
}