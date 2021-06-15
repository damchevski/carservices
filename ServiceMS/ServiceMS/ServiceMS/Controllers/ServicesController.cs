
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ServiceMS.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ServiceMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            this._context = context;
        }

        // GET: api/<ServicesController>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<List<Service>> Get()
        {
            var auth = this.VerifyToken(Request.Headers["Authorization"]);
            if (!auth)
                throw new Exception("Fail authentication");

            return this._context.Services.ToList();
        }

        // POST api/<ServicesController>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<Service> Post([FromBody] ServiceDto service)
        {
            var auth = this.VerifyToken(Request.Headers["Authorization"]);
            if (!auth)
                throw new Exception("Fail authentication");

            if (service.UserId == null || service.CarId == null ||
                service.ServiceItemsIds.Count == 0)
                throw new Exception("All Fields required");


            List<ServiceItem> serviceItems = new List<ServiceItem>();
            int totalPrice = 0;


            HttpClient client = new HttpClient();

            foreach (var item in service.ServiceItemsIds)
            {
                string URI = "https://localhost:44385/api/serviceitems/" + item;

                HttpResponseMessage responseMessage = await client.GetAsync(URI);

                var result = responseMessage.Content.ReadAsAsync<ServiceItem>().Result;

                serviceItems.Add(result);
                totalPrice += result.Price;
            }

            string URI1 = "https://localhost:44385/api/cars/" + service.CarId;

            HttpResponseMessage responseMessage1 = await client.GetAsync(URI1);

            Car car = responseMessage1.Content.ReadAsAsync<Car>().Result;

            Service newService = new Service()
            {
                UserId = service.UserId,
                CarId = service.CarId,
                CarModel = car.Manufacturer + "-" + car.Model,
                TotalPrice = totalPrice,
                Date = DateTime.Now
            };

            this._context.Add(newService);
            this._context.SaveChanges();

            foreach(var serviceItem in serviceItems)
            {
                this._context.Add(new ServiceServiceItem()
                {
                    ServiceName = serviceItem.Name,
                    ServiceId = newService.Id,
                    ServiceItemId = serviceItem.Id
                });
                this._context.SaveChanges();
            }

            return newService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<List<ServiceServiceItem>> GetAllItemsForService([FromQuery] int serviceId)
        {
            var auth = this.VerifyToken(Request.Headers["Authorization"]);
            if (!auth)
                throw new Exception("Fail authentication");

            if (serviceId == null)
                throw new Exception("All fields required");

            return this._context.ServiceServiceItems
                .Where(x => x.ServiceId == serviceId).ToList();
        }

        // DELETE api/<ServicesController>/5
        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<bool> Delete([FromQuery] int id)
        {
            var token = User.Claims.SingleOrDefault().Value;

            var auth = this.VerifyToken(Request.Headers["Authorization"]);
            var role = this.GetRole(token, "Admin");

            if (!auth)
                throw new Exception("Fail authentication");

            if (!role)
                throw new Exception("You are not Admin");

            Service service = this._context.Services.Where(x => x.Id == id).FirstOrDefault();
            this._context.Remove(service);
            this._context.SaveChanges();

            foreach(var item in this._context.ServiceServiceItems.Where(x=>x.ServiceId == service.Id))
            {
                this._context.Remove(item);
                this._context.SaveChanges();
            }

            return true;
        }

        private bool GetRole(string Token, string role)
        {
            return role.Equals(Token.Split(",")[1]);
        }

        private bool VerifyToken(string Token)
        {
            HttpClient client = new HttpClient();

            string URI = "https://localhost:44311/api/User/Verify";
            client.DefaultRequestHeaders.Add("Authorization", Token);

            HttpResponseMessage responseMessage = client.GetAsync(URI).Result;

            var result = Boolean.Parse(responseMessage.Content.ReadAsStringAsync().Result);

            return result;
        }
    }
}
