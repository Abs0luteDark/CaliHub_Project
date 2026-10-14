using Microsoft.AspNetCore.Mvc;
using CaliHub_DAL;
using CaliHub_Model;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CaliHub_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserDB _userDb;

        // הזרקת ה-UserDB שנרשם ב-Program.cs
        public UsersController(UserDB userDb)
        {
            _userDb = userDb;
        }

        // GET: api/users - שליפת כל המשתמשים
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userDb.GetAllUsersAsync();
            return Ok(users); // 200 OK
        }

        // GET: api/users/5 - שליפת משתמש לפי מזהה
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userDb.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." }); // 404 Not Found
            }
            return Ok(user); // 200 OK
        }

        // GET: api/users/count-by-role - ספירת משתמשים לפי תפקיד
        [HttpGet("count-by-role")]
        public async Task<IActionResult> CountUsersByRole()
        {
            var counts = await _userDb.CountUsersByRoleAsync();
            return Ok(counts); // 200 OK
        }

        // POST: api/users/login - בדיקת התחברות
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userDb.CheckLoginAsync(request.Username, request.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password." }); // 401 Unauthorized
            }
            return Ok(user); // 200 OK
        }

        // POST: api/users - הוספת משתמש חדש
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            if (await _userDb.UsernameExistsAsync(user.Username))
            {
                return Conflict(new { message = "Username is already taken." }); // 409 Conflict
            }

            if (await _userDb.EmailExistsAsync(user.Email))
            {
                return Conflict(new { message = "Email is already in use." }); // 409 Conflict
            }

            int newId = await _userDb.AddUserAsync(user);
            user.Id = newId;

            return CreatedAtAction(nameof(GetUserById), new { id = newId }, user); // 201 Created
        }

        // PUT: api/users/5/role - עדכון תפקיד משתמש
        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] int newRoleId)
        {
            var user = await _userDb.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." }); // 404 Not Found
            }

            bool success = await _userDb.UpdateUserRoleAsync(id, newRoleId);
            if (!success)
            {
                return BadRequest(new { message = "Failed to update role." }); // 400 Bad Request
            }

            return NoContent(); // 204 No Content
        }

        // PUT: api/users/5/password - עדכון סיסמה
        [HttpPut("{id}/password")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] string newPassword)
        {
            var user = await _userDb.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." }); // 404 Not Found
            }

            bool success = await _userDb.UpdatePasswordAsync(id, newPassword);
            if (!success)
            {
                return BadRequest(new { message = "Failed to update password." }); // 400 Bad Request
            }

            return NoContent(); // 204 No Content
        }

        // PUT: api/users/5/email - עדכון אימייל
        [HttpPut("{id}/email")]
        public async Task<IActionResult> UpdateEmail(int id, [FromBody] string newEmail)
        {
            var user = await _userDb.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." }); // 404 Not Found
            }

            if (await _userDb.EmailExistsAsync(newEmail))
            {
                return Conflict(new { message = "Email is already in use." }); // 409 Conflict
            }

            bool success = await _userDb.UpdateEmailAsync(id, newEmail);
            if (!success)
            {
                return BadRequest(new { message = "Failed to update email." }); // 400 Bad Request
            }

            return NoContent(); // 204 No Content
        }
    }

    // מודל עזר קטן עבור בקשת להתחברות
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}