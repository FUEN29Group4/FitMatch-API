using Dapper;
using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using System.Text;


namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly IDbConnection _db;

        public LoginController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpGet]
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMember(int id)
        {
            const string sql = @"SELECT MemberId,Email,Password FROM Member WHERE MemberId = @MemberId";
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

        [HttpPost("login")]
        public async Task<IActionResult> Login(Member loginMember)
        {
            const string sql = @"SELECT * FROM Member WHERE Email = @Email AND Password = @Password";
            var parameters = new { Email = loginMember.Email, Password = loginMember.Password };

            using (var multi = await _db.QueryMultipleAsync(sql, parameters))
            {
                var member = multi.Read<Member>().FirstOrDefault();
                if (member == null)
                {
                    return NotFound("Email 或 Password 錯誤");
                }
                return Ok(member);
            }
        }


    }
}
