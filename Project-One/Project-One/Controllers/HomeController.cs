using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Controllers
{
    public class HomeController : Controller
    {
        ContactDbEntities dt = new ContactDbEntities(); 

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(Table cot)
        {
            dt.Tables.Add(cot);
            dt.SaveChanges();
            return RedirectToAction("Second");
        }

        public ActionResult Second()
        {
            return View();
        }
    }
}