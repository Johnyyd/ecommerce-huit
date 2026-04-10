using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HuitShopDB.Models;
namespace HuitShopDB.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        HuitShopDBDataContext data = new HuitShopDBDataContext();
        public ActionResult Index()
        {
            return View();
        }

    }
}
