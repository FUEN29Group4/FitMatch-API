using Dapper;
using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        // 網址 :  api/Member


        private readonly IDbConnection _db;

        public MemberController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }


        // GET: api/<MemberController>
        [HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}
        public async Task<IActionResult> GetAllAsync()
        {
            const string sql = @"SELECT * FROM Member";

            using (var multi = await _db.QueryMultipleAsync(sql))
            {
                var Member = multi.Read<Member>().ToList();
                // 基本驗證，確保資料存在
                if (Member == null)
                {
                    return NotFound("No data found");
                }
                return Ok(Member);
            }
        }

        
        // GET api/<MemberController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMember(int id)
        {
            const string sql = @"SELECT * FROM Member WHERE MemberId = @MemberId";
            var parameters = new { MemberId = id };

            using (var multi = await _db.QueryMultipleAsync(sql, parameters))
            {
                var member = multi.Read<Member>().FirstOrDefault();
                // 基本驗證，確保資料存在
                if (member == null)
                {
                    return NotFound("No data found");
                }
                return Ok(member);
            }
        }

        // POST api/<MemberController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }



        // 模拟一个简单的用户数据库，用List表示用户信息
        //private static List<User> users = new List<User>
        //{
        //    new User { Id = 1, Username = "user1", Email = "user1@example.com", Password = "password1" },
        //    new User { Id = 2, Username = "user2", Email = "user2@example.com", Password = "password2" }
        //};

        //[HttpPut("{id}")]
        //public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
        //{
        //    // 检查用户是否存在
        //    var existingUser = users.FirstOrDefault(u => u.Id == id);
        //    if (existingUser == null)
        //    {
        //        return NotFound("用户不存在");
        //    }

        //    // 验证数据
        //    if (string.IsNullOrWhiteSpace(updatedUser.Email) || string.IsNullOrWhiteSpace(updatedUser.Password))
        //    {
        //        return BadRequest("必须提供有效的电子邮件和密码");
        //    }

        //    // 更新用户信息
        //    existingUser.Email = updatedUser.Email;
        //    existingUser.Password = updatedUser.Password;

        //    // 返回成功响应
        //    return Ok("用户资料已成功更新");
        //}



        // DELETE api/<MemberController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
