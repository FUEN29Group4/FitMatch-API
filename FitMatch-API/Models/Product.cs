using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models
{

    public partial class Product
    {

        public int ProductId { get; set; }//商品編號

        public string? ProductName { get; set; }//商品名稱

        public string? ProductDescription { get; set; }//商品描述


        public int? Price { get; set; }//價格

        public int? TypeId { get; set; }//商品類別編號

        public int? ProductInventory { get; set; }//商品庫存

        public bool? Approved { get; set; }//審核通過與否

        public bool? Status { get; set; }

        public string? Photo { get; set; }//照片


        //public List<Product> Products { get; set; } 

        public int OrderDetailId { get; set; }//訂單明細編號

        public int OrderId { get; set; }//訂單編號

        public int? Quantity { get; set; }//數量


        public int? MemberId { get; set; }//會員編號


        public List<Member> Members { get; set; } = new List<Member>();
        public List<Order> Orders { get; set; } = new List<Order>();

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    }
}