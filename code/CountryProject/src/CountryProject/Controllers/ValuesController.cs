using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using CountryProject.Models;

namespace CountryProject.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly Country[] _countries = new Country[]
{
            new Country { Id = new Guid("CA72535B-F50A-4525-B536-BD0547D0902E"), Name = "Japan" },
            new Country { Id = new Guid("E8FE299D-A0FA-4775-8EE1-F79FDEEADB91"), Name = "Ukraine" },
            new Country { Id = new Guid("403B4356-DD37-49AA-94CD-C7336DF31FB8"), Name = "USA"}
};
        // GET: api/values
        [HttpGet]
        public IEnumerable<Country> Get()
        {
            return _countries;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
