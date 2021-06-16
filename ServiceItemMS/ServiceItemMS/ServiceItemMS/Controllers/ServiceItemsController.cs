using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceItemMS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceItemMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
        public class ServiceItemsController : ControllerBase
        {
            private readonly ApplicationDbContext _context;

            public ServiceItemsController(ApplicationDbContext context)
            {
                this._context = context;
            }

            // GET: api/<ServiceItemsController>
            [HttpGet]
            [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
            public async Task<List<ServiceItem>> Get()
            {
                var auth = this.VerifyToken(Request.Headers["Authorization"]);
                if (!auth)
                    throw new Exception("Fail authentication");

                return this._context.ServiceItems.ToList();
            }

            // GET api/<ServiceItemsController>/5
            [HttpGet("{id}")]
            [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
            public async Task<ServiceItem> Get( int id)
            {
                var auth = this.VerifyToken(Request.Headers["Authorization"]);
                if (!auth)
                    throw new Exception("Fail authentication");

                return this._context.ServiceItems.Where(x => x.Id == id).FirstOrDefault();
            }

            // POST api/<ServiceItemsController>
            [HttpPost]
            [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
            public async Task<ServiceItem> Post([FromBody] ServiceItem serviceItem)
            {
                var token = User.Claims.SingleOrDefault().Value;

                var auth = this.VerifyToken(Request.Headers["Authorization"]);
                var role = this.GetRole(token, "Admin");

                if (!auth)
                    throw new Exception("Fail authentication");

                if (!role)
                    throw new Exception("You are not Admin");

                if (serviceItem.Name == null || serviceItem.Price == null)
                {
                    return null;
                }

                ServiceItem findServiceItem = this._context.ServiceItems.Where(x => x.Name == serviceItem.Name).FirstOrDefault();

                if (findServiceItem != null)
                    return findServiceItem;

                this._context.Add(serviceItem);
                this._context.SaveChanges();

                return serviceItem;
            }


            // DELETE api/<ServiceItemsController>/5
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

                ServiceItem serviceItem = this._context.ServiceItems.Where(x => x.Id == id).FirstOrDefault();
                this._context.Remove(serviceItem);
            this._context.SaveChanges();

                return true;
            }


            private bool GetRole(string Token, string role)
            {
                return role.Equals(Token.Split(",")[1]);
            }

            private bool VerifyToken(string Token)
            {
                HttpClient client = new HttpClient();

                string URI = "https://localhost:5001/api/User/Verify";
                client.DefaultRequestHeaders.Add("Authorization", Token);

                HttpResponseMessage responseMessage = client.GetAsync(URI).Result;

                var result = Boolean.Parse(responseMessage.Content.ReadAsStringAsync().Result);

                return result;
            }
        }
    }

