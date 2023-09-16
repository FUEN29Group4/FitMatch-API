using Dapper;
using Microsoft.AspNetCore.SignalR;
using System.Data.SqlClient;
using System.Data;
using FitMatch_API.Models;

namespace FitMatch_API.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly Dictionary<string, string> ConnectionMapping = new Dictionary<string, string>();

        private readonly IDbConnection _db;
        public ChatHub(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }


        //public override async Task OnConnectedAsync()
        //{
        //    var httpContext = Context.GetHttpContext();
        //    var sessionId = httpContext.Request.Headers["Session-Id"].ToString();
        //    // 保存 ConnectionId 和 sessionId 的关联
        //    ConnectionMapping[sessionId] = Context.ConnectionId;

        //    await base.OnConnectedAsync();
        //}

        public void SendMessage(int receiverId, string message, string senderId, string role)
        {
            try
            {

                CustomerService customerService = new CustomerService();
                customerService.DateTime = DateTime.Now;
                customerService.MessageContent = message;
                customerService.SenderId = int.Parse(senderId);
                customerService.ReceiverId = receiverId;
                customerService.Role = role; // 使用计算得到的接收者角色


                // 儲存到資料庫
                string sql = @"INSERT INTO CustomerService (SenderId, ReceiverId, MessageContent, DateTime, Role) 
VALUES (@SenderId, @ReceiverId, @MessageContent, @DateTime,@Role)";
                _db.ExecuteAsync(sql, customerService);

                // 如果有 receiver 的 ConnectionId，則發送消息
                if (ConnectionMapping.TryGetValue(receiverId.ToString(), out string receiverConnectionId))
                {
                    Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", senderId, message);
                }

            }
            catch (Exception e)
            {
                // 輸出錯誤信息
                Console.WriteLine($"Exception: {e.Message}, StackTrace: {e.StackTrace}");
            }

        }
    }
}
