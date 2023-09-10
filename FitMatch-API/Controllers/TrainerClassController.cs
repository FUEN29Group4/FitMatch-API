using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using FitMatch_API.Models;
using System.Linq;
using System.Security.Claims;
using FitMatch_API.DTO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainerClassController : ControllerBase
    {
        private readonly IDbConnection _db;
        public TrainerClassController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }
        // GET: api/<TrainerClassController>



        // GET api/<TrainerClassController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAllTrainerClass(int id)
        {
            const string sql = @"SELECT a.ClassID, a.CourseStatus, a.StartTime, a.BuildTime, a.EndTime, 
                    b.GymID, b.GymName,
                    c.MemberID, c.MemberName, 
                    d.ClassTypeID, d.ClassName,
                    e.TrainerName,e.TrainerID
                    FROM Class AS a
                    INNER JOIN Gyms AS b ON a.GymID = b.GymID
                    INNER JOIN Member AS c ON a.MemberID = c.MemberID
                    INNER JOIN ClassTypes AS d ON a.ClassTypeID = d.ClassTypeID
                    INNER JOIN Trainers AS e ON a.TrainerID = e.TrainerID
                    WHERE a.TrainerID = @TrainerId ;";
            var parameters = new { TrainerId = id };

            using (var multi = await _db.QueryMultipleAsync(sql, parameters))
            {
                var TrainerClass = multi.Read<TrainerClassDTO>().ToList();
                if (TrainerClass == null || TrainerClass.Count == 0)
                {
                    return NotFound("No data found");
                }
                return Ok(TrainerClass);
            }
        }

        // POST api/<TrainerClassController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<TrainerClassController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<TrainerClassController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
