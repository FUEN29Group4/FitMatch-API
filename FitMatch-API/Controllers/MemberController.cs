using Dapper;
using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

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


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMemberByIdAsync(int id, [FromBody] Member MemberData)
        {
            //// 初始化一个空的字符串列表来存储需要更新的列
            //List<string> updateClauses = new List<string>();

            //// 使用条件语句检查每个要更新的列
            //if (!string.IsNullOrEmpty(MemberData.MemberName))
            //{
            //    updateClauses.Add("MemberName = @MemberName");
            //}

            //if (!string.IsNullOrEmpty(MemberData.Address))
            //{
            //    updateClauses.Add("Address = @Address");
            //}

            //if (!string.IsNullOrEmpty(MemberData.Phone))
            //{
            //    updateClauses.Add("Phone = @Phone");
            //}

            //if (!string.IsNullOrEmpty(MemberData.Email))
            //{
            //    updateClauses.Add("Email = @Email");
            //}

            //if (!string.IsNullOrEmpty(MemberData.Password))
            //{
            //    updateClauses.Add("Password = @Password");
            //}

            //if (!string.IsNullOrEmpty(MemberData.Photo))
            //{
            //    updateClauses.Add("Photo = @Photo");
            //}

            //// 构建SQL查询字符串
            //string updateColumns = string.Join(", ", updateClauses);
            //string sql = $@"UPDATE Member SET {updateColumns} WHERE MemberID = @MemberId;";

            const string sql = @"UPDATE Member SET MemberName = @MemberName, Address = @Address, Phone = @Phone, Email = @Email,  Password = @Password , Photo = @Photo WHERE MemberID = @MemberId;";

            int rowsAffected = await _db.ExecuteAsync(sql,
                new
                {
                    MemberId = id,
                    MemberName = MemberData.MemberName,
                    Address = MemberData.Address,
                    Phone = MemberData.Phone,
                    Email = MemberData.Email,
                    Password = MemberData.Password,
                    Photo = MemberData.Photo,

                });

            if (rowsAffected == 0)
            {
                return NotFound("No data found to update");
            }
            return Ok(new { status = "success", message = "會員資料已成功更新" });
        }


        // DELETE api/<MemberController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
