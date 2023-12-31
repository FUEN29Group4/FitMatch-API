﻿using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using System.Numerics;
using static FitMatch_API.Controllers.ReservationController;

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
            const string sql = @"SELECT TrainerID,TrainerName,Phone,Address,Photo,CourseFee,Introduce,Certificate,Expertise,Experience,Salt,Approved FROM Trainers where TrainerID = @TrainerId";

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
        [HttpPost("FavoriteTrainer")]
        public async Task<IActionResult> InsertFavoriteTrainer([FromBody] FavoriteTrainer favoriteTrainer)
        {//接收教練收藏
            if (favoriteTrainer == null)
            {
                return BadRequest("找不到data");
            }
            string sql = @"insert into MemberFavorite(MemberID,TrainerID) values (@MemberId,@TrainerId);";

            var parameters = new
            {
                MemberId = favoriteTrainer.MemberId,
                TrainerId = favoriteTrainer.TrainerId,
            };
            try
            {
                int rowsAffected = await _db.ExecuteAsync(sql, parameters);

                if (rowsAffected > 0)
                {
                    return Ok(new
                    {
                        status = "success",
                        message = "收藏成功"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = $"内部服务器错误: {ex.Message}"
                });
            }

            return BadRequest("無法加入收藏");
        }
        // 接收的模型
        public class FavoriteTrainer
        {
            public int MemberId { get; set; }
            public int TrainerId { get; set; }
        }
        [HttpGet("GetFavoriteTrainer/{id}")]
        public async Task<IActionResult> GetFavoriteTrainer(int id)
        {//查詢收藏
            try
            {
                // Your SQL query to retrieve all records for the given MemberId
                string sqlQuery = "SELECT * FROM MemberFavorite WHERE MemberId = @MemberId AND (TrainerId IS NOT NULL OR ProductId IS NOT NULL)";

                // Execute the query using Dapper
                var favoriteRecords = await _db.QueryAsync<SimpleMemberFavorite>(sqlQuery, new { MemberId = id });

                if (favoriteRecords != null && favoriteRecords.Any())
                {
                    var response = new
                    {
                        status = "success",
                        message = "收藏查詢成功",
                        data = favoriteRecords 
                    };

                    return Ok(favoriteRecords);
                }
                else
                {
                    var response = new
                    {
                        status = "error",
                        message = "找不到具有TrainerId或ProductId的紀錄"
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                var response = new
                {
                    status = "error",
                    message = $"內部服務器錯誤: {ex.Message}"
                };
                return StatusCode(500, response);
            }
        }
        //自訂模型 因為原本模型太多東西了
        public class SimpleMemberFavorite
        {
            public int MemberFavoriteId { get; set; }
            public int? MemberId { get; set; }
            public int? TrainerId { get; set; }
            public int? ProductId { get; set; }
        }



        //讀取top4教練

        [HttpGet("GetTopTrainer")]
        public async Task<IActionResult> GetTopTrainer()
        {
            const string sql = @"
            WITH RankedTrainers AS (
                SELECT  T.TrainerID,
                        T.TrainerName,
                        T.Gender,
                        T.Photo,
                        T.Expertise,
                        ROW_NUMBER() OVER (ORDER BY COUNT(C.TrainerID) DESC) AS 'Rank'
                FROM    Trainers AS T 
                LEFT JOIN Class AS C ON T.TrainerID = C.TrainerID AND C.CourseStatus= N'已完成'
                GROUP BY T.TrainerID, T.TrainerName, T.Gender, T.Photo, T.Expertise
            )
            SELECT DISTINCT [TrainerID],
                    [TrainerName],
                    [Gender],
                    [Photo],
                    [Expertise]
        
            FROM    RankedTrainers
            WHERE   Rank <= 4
            ";

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








    }
}
