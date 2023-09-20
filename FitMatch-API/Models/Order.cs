using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models;

public partial class Order
{
    public int OrderId { get; set; }//訂單編號

    public int? MemberId { get; set; }//會員編號

    public decimal? TotalPrice { get; set; }//訂單總價

    public DateTime? OrderTime { get; set; }//訂單時間

    public string? PaymentMethod { get; set; }//支付方式

    public string? ShippingMethod { get; set; }//運送方式


    public DateTime? PayTime { get; set; }//付款時間


    public string? MemberName { get; set; }


    public int ProductId { get; set; }//商品編號

    public string? ProductName { get; set; }//商品名稱

    public int? Price { get; set; }//價格


    public List<Member> Members { get; set; }= new List<Member>();
    public List<Product> Products { get; set; } = new List<Product>();


    public bool? Status { get; set; }//狀態

   
}
