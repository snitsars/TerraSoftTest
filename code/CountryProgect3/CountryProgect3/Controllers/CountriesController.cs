using System;
using System.Web.Http;
using System.Web.Mvc;
using CountryProgect3.Models;

namespace CountryProgect3.Controllers
{
    public class CountriesController : Controller
    {
        //[FromServices]
        private ICountriesRepository CountryItems = new CountriesRepository();

        public ActionResult Index()
        {
            ViewBag.Title = "Countries edit page";
            if (CountryItems != null)
                return View(CountryItems.GetAll());
            else
            {
                return View();
            }
        }

        public ActionResult Create()
        {
            return View();
        }

        // POST api/values
        [System.Web.Mvc.HttpPost]
        public ActionResult Create([FromBody]string name)
        {
            try
            {
                CountryItems.Add(name);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
            
        }

    }
}