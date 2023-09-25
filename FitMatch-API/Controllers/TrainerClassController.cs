using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using FitMatch_API.Models;
using System.Linq;
using System.Security.Claims;
using FitMatch_API.DTO;
using System.Reflection;

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
            const string sql = @"SELECT a.ClassID, a.CourseStatus, a.CourseUnitPrice, a.StartTime, a.BuildTime, a.EndTime, 
        b.GymID, b.GymName, b.Address, b.OpentimeStart, b.OpentimeEnd,a.VenueReservationID,v.VenueReservationDate,
        c.MemberID, c.MemberName, 
        e.TrainerName, e.TrainerID,e.CourseFee
        FROM Class AS a
        INNER JOIN Gyms AS b ON a.GymID = b.GymID
        LEFT JOIN Member AS c ON a.MemberID = c.MemberID
        INNER JOIN Trainers AS e ON a.TrainerID = e.TrainerID
        INNER JOIN VenueReservation AS v ON a.VenueReservationID = v.VenueReservationID
        WHERE a.TrainerID = @TrainerId ;";

            var parameters = new { TrainerId = id };

            using (var multi = await _db.QueryMultipleAsync(sql, parameters))
            {
                var TrainerClass = multi.Read<TrainerClassDTO>().ToList();
                if (TrainerClass == null || TrainerClass.Count == 0)
                {
                    return NotFound("No data found");
                }

                TimeZoneInfo taipeiZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");

                //foreach (var trainerClass in TrainerClass)
                //{
                //    if (trainerClass.StartTime.HasValue)
                //    {
                //        var startTimeOffset = new DateTimeOffset(trainerClass.StartTime.Value, taipeiZone.GetUtcOffset(trainerClass.StartTime.Value));
                //        trainerClass.StartTimeOffset = startTimeOffset;
                //    }

                //    if (trainerClass.BuildTime.HasValue)
                //    {
                //        var buildTimeOffset = new DateTimeOffset(trainerClass.BuildTime.Value, taipeiZone.GetUtcOffset(trainerClass.BuildTime.Value));
                //        trainerClass.BuildTimeOffset = buildTimeOffset;
                //    }

                //    if (trainerClass.EndTime.HasValue)
                //    {
                //        var endTimeOffset = new DateTimeOffset(trainerClass.EndTime.Value, taipeiZone.GetUtcOffset(trainerClass.EndTime.Value));
                //        trainerClass.EndTimeOffset = endTimeOffset;
                //    }

                //    if (trainerClass.OpentimeStart.HasValue)
                //    {
                //        var opentimeStartOffset = new DateTimeOffset(trainerClass.OpentimeStart.Value, taipeiZone.GetUtcOffset(trainerClass.OpentimeStart.Value));
                //        trainerClass.OpentimeStartOffset = opentimeStartOffset;
                //    }

                //    if (trainerClass.OpentimeEnd.HasValue)
                //    {
                //        var opentimeEndOffset = new DateTimeOffset(trainerClass.OpentimeEnd.Value, taipeiZone.GetUtcOffset(trainerClass.OpentimeEnd.Value));
                //        trainerClass.OpentimeEndOffset = opentimeEndOffset;
                //    }

                //    if (trainerClass.VenueReservationDate.HasValue)
                //    {
                //        var venueReservationDateOffset = new DateTimeOffset(trainerClass.VenueReservationDate.Value, taipeiZone.GetUtcOffset(trainerClass.VenueReservationDate.Value));
                //        trainerClass.VenueReservationDateOffset = venueReservationDateOffset;
                //    }
                //}

                return Ok(TrainerClass);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllGym()
        {
            const string sql = @"select GymID,GymName,Phone,Address,OpentimeStart,OpentimeEnd,Approved,GymDescription from Gyms; ";

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
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetDetailById(int id)
        {
            const string sql = @"SELECT a.ClassID, a.CourseStatus, a.CourseUnitPrice, a.StartTime, a.BuildTime, a.EndTime, 
       b.GymID, b.GymName, b.Address, b.OpentimeStart, b.OpentimeEnd,
       c.MemberID, c.MemberName, 
       e.TrainerName, e.TrainerID,e.CourseFee
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


        [HttpPost("VenueReservation")]
        public async Task<IActionResult> CreateOrUpdateVenueReservation(VenueReservation model)
        {
            if (model == null)
            {
                return BadRequest("Invalid data.");
            }

            const string checkExistenceSql = @"SELECT * FROM VenueReservation WHERE TrainerId = @TrainerId AND VenueReservationDate = @VenueReservationDate";
            var existingReservation = (await _db.QueryAsync<VenueReservation>(checkExistenceSql, new { model.TrainerId, model.VenueReservationDate })).FirstOrDefault();

            if (existingReservation != null)
            {
                // 如果存在，更新場館信息
                const string updateSql = @"UPDATE VenueReservation SET GymId = @GymId WHERE VenueReservationID = @VenueReservationID";
                await _db.ExecuteAsync(updateSql, new { model.GymId, existingReservation.VenueReservationID });
                return Ok(new { Message = "Reservation updated." });
            }
            else
            {
                // 如果不存在，創建新的預定
                const string insertSql = @"INSERT INTO VenueReservation (TrainerID, GymId, VenueReservationDate) VALUES (@TrainerId, @GymId, @VenueReservationDate)";
                await _db.ExecuteAsync(insertSql, model);
                return Ok(new { Message = "New reservation created." });
            }

        }
        [HttpGet("VenueReservation/{id}")]
        public async Task<IActionResult> GetAllVenueReservation(int id)
        {
                const string sql = @"SELECT 
                v.GymID,
                v.TrainerID,
                v.VenueReservationDate,
                v.VenueReservationID,
                g.GymName,
                t.TrainerName
                ,g.Address

                FROM VenueReservation v
                INNER JOIN Gyms g ON v.GymID = g.GymID
                INNER JOIN Trainers t ON v.TrainerID = t.TrainerID
                WHERE v.TrainerID = @TrainerId ;";
            var parameters = new { TrainerId = id };

            var VenueReservations = await _db.QueryAsync<VenueReservationDTO>(sql, parameters);
            // 基本驗證，確保資料存在
            if (VenueReservations == null || !VenueReservations.Any())
            {
                return NotFound("No data found");
            }
            return Ok(VenueReservations.ToList());
        }

        [HttpDelete("VenueReservation/{venueReservationId}")]
        public async Task<IActionResult> DeleteVenueReservation(int venueReservationId)
        {
            if (venueReservationId <= 0)
            {
                return BadRequest("Invalid id.");
            }

            const string checkExistenceSql = @"SELECT * FROM VenueReservation WHERE VenueReservationID = @VenueReservationID";
            var existingReservation = (await _db.QueryAsync<VenueReservation>(checkExistenceSql, new { VenueReservationID = venueReservationId })).FirstOrDefault();

            if (existingReservation != null)
            {
                const string deleteSql = @"DELETE FROM VenueReservation WHERE VenueReservationID = @VenueReservationID";
                await _db.ExecuteAsync(deleteSql, new { VenueReservationID = existingReservation.VenueReservationID });
                return Ok(new { Message = "Reservation deleted." });
            }
            else
            {
                return NotFound("Reservation not found.");
            }
        }
        [HttpPost("Class")]
        public async Task<IActionResult> InsertClass(Class model)
        {
            if (model == null)
            {
                return BadRequest("Invalid data.");
            }

            // 查找特定時間段是否存在課程
            const string checkExistenceSql = @"
SELECT a.ClassID, a.CourseStatus, a.CourseUnitPrice, a.StartTime, a.BuildTime, a.EndTime, a.VenueReservationID,
       b.GymID, b.GymName, b.Address, b.OpentimeStart, b.OpentimeEnd,
       c.MemberID, c.MemberName, 
       e.TrainerName, e.TrainerID, e.CourseFee
FROM Class AS a
INNER JOIN Gyms AS b ON a.GymID = b.GymID
INNER JOIN Member AS c ON a.MemberID = c.MemberID
INNER JOIN Trainers AS e ON a.TrainerID = e.TrainerID
WHERE a.TrainerID = @TrainerId AND a.StartTime >= @StartTime AND a.EndTime <= @EndTime";

            var existingClass = (await _db.QueryAsync<TrainerClassDTO>(checkExistenceSql,
                                    new { model.TrainerId, StartTime = model.StartTime, EndTime = model.EndTime }))
                                    .FirstOrDefault();

            if (existingClass != null)
            {
                return BadRequest("A class with the same TrainerId and time slot already exists.");
            }

            // 如果CourseStatus为空，设置为'進行中'
            if (string.IsNullOrEmpty(model.CourseStatus))
            {
                model.CourseStatus = "進行中";
            }

            // 查询教练的CourseFee
            const string trainerCourseFeeSql = "SELECT CourseFee FROM Trainers WHERE TrainerID = @TrainerId";
            var courseFee = await _db.QueryFirstOrDefaultAsync<decimal>(trainerCourseFeeSql, new { model.TrainerId });

            // 获取当前时间作为BuildTime
            var buildTime = DateTime.Now;

            // 插入新的課程
            const string insertSql = @"
                    INSERT INTO Class (TrainerId, StartTime, EndTime, GymId , CourseStatus, CourseUnitPrice, VenueReservationID, BuildTime)
                    VALUES (@TrainerId, @StartTime, @EndTime, @GymId, @CourseStatus, @CourseFee, @VenueReservationID, @BuildTime);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

            var classId = await _db.ExecuteScalarAsync<int>(insertSql,
                    new { model.TrainerId, StartTime = model.StartTime, EndTime = model.EndTime, model.GymId, MemberID = model.MemberId, CourseStatus = model.CourseStatus, CourseFee = courseFee, model.VenueReservationID, BuildTime = buildTime });

            return Ok(new { ClassId = classId, Message = "New class created." });
        }

        [HttpDelete("Class/{classId}")]
        public async Task<IActionResult> DeleteClass(int classId)
        {
            // 检查ClassID是否存在
            const string checkExistenceSql = @"SELECT ClassID FROM Class WHERE ClassID = @ClassID";
            var existingClassId = await _db.QueryFirstOrDefaultAsync<int>(checkExistenceSql, new { ClassID = classId });

            if (existingClassId == 0)
            {
                // 如果ClassID不存在，则返回一个错误消息
                return NotFound(new { Message = "Class not found." });
            }

            // 删除课程
            const string deleteSql = @"DELETE FROM Class WHERE ClassID = @ClassID";
            await _db.ExecuteAsync(deleteSql, new { ClassID = classId });

            // 返回成功消息
            return Ok(new { Message = "Class deleted." });
        }




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
