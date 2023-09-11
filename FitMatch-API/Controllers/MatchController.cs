﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using FitMatch_API.Models;
using System.Data.Common;

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
        [HttpGet("TrainerTOP5")]
        public async Task<IActionResult> GetTOP5()
        {
            // 查詢TOP5 TrainerID和Photo
            var query = @"SELECT t.TrainerID,t.TrainerName, t.Photo FROM Trainers AS t INNER JOIN ( SELECT TOP 5 c.TrainerID FROM Class AS c GROUP BY c.TrainerID ORDER BY COUNT(c.TrainerID) DESC ) AS top_trainers ON t.TrainerID = top_trainers.TrainerID WHERE t.Approved = 1;";

            var trainersWithPhoto = await _db.QueryAsync<Trainer>(query);
            return Ok(trainersWithPhoto);
        }
        [HttpGet("GymTOP4")]
        public async Task<IActionResult> GetGyms()
        {
            const string sql = @"SELECT TOP 4 GymID, GymName, Photo FROM Gyms";

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
        {
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
    }
}
