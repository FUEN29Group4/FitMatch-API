using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    public class IndexController : Controller
    {
        //======宣告變數跟串資料庫=======

        private readonly IDbConnection _db;//宣吿類別級變數，串資料庫
        private readonly ILogger<OrderAPIController> _logger; //宣吿類別級變數，串登入資訊

        //定義_context
        public IndexController(IConfiguration configuration)
        {
            _db= new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }



        //獲取會員、教練、開課數、健身房各自的總數量 => ok 
        [HttpGet("GetCounts")]
        public async Task<IActionResult> GetCounts()
        {
            try
            {
                const string memberSql = "SELECT COUNT(MemberID) FROM Member;";
                const string trainerSql = "SELECT COUNT(TrainerId) FROM Trainers";
                const string classSql = "SELECT COUNT(ClassId) FROM Class";
                const string gymSql = "SELECT COUNT(GymId) FROM Gyms";

                // 查詢每一個資料表的ID總數
                var memberCount = await _db.ExecuteScalarAsync<int>(memberSql);
                var trainerCount = await _db.ExecuteScalarAsync<int>(trainerSql);
                var classCount = await _db.ExecuteScalarAsync<int>(classSql);
                var gymCount = await _db.ExecuteScalarAsync<int>(gymSql);

                // 建立回應物件
                var result = new
                {
                    MemberCount = memberCount,
                    TrainerCount = trainerCount,
                    ClassCount = classCount,
                    GymCount = gymCount
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}

