using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models
{
    public partial class OrderDetail
    {
        public int OrderDetailId { get; set; }//訂單明細編號

        public int OrderId { get; set; }//訂單編號

        public int? ProductId { get; set; }//產品編號

        public int? Quantity { get; set; }//數量

    }
}