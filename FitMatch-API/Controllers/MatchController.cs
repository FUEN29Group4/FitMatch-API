using Microsoft.AspNetCore.Http;
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
            var query = @"SELECT t.TrainerID, t.Photo FROM Trainers AS t INNER JOIN ( SELECT TOP 5 c.TrainerID FROM Class AS c GROUP BY c.TrainerID ORDER BY COUNT(c.TrainerID) DESC ) AS top_trainers ON t.TrainerID = top_trainers.TrainerID;";

            var trainersWithPhoto = await _db.QueryAsync<Trainer>(query);
            return Ok(trainersWithPhoto);
        }
    }
}
