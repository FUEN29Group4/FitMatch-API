using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using FitMatch_API.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
        //public async Task InitializeClient(int senderId)
        //{
        //    // 刪除舊連線
        //    if (ConnectionMapping.ContainsKey(senderId.ToString()))
        //    {
        //        ConnectionMapping.Remove(senderId.ToString());
        //    }
        //    // 添加新的連線
        //    ConnectionMapping[senderId.ToString()] = Context.ConnectionId;
        //    await Clients.Client(Context.ConnectionId).SendAsync("Initialized", true);
        //}

        

        //public override async task onconnectedasync()
        //{
        //    //var httpcontext = Context.gethttpcontext();
        //    var sessionid = httpcontext.request.headers["session-id"].tostring();
        //    // 保存 connectionid 和 sessionid 的关联
        //    ConnectionMapping[sessionid] = Context.ConnectionId;

        //    await base.onconnectedasync();
        //}

        public async Task SendMessage(int receiverId, string message, string senderId, string role)
        {

            var formattedMessage = $"{message}";
            await Clients.All.SendAsync("ReceiveMessage", senderId, formattedMessage, role);


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

        }
        public static List<string> ConnIDList = new List<string>();
        public override async Task OnConnectedAsync()
        {
            if (ConnIDList.Where(p => p == Context.ConnectionId).FirstOrDefault() == null)
            {
                ConnIDList.Add(Context.ConnectionId);
            }
            // 更新連線 ID 列表
            string jsonString = JsonConvert.SerializeObject(ConnIDList);
            //await Clients.All.SendAsync("UpdList", jsonString);

            //// 更新個人 ID
            //await Clients.Client(Context.ConnectionId).SendAsync("UpdSelfID", Context.ConnectionId);

            //// 更新聊天內容
            //await Clients.All.SendAsync("UpdContent", "新連線 ID: " + Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string id = ConnIDList.Where(p => p == Context.ConnectionId).FirstOrDefault();
            if (id != null)
            {
                ConnIDList.Remove(id);
            }
            // 更新連線 ID 列表
            string jsonString = JsonConvert.SerializeObject(ConnIDList);
            //await Clients.All.SendAsync("UpdList", jsonString);

            // 更新聊天內容
            //await Clients.All.SendAsync("UpdContent", "已離線 ID: " + Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }


    }
}
