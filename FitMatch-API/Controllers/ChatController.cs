using FitMatch_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;


namespace FitMatch_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IDbConnection _db;
        public ChatController(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }



        // GET: api/Chat/GetHistory
        [HttpGet("GetHistory/{senderId}/{receiverId}")]
        public async Task<IActionResult> GetChatHistory(int senderId, int receiverId)
        {
            string sql = @"SELECT * FROM CustomerService
                   WHERE (SenderId = @SenderId AND ReceiverId = @ReceiverId) 
                   OR (SenderId = @ReceiverId AND ReceiverId = @SenderId)
                   ORDER BY DateTime ASC";

            var parameters = new { SenderId = senderId, ReceiverId = receiverId };
            var result = await _db.QueryAsync<CustomerService>(sql, parameters);
            return Ok(result);
        }
        [HttpGet("Photo/{Id}/{role}")]
        public async Task<IActionResult> GetPhoto(int Id, string role)
        {
            string sql = "";

            if (role == "Member")
            {
                sql = @"SELECT Photo FROM Member WHERE MemberId = @Id";
            }
            else if (role == "Trainer")
            {
                sql = @"SELECT Photo FROM Trainers WHERE TrainerId = @Id";
            }

            var parameter = new { Id = Id };
            var photo = await _db.QuerySingleOrDefaultAsync<string>(sql, parameter);
            return Ok(new { Photo = photo });
        }
        //[HttpGet("GetNewMessages/{senderId}/{receiverId}")]
        //public async Task<IActionResult> GetNewMessages(int senderId, int receiverId)
        //{
        //    // 查询新消息的 SQL 查询语句，你需要根据你的数据库架构来自定义
        //    string sql = @"
        //SELECT * FROM CustomerService
        //WHERE SenderId = @ReceiverId AND ReceiverId = @SenderId AND IsRead = 0
        //ORDER BY DateTime ASC";

        //    var parameters = new { SenderId = senderId, ReceiverId = receiverId };
        //    var newMessages = await _db.QueryAsync<CustomerService>(sql, parameters);

        //    // 将新消息标记为已读
        //    sql = @"
        //UPDATE CustomerService
        //SET IsRead = 1
        //WHERE SenderId = @ReceiverId AND ReceiverId = @SenderId AND IsRead = 0";

        //    await _db.ExecuteAsync(sql, parameters);

        //    return Ok(newMessages);
        //}

        //[HttpGet("SessionInfo")]
        //public IActionResult GetSessionInfo()
        //{
        //    int userId = HttpContext.Session.GetInt32("UserId") ?? 0; // 假設您從 Session 獲取
        //    string userType = HttpContext.Session.GetString("UserType"); // 假設您從 Session 獲取

        //    if (userId != 0 && !string.IsNullOrEmpty(userType))
        //    {
        //        return Ok(new { UserId = userId, UserType = userType });
        //    }
        //    else
        //    {
        //        return Unauthorized();
        //    }
        //}
    }
}


//////////////////
///using Dapper;
//using Microsoft.AspNetCore.SignalR;
//using System.Data.SqlClient;
//using System.Data;
//using FitMatch_API.Models;
//using Microsoft.AspNetCore.Authorization;

//namespace FitMatch_API.Hubs
//{
//    public class ChatHub : Hub
//    {
//        private static readonly Dictionary<string, string> ConnectionMapping = new Dictionary<string, string>();

//        private readonly IDbConnection _db;
//        public ChatHub(IConfiguration configuration)
//        {
//            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
//        }


//        //public override async Task OnConnectedAsync()
//        //{
//        //    var httpContext = Context.GetHttpContext();
//        //    var sessionId = httpContext.Request.Headers["Session-Id"].ToString();
//        //    // 保存 ConnectionId 和 sessionId 的关联
//        //    ConnectionMapping[sessionId] = Context.ConnectionId;

//        //    await base.OnConnectedAsync();
//        //}
//        //[Authorize]
//        public async Task SendMessage(string receiverId, string message, string senderId, string role)
//        {
//            try
//            {
//                string receiverRole = role == "Member" ? "Trainer" : "Member";

//                // 初始化 CustomerService 實例
//                CustomerService customerService = new CustomerService();
//                customerService.DateTime = DateTime.Now;
//                customerService.MessageContent = message;
//                customerService.SenderId = int.Parse(senderId);
//                customerService.ReceiverId = int.Parse(receiverId);
//                customerService.Role = receiverRole; // 使用计算得到的接收者角色

//                // 儲存到資料庫
//                string sql = @"INSERT INTO CustomerService (SenderId, ReceiverId, MessageContent, DateTime, Role) 
//                VALUES (@SenderId, @ReceiverId, @MessageContent, @DateTime, @Role)";
//                await _db.ExecuteAsync(sql, customerService);

//                // 如果有 receiver 的 ConnectionId，則發送消息
//                if (ConnectionMapping.TryGetValue(receiverId, out string receiverConnectionId))
//                {
//                    await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", senderId, message);
//                }

//            }
//            catch (Exception ex)
//            {
//                // 輸出錯誤信息
//                Console.WriteLine($"An error occurred in SendMessage: {ex.Message}");
//                throw; // 重新拋出錯誤以通知客戶端
//            }
//        }


//    }
//}
