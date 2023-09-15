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
                    b.GymID, b.GymName,b.OpentimeStart,b.OpentimeEnd,
                    c.MemberID, c.MemberName, 
                     
                    e.TrainerName,e.TrainerID
                    FROM Class AS a
                    INNER JOIN Gyms AS b ON a.GymID = b.GymID
                    INNER JOIN Member AS c ON a.MemberID = c.MemberID
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
        //[HttpGet]
        //public async Task<IActionResult> GetAllGym()
        //{
        //    const string sql = @"select * from Gyms; ";
        //   var gyms = await _db.QueryAsync<Gym>(sql);
        //    return Ok(gyms);
        //}



        // POST api/<TrainerClassController>


        //[HttpGet("{id}/check")]
        //public async Task<IActionResult> Check(int id)
        //{
        //    const string sql = @"SELECT a.ClassID, a.CourseStatus, a.StartTime, a.BuildTime, a.EndTime, 
        //            b.GymID, b.GymName,b.OpentimeStart,b.OpentimeEnd,
        //            c.MemberID, c.MemberName, 
        //            d.ClassTypeID, d.ClassName,
        //            e.TrainerName,e.TrainerID
        //            FROM Class AS a
        //            INNER JOIN Gyms AS b ON a.GymID = b.GymID
        //            INNER JOIN Member AS c ON a.MemberID = c.MemberID
        //            INNER JOIN ClassTypes AS d ON a.ClassTypeID = d.ClassTypeID
        //            INNER JOIN Trainers AS e ON a.TrainerID = e.TrainerID
        //            WHERE a.TrainerID = @TrainerId ;"; // 保持原样
        //    var parameters = new { TrainerId = id };

        //    using (var multi = await _db.QueryMultipleAsync(sql, parameters))
        //    {
        //        var TrainerClass = multi.Read<TrainerClassDTO>().ToList();
        //        if (TrainerClass == null || TrainerClass.Count == 0)
        //        {
        //            return NotFound("No data found");
        //        }
        //        return Ok(TrainerClass);
        //    }
        //}
        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] TrainerClassDTO newClass)
        //{

        //    const string sql = @"INSERT INTO Class (ClassTypeID, TrainerID, GymID, StartTime, EndTime, CourseStatus, BuildTime)
        //                     VALUES (@ClassTypeID, @TrainerID, @GymID, @StartTime, @EndTime, @CourseStatus, GETDATE());";

        //    try
        //    {
        //        var parameters = new
        //        {
        //            ClassTypeID = newClass.ClassTypeId,
        //            TrainerID = newClass.TrainerID,
        //            GymID = newClass.GymId,
        //            StartTime = newClass.StartTime,
        //            EndTime = newClass.EndTime,
        //            CourseStatus = newClass.CourseStatus
        //        };

        //        await _db.ExecuteAsync(sql, parameters);

        //        return Ok(); // 插入成功
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message); // 如果出错，返回错误详情
        //    }
        //}

        //[HttpPut("{id}")]
        //public async Task<IActionResult> Update(int id, [FromBody] Class updatedClass)
        //{
        //    return Ok();
        //    // 更新课程的逻辑
        //    // 如果成功，返回 Ok()
        //    // 如果出错，返回 BadRequest(errorDetails)
        //}
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    return Ok();
        //    // 删除课程的逻辑
        //    // 如果成功，返回 Ok()
        //    // 如果出错，返回 BadRequest(errorDetails)
        //}





    }
}
