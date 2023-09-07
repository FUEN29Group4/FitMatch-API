using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;

namespace FitMatch_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationForClassTypeController : ControllerBase
    {
        private readonly IDbConnection _db;

        public ReservationForClassTypeController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByClassId(int id)
        {
            string sql = @"SELECT t.TrainerID ,t.TrainerName, t.Photo, t.Certificate, t.Experience, t.Expertise, t.CourseFee, c.ClassID, c.ClassTypeID,  c.MemberID, c.StartTime, c.EndTime, c.CourseStatus,ct.ClassName,ct.Introduction,ct.Photo FROM Trainers as t INNER JOIN Class as c ON t.TrainerID = c.TrainerID INNER JOIN ClassTypes as ct ON c.ClassTypeID = ct.ClassTypeID WHERE t.TrainerID = @Id";

            var lookup = new Dictionary<int, Trainer>();
            await _db.QueryAsync<Trainer, Class, ClassType, Trainer>(
                sql,
                (trainer, classDetails, classtypeDetails) =>
                {
                    Trainer trainerEntry;

                    if (!lookup.TryGetValue(trainer.TrainerId, out trainerEntry))
                    {
                        lookup.Add(trainer.TrainerId, trainerEntry = trainer);
                        trainerEntry.Classes = new List<Class>();
                        trainerEntry.ClassTypes = new List<ClassType>();
                    }

                    trainerEntry.Classes.Add(classDetails);
                    trainerEntry.ClassTypes.Add(classtypeDetails);
                    return trainerEntry;
                },
                new { Id = id },
                splitOn: "ClassID, ClassTypeID");

            var result = lookup.Values.FirstOrDefault();

            if (result == null)
            {
                return NotFound("查無此教練相關的課程");
            }

            return Ok(result);
        }
    }
}
