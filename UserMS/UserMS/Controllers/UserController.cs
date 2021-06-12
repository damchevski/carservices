using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UserMS.Data;
using UserMS.Services;

namespace UserMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenBuilder _tokenBuilder;

        public UserController(
            ApplicationDbContext context,
            ITokenBuilder tokenBuilder)
        {
            _context = context;
            _tokenBuilder = tokenBuilder;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var dbUser = await _context
                .Users
                .SingleOrDefaultAsync(u => u.Username == user.Username);

            if (dbUser != null)
            {
                return BadRequest("User Exists");
            }


            if (user.Email == null || user.Username == null ||
            user.Password == null || user.Role == null){
                return BadRequest("All Fields Are Required");
            }


            user.Password = Encrypt_Password(user.Password);

            _context.Add(user);
            _context.SaveChanges();

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var dbUser = await _context
                .Users
                .SingleOrDefaultAsync(u => u.Username == user.Username);

            if (dbUser == null)
            {
                return NotFound("User not found.");
            }


            var isValid = Decrypt_Password(dbUser.Password) == user.Password;

            if (!isValid)
            {
                return BadRequest("Could not authenticate user.");
            }

            var token = _tokenBuilder.BuildToken(user.Username, dbUser.Role);

            return Ok(token);
        }

        [HttpGet("verify")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<bool> VerifyToken(string? token)
        {

            var full = "";
            if (token == null)
            {
                full = User
                .Claims
                .SingleOrDefault().Value;
            }
            else
            {
                full = token;
            }

            if (full == "")
                return false;

            var username = full.Split(",")[0];
            var role = full.Split(",")[1];

            if (username == null)
            {
                return false;
            }

            var userExists = await _context
                .Users
                .AnyAsync(u => u.Username == username);

            if (!userExists)
            {
                return false;
            }

            return true;
        }

 

        private string Encrypt_Password(string password)
        {
            string pwdstring = string.Empty;
            byte[] pwd_encode = new byte[password.Length];
            pwd_encode = Encoding.UTF8.GetBytes(password);
            pwdstring = Convert.ToBase64String(pwd_encode);
            return pwdstring;
        }

        private string Decrypt_Password(string encryptpassword)
        {
            string pwdstring = string.Empty;
            UTF8Encoding encode_pwd = new UTF8Encoding();
            Decoder Decode = encode_pwd.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(encryptpassword);
            int charCount = Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            pwdstring = new String(decoded_char);
            return pwdstring;
        }
    }

   
}
