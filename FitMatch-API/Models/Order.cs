using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models;

public partial class Order
{
    public int OrderId { get; set; }//訂單編號

    public int? MemberId { get; set; }//會員編號

    public int? TotalPrice { get; set; }//訂單總價

    public DateTime? OrderTime { get; set; }//訂單時間

    public string? PaymentMethod { get; set; }//支付方式

    public string? ShippingMethod { get; set; }//運送方式


    public DateTime? PayTime { get; set; }//付款時間
}
