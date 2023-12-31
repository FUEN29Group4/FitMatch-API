﻿using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using System.Net;
using System.Text;

namespace FitMatch_API.Controllers
{  
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IDbConnection _db;

        public ReservationController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {//做教練預約週曆用的
            // 參數化查詢，防止SQL注入
            string sql = @"SELECT t.TrainerID ,t.TrainerName, t.Photo, t.Certificate, t.Experience, t.Expertise, t.CourseFee, c.ClassID, c.GymID, c.MemberID, c.StartTime, c.EndTime, c.CourseStatus, g.GymID, g.GymName, g.Phone, g.Address,g.Photo FROM Trainers as t INNER JOIN Class as c ON t.TrainerID = c.TrainerID INNER JOIN Gyms as g ON c.GymID = g.GymID WHERE t.TrainerID = @Id AND t.Approved = 1";

            var lookup = new Dictionary<int, Trainer>();
            await _db.QueryAsync<Trainer, Class, Gym, Trainer>(
                sql,
                (trainer, classDetails, gymDetails) =>
                {
                    Trainer trainerEntry;

                    if (!lookup.TryGetValue(trainer.TrainerId, out trainerEntry))
                    {
                        lookup.Add(trainer.TrainerId, trainerEntry = trainer);
                        trainerEntry.Classes = new List<Class>();
                        trainerEntry.Gyms = new List<Gym>();
                    }

                    trainerEntry.Classes.Add(classDetails);
                    trainerEntry.Gyms.Add(gymDetails);
                    return trainerEntry;
                },
                new { Id = id },
                splitOn: "ClassID, GymID");

            var result = lookup.Values.FirstOrDefault();

            if (result == null)
            {
                return NotFound("查無此教練");
            }

            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> MakeReservation([FromBody] ReservationData ReservationData)
        {//接收預約資訊 更新sql
            if (ReservationData == null)
            {
                return BadRequest("找不到data");
            }

            // 更新課程記錄
            string sql = @"UPDATE Class SET MemberID = @MemberId WHERE ClassID = @ClassId";

            var parameters = new
            {
                ClassId = ReservationData.ClassId,
                MemberId = ReservationData.MemberId,
            };

            try
            {
                int rowsAffected = await _db.ExecuteAsync(sql, parameters);

                if (rowsAffected > 0)
                {
                    testmessage(ReservationData);
                    return Ok("預約成功");
                }
            }
            catch (Exception ex)
            {
                // Log exception here
                return StatusCode(500, $"錯誤訊息: {ex}");
            }

            return BadRequest("無法預約");
        }

        private void testmessage(ReservationData reservationData)
        {
            string token = "cKkJQCCyc9MWchZaeE8wr82jvH7kdyJ8RTXp1FgS4kA";
            string url = "https://notify-api.line.me/api/notify";

            string message = $"\n{reservationData.trainerName}教練收到預約\n預約編號:{reservationData.ClassId}\n會員編號:{reservationData.MemberId}";

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.DefaultConnectionLimit = 9999;
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12; // Use TLS 1.2, TLS 1.1, and TLS 1.0

                var request = (HttpWebRequest)WebRequest.Create(url);
                var postData = string.Format("message={0}", message);
                var data = Encoding.UTF8.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Headers.Add("Authorization", "Bearer " + token);

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // Use TLS 1.2
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true; // Bypass certificate validation

                using (var stream = request.GetRequestStream()) stream.Write(data, 0, data.Length);
                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        // 接收的模型
        public class ReservationData
        {
            public int ClassId { get; set; }
            public int MemberId { get; set; }
            public string trainerName { get; set; }
        }

        //從課程預約之週曆 讀取教練
        [HttpGet("MatchGym/{id}")]
        public async Task<IActionResult> GetGymId(int id)
        {
            // 參數化查詢，防止SQL注入
            string sql = @"SELECT c.*, t.TrainerId, t.TrainerName ,t.Photo
                            FROM Class as c
                            LEFT JOIN Trainers as t ON c.TrainerId = t.TrainerId
                            WHERE c.GymId = @GymId AND t.approved = 1;";

            var parameters = new { GymId = id };
            var lookup = new Dictionary<int, Class>();

            await _db.QueryAsync<Class, Trainer, Class>(sql,
                (c, t) =>
                {
                    Class classEntry;

                    if (!lookup.TryGetValue(c.ClassId, out classEntry))
                    {
                        lookup.Add(c.ClassId, classEntry = c);
                    }

                    if (classEntry.Trainers == null)
                        classEntry.Trainers = new List<Trainer>();

                    classEntry.Trainers.Add(t); // Add trainer to list
                    return classEntry;
                }, splitOn: "TrainerId", param: parameters);

            var resultList = lookup.Values;

            return Ok(resultList);
        }

    }
}
