using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CountryCRUD_ASP.Models;

namespace CountryCRUD_ASP.Controllers
{
    public class CountriesController : ApiController
    {
        private readonly Country[] _countries = new Country[]
        {
            new Country { Id = new Guid("CA72535B-F50A-4525-B536-BD0547D0902E"), Name = "Japan" },
            new Country { Id = new Guid("E8FE299D-A0FA-4775-8EE1-F79FDEEADB91"), Name = "Ukraine" },
            new Country { Id = new Guid("403B4356-DD37-49AA-94CD-C7336DF31FB8"), Name = "USA"}
        };

        public IEnumerable<Country> GetAllProducts()
        {
            return _countries;
        }


        public IHttpActionResult GetCountries(Guid id)
        {
            var country = _countries.FirstOrDefault((p) => p.Id == id);
            if (_countries == null)
            {
                return NotFound();
            }
            return Ok(country);
        }
    }
}
