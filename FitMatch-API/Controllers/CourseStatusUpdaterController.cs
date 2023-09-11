using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using FitMatch_API.Models;

namespace FitMatch_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseStatusUpdaterController : ControllerBase
    {
        private readonly IDbConnection _db;
        public CourseStatusUpdaterController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpGet("updateStatus")]
        public async Task<IActionResult> UpdateCourseStatus()
        {
            const string sql = @"SELECT a.ClassID, a.CourseStatus, a.StartTime, a.EndTime, a.MemberID
                                 FROM Class AS a
                                 WHERE a.EndTime < @CurrentTime";

            var parameters = new { CurrentTime = DateTime.Now };

            using (var multi = await _db.QueryMultipleAsync(sql, parameters))
            {
                var courses = multi.Read<Class>().ToList();
                if (courses == null || courses.Count == 0)
                {
                    return NotFound("No outdated courses found");
                }

                foreach (var course in courses)
                {
                    string newStatus;
                    if (course.MemberId > 0)
                    {
                        newStatus = "已完成";
                    }
                    else
                    {
                        newStatus = "已過期";
                    }

                    const string updateSql = "UPDATE Class SET CourseStatus = @NewStatus WHERE ClassID = @ClassID";
                    await _db.ExecuteAsync(updateSql, new { NewStatus = newStatus, ClassID = course.ClassId });
                }

                return Ok("課程狀態更新");
            }
        }
    }
}
