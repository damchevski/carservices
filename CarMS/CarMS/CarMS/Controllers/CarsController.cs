using CarMS.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CarMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CarsController(ApplicationDbContext context)
        {
            this._context = context;
        }

        // GET: api/<CarsController>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<List<Car>> Get()
        {
            var auth = this.VerifyToken(Request.Headers["Authorization"]);
            if (!auth)
                throw new Exception("Fail authentication");

            return this._context.Cars.ToList();
        }

        // GET api/<CarsController>/5
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Car> Get(int id)
        {
            var auth = this.VerifyToken(Request.Headers["Authorization"]);
            if (!auth)
                throw new Exception("Fail authentication");

            return this._context.Cars.Where(x => x.Id == id).FirstOrDefault();   
        }

        // POST api/<CarsController>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Car> Post([FromBody] Car car)
        {
            var token = User.Claims.SingleOrDefault().Value;

            var auth = this.VerifyToken(Request.Headers["Authorization"]);
            var role = this.GetRole(token, "Admin");

            if (!auth)
                throw new Exception("Fail authentication");

            if (!role)
                throw new Exception("You are not Admin");

            if (car.Color == null || car.Manufacturer == null || car.YearOfProduction == null || car.Model == null)
            {
                return null;
            }

            Car findCar = this._context.Cars.Where(x => x.Manufacturer == car.Manufacturer && x.Model == car.Model).FirstOrDefault();

            if (findCar != null)
                return findCar;

            this._context.Add(car);
            this._context.SaveChanges();

            return car;
        }


        // DELETE api/<CarsController>/5
        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<bool> Delete([FromQuery]int id)
        {
            var token = User.Claims.SingleOrDefault().Value;

            var auth = this.VerifyToken(Request.Headers["Authorization"]);
            var role = this.GetRole(token, "Admin");

            if (!auth)
                throw new Exception("Fail authentication");

            if (!role)
                throw new Exception("You are not Admin");

            Car car = this._context.Cars.Where(x => x.Id == id).FirstOrDefault();
            this._context.Remove(car);

            return true;
        }

       [HttpPost("addcar")]
       [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
       public async Task<UserCar> AddCar([FromQuery] int CarId)
       {
            var auth = this.VerifyToken(Request.Headers["Authorization"]);
            if (!auth)
                throw new Exception("Fail authentication");

            if (CarId.Equals("") || CarId == null)
           {
               throw new Exception("Car Id Required");
           }

           var full = User
               .Claims
               .SingleOrDefault();

           var username = full.Value.Split(",")[0];

           UserCar forAdd = new UserCar() { Username = username, CarId = CarId };

           _context.Add(forAdd);
           _context.SaveChanges();

           return forAdd;
       }

        private bool GetRole(string Token, string role)
        {
            return role.Equals( Token.Split(",")[1]);
        }

        private bool VerifyToken(string Token)
        {
            HttpClient client = new HttpClient();

            string URI = "https://localhost:44311/api/User/Verify";
            client.DefaultRequestHeaders.Add("Authorization",Token);

            HttpResponseMessage responseMessage = client.GetAsync(URI).Result;

            var result = Boolean.Parse(responseMessage.Content.ReadAsStringAsync().Result);

            return result;
        }
    }
}
