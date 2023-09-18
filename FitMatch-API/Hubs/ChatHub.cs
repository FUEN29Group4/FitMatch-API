using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using FitMatch_API.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FitMatch_API.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly Dictionary<string, string> ConnectionMapping = new Dictionary<string, string>();

        private readonly IDbConnection _db;
        private readonly ILogger<ChatHub> _logger;  // Logger

        public ChatHub(IConfiguration configuration, ILogger<ChatHub> logger)
        {
            _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            _logger = logger;

        }
        public async Task InitializeClient(int senderId)
        {
            ConnectionMapping[senderId.ToString()] = Context.ConnectionId;
            await Clients.Client(Context.ConnectionId).SendAsync("Initialized", true);
        }


        //public override async task onconnectedasync()
        //{
        //    var httpcontext = context.gethttpcontext();
        //    var sessionid = httpcontext.request.headers["session-id"].tostring();
        //    // 保存 connectionid 和 sessionid 的关联
        //    connectionmapping[sessionid] = context.connectionid;

        //    await base.onconnectedasync();
        //}

        public void SendMessage(int receiverId, string message, string senderId, string role)
        {
            try
            {

                CustomerService customerService = new CustomerService
                {
                    DateTime = DateTime.Now,
                    MessageContent = message,
                    SenderId = int.Parse(senderId),
                    ReceiverId = receiverId,
                    Role = role
                };


                // 儲存到資料庫
                string sql = @"INSERT INTO CustomerService (SenderId, ReceiverId, MessageContent, DateTime, Role) 
VALUES (@SenderId, @ReceiverId, @MessageContent, @DateTime,@Role)";
                _db.ExecuteAsync(sql, customerService);

                // Send the message if receiver's ConnectionId exists
                // 发送消息给接收者
                if (ConnectionMapping.TryGetValue(receiverId.ToString(), out string receiverConnectionId))
                {
                    Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", senderId, message, role);
                }
                else
                {
                    _logger.LogWarning($"No client associated with receiverId: {receiverId}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception: {e.Message}, StackTrace: {e.StackTrace}");
            }

        }
        public override async Task OnConnectedAsync()
        {
            var senderId = Context.GetHttpContext().Request.Headers["senderId"].ToString();
            ConnectionMapping[senderId] = Context.ConnectionId;
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var senderId = Context.GetHttpContext().Request.Headers["senderId"].ToString();
            ConnectionMapping.Remove(senderId);
            await base.OnDisconnectedAsync(exception);
        }


    }
}
