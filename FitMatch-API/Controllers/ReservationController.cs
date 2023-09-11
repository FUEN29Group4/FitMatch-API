using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;

namespace FitMatch_API.Controllers
{  
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IDbConnection _db;

        public ReservationController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{ //要查class裡面的 trainers跟gyms資料 sql還沒寫好 只是先試查而已
        //    const string sql = @"SELECT * FROM Trainers;
        //                         SELECT * FROM Gyms;
        //                         SELECT * FROM Class;";

        //    using (var multi = await _db.QueryMultipleAsync(sql))
        //    {
        //        var trainers = multi.Read<Trainer>().ToList();
        //        var gyms = multi.Read<Gym>().ToList();
        //        var classes = multi.Read<Class>().ToList();
        //        // 基本驗證，確保資料存在
        //        if (trainers == null || gyms == null || classes == null)
        //        {
        //            return NotFound("No data found");
        //        }
        //        return Ok(new { Trainers = trainers, Gyms = gyms, Classes = classes });
        //    }
        //}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // 參數化查詢，防止SQL注入
            //var trainer = await _db.QueryFirstOrDefaultAsync<Trainer>("select TrainerName,photo,Certificate,Experience,Expertise,CourseFee,Approved from Trainers where TrainerID = @Id AND Approved = 1", new { Id = id });
            string sql = @"SELECT t.TrainerID ,t.TrainerName, t.Photo, t.Certificate, t.Experience, t.Expertise, t.CourseFee, c.ClassID, c.ClassTypeID, c.GymID, c.MemberID, c.StartTime, c.EndTime, c.CourseStatus, g.GymID, g.GymName, g.Phone, g.Address,g.Photo FROM Trainers as t INNER JOIN Class as c ON t.TrainerID = c.TrainerID INNER JOIN Gyms as g ON c.GymID = g.GymID WHERE t.TrainerID = @Id AND t.Approved = 1";

            var lookup = new Dictionary<int, Trainer>();
            await _db.QueryAsync<Trainer, Class, Gym, Trainer>(
                sql,
                (trainer, classDetails, gymDetails) =>
                {
                    Trainer trainerEntry;

                    if (!lookup.TryGetValue(trainer.TrainerId, out trainerEntry))
                    {
                        lookup.Add(trainer.TrainerId, trainerEntry = trainer);
                        trainerEntry.Classes = new List<Class>();
                        trainerEntry.Gyms = new List<Gym>();
                    }

                    trainerEntry.Classes.Add(classDetails);
                    trainerEntry.Gyms.Add(gymDetails);
                    return trainerEntry;
                },
                new { Id = id },
                splitOn: "ClassID, GymID");

            var result = lookup.Values.FirstOrDefault();

            if (result == null)
            {
                return NotFound("查無此教練");
            }

            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> MakeReservation([FromBody] ReservationData ReservationData)
        {
            if (ReservationData == null)
            {
                return BadRequest("找不到data");
            }

            // 更新課程記錄
            string sql = @"UPDATE Class SET MemberID = @MemberId WHERE ClassID = @ClassId";

            var parameters = new
            {
                ClassId = ReservationData.ClassId,
                MemberId = ReservationData.MemberId,
            };

            try
            {
                int rowsAffected = await _db.ExecuteAsync(sql, parameters);

                if (rowsAffected > 0)
                {
                    return Ok("預約成功");
                }
            }
            catch (Exception ex)
            {
                // Log exception here
                return StatusCode(500, $"錯誤訊息: {ex}");
            }

            return BadRequest("無法預約");
        }
        // 接收的模型
        public class ReservationData
        {
            public int ClassId { get; set; }
            public int MemberId { get; set; }
        }

    }
}
