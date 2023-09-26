using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using FitMatch_API.Models;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        private readonly IDbConnection _db;

        public MatchController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }
        [HttpGet("Trainer")]
        public async Task<IActionResult> GetTOP()
        {

            //var query = @"SELECT t.TrainerID,t.TrainerName, t.Photo FROM Trainers AS t INNER JOIN ( SELECT TOP 5 c.TrainerID FROM Class AS c GROUP BY c.TrainerID ORDER BY COUNT(c.TrainerID) DESC ) AS top_trainers ON t.TrainerID = top_trainers.TrainerID WHERE t.Approved = 1;";

            // 查詢最活躍的教練(依照開課數量)
            //var query = @"SELECT  COUNT(c.ClassID) AS ClassCount, t.TrainerID, t.TrainerName, t.Photo, t.Gender, t.Address, t.Certificate, t.Expertise, t.Experience, t.CourseFee, t.Introduce
            //    FROM Trainers AS t
            //    LEFT JOIN Class AS c ON t.TrainerID = c.TrainerID
            //    WHERE t.Approved = 1
            //    GROUP BY t.TrainerID, t.TrainerName, t.Photo, t.Gender, t.Address, t.Certificate, t.Expertise, t.Experience, t.CourseFee, t.Introduce;";
           
            // 查詢最熱門的教練(依照課程預約率)
            var query = @"SELECT t.TrainerID, t.TrainerName, t.Photo, t.Gender, t.Address, t.Certificate, t.Expertise, t.Experience, t.CourseFee, t.Introduce, COUNT(c.MemberID) AS MemberCount FROM Trainers AS t LEFT JOIN Class AS c ON t.TrainerID = c.TrainerID WHERE t.Approved = 1 AND c.MemberID IS NOT NULL GROUP BY t.TrainerID, t.TrainerName, t.Photo, t.Gender, t.Address, t.Certificate, t.Expertise, t.Experience, t.CourseFee, t.Introduce ORDER BY MemberCount DESC; ";

            var trainerAll = await _db.QueryAsync<Trainer>(query);
            return Ok(trainerAll);
        }
        [HttpGet("Gym")]
        public async Task<IActionResult> GetGyms()
        {//查場館前五筆
            const string sql = @"SELECT * FROM Gyms";

            using (var multi = await _db.QueryMultipleAsync(sql))
            {
                var gyms = multi.Read<Gym>().ToList();
                // 基本驗證，確保資料存在
                if (gyms == null)
                {
                    return NotFound("No data found");
                }
                return Ok(gyms);
            }
        }
        [HttpGet("Reviews")]
        public async Task<IActionResult> GetReviews()
        {//查評價
            const string sql = @"select r.MemberID,r.Comment,m.MemberName from Reviews as r inner join Member as m on r.MemberID=m.MemberID";

            using (var multi = await _db.QueryMultipleAsync(sql))
            {
                var Reviews = multi.Read<Review>().ToList();
                if (Reviews == null)
                {
                    return NotFound("No data found");
                }
                return Ok(Reviews);
            }
        }
        [HttpGet("ClassTypes")]
        public async Task<IActionResult> GetClassTypes()
        {
            const string sql = @"SELECT * FROM ClassTypes WHERE Status > 0;";

            using (var multi = await _db.QueryMultipleAsync(sql))
            {
                var ClassTypes = multi.Read<ClassType>().ToList();
                if (ClassTypes == null)
                {
                    return NotFound("No data found");
                }
                return Ok(ClassTypes);
            }
        }
        //[HttpGet("Trainer")]
        //public async Task<IActionResult> GetALL()
        //{
        //    // 查詢所有教練不含個資訊息
        //    var query = @"SELECT TrainerID, TrainerName, Gender,Address,Certificate,Expertise,Experience,CourseFee,Introduce, Photo FROM Trainers WHERE Approved = 1;";
        //    using (var multi = await _db.QueryMultipleAsync(query))
        //    {
        //        var Trainer = multi.Read<Trainer>().ToList();
        //        if (Trainer == null)
        //        {
        //            return NotFound("No data found");
        //        }
        //        return Ok(Trainer);
        //    }
        //}
    }
}
