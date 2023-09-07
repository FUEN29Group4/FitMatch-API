using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using FitMatch_API.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberFavoriteController : ControllerBase
    {

        //======宣告變數跟串資料庫=======

        private readonly IDbConnection _context;//宣吿類別級變數，串資料庫
        private readonly ILogger<OrderAPIController> _logger; //宣吿類別級變數，串登入資訊

        //定義_context
        public MemberFavoriteController(IConfiguration configuration)
        {
            _context = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        // R: 讀取所有MemberFavorite列表資料 => ok
        // GET: api/<MemberFavoriteController>
        [HttpGet]
        public async Task<IActionResult> GetALLMemberFavorite()
        {
            const string sql = @"SELECT * FROM [MemberFavorite]";

            using (var multi = await _context.QueryMultipleAsync(sql))
            {
                var MemberFavorites = multi.Read<MemberFavorite>().ToList();
                // 基本驗證，確保資料存在
                if (MemberFavorites == null)
                {
                    return NotFound("No data found");
                }
                return Ok(MemberFavorites);
            }
        }

        // GET api/<MemberFavoriteController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMemberFavorites(int id)
        {
            const string sql = @"SELECT * FROM [MemberFavorite] WHERE MemberId = @MemberId";
            var parameters = new { MemberId = id };

            using (var multi = await _context.QueryMultipleAsync(sql, parameters))
            {
                var memberFavorites = multi.Read<MemberFavorite>().ToList();
                // 基本驗證，確保資料存在
                if (memberFavorites == null || memberFavorites.Count == 0)
                {
                    return NotFound("No data found");
                }
                return Ok(memberFavorites);
            }
        }


        //// POST api/<MemberFavoriteController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<MemberFavoriteController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<MemberFavoriteController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
