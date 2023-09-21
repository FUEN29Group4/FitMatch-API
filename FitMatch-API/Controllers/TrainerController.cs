using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using System.Numerics;

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainerController : ControllerBase
    {
        //這頁只是範例 教練個人履歷可以用這頁改(但一定要加上權限!!)
        private readonly IDbConnection _db;
        public TrainerController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrainerByIdAsync(int id)
        {
            const string sql = @"SELECT TrainerID,TrainerName,Phone,Address,Photo,CourseFee,Introduce,Certificate,Expertise,Experience,Salt FROM Trainers where TrainerID = @TrainerId";

            var trainer = await _db.QueryFirstOrDefaultAsync<Trainer>(sql, new { TrainerId = id });

            if (trainer == null)
            {
                return NotFound("No data found");
            }


            return Ok(trainer);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            const string sql = @"SELECT * FROM Trainers";

            using (var multi = await _db.QueryMultipleAsync(sql))
            {
                var trainers = multi.Read<Trainer>().ToList();
                // 基本驗證，確保資料存在
                if (trainers == null)
                {
                    return NotFound("No data found");
                }
                return Ok(trainers);
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchTrainers(string? area, bool? gender, string? expertise)
        {
            var sql = @"SELECT * FROM Trainers WHERE 1=1 ";

            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(area))
            {
                sql += " AND Address LIKE @Area ";
                parameters.Add("@Area", "%" + area + "%");
            }

            if (gender.HasValue)
            {
                sql += " AND Gender = @Gender ";
                parameters.Add("@Gender", gender);
            }

            if (!string.IsNullOrEmpty(expertise))
            {
                sql += " AND Expertise LIKE @Expertise ";
                parameters.Add("@Expertise", "%" + expertise + "%");
            }

            using (var multi = await _db.QueryMultipleAsync(sql, parameters))
            {
                var trainers = multi.Read<Trainer>().ToList();
                if (trainers == null || !trainers.Any())
                {
                    return NotFound("No data found");
                }
                return Ok(trainers);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrainerByIdAsync(int id, [FromBody] Trainer trainerData)
        {
            const string sql = @"UPDATE Trainers SET Phone=@Phone, TrainerName = @TrainerName, Address = @Address, Certificate = @Certificate, Expertise = @Expertise, CourseFee = @CourseFee, Introduce = @Introduce, Experience = @Experience, photo = @Photo ,Salt = @Salt WHERE TrainerID = @TrainerId;";

            int rowsAffected = await _db.ExecuteAsync(sql,
                new
                {
                    
                    TrainerId = id,
                    Phone = trainerData.Phone,
                    TrainerName = trainerData.TrainerName,
                    Address = trainerData.Address,
                    Certificate = trainerData.Certificate,
                    Expertise = trainerData.Expertise,
                    CourseFee = trainerData.CourseFee,
                    Introduce = trainerData.Introduce,
                    Experience = trainerData.Experience,
                    Photo = trainerData.Photo,
                    Salt = trainerData.Salt
                });

            if (rowsAffected == 0)
            {
                return NotFound("No data found to update");
            }
            return Ok(new { status = "success", message = "教練資訊已成功更新" });
        }
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetTrainerAsync(int id)
        //{
        //    const string sql = @"SELECT * FROM Trainers Where Trainer = @TrainerId";

        //    using (var multi = await _db.QueryMultipleAsync(sql))
        //    {
        //        var trainers = multi.Read<Trainer>().ToList();
        //        // 基本驗證，確保資料存在
        //        if (trainers == null)
        //        {
        //            return NotFound("No data found");
        //        }
        //        return Ok(trainers);
        //    }
        //}
    }
}
