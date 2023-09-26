using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;


namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class examineController : ControllerBase
    {
        private readonly IDbConnection _db;
        public examineController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }




        // GET api/<examineController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAllAsync(int id)
        {

            const string sql = @"SELECT TrainerID FROM trainers where Approved = 1 and trainerid = @trainerid;";
            var parameters = new { trainerid = id };

            var trainers = await _db.QueryFirstOrDefaultAsync<Trainer>(sql, parameters);
            if (trainers == null)
            {
                return NotFound("No data found");
            }
            return Ok(trainers);
        }

    }
}
