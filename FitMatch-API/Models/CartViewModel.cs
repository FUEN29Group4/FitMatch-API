using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models;

public partial class CartViewModel
{

    //關於訂單 oreder
    public int OrderId { get; set; }//訂單編號
    public int? MemberId { get; set; }//會員編號
    public int? TotalPrice { get; set; }//訂單總價
    public DateTime? OrderTime { get; set; }//訂單時間
    public string? PaymentMethod { get; set; }//支付方式
    public string? ShippingMethod { get; set; }//運送方式
    public DateTime? PayTime { get; set; }//付款時間
    //public int? Totalpirce { get; set; } 照理說不需要在table 中，之後要刪除此欄位


    //關於訂單明細 orderdetails
    public int OrderDetailId { get; set; }//訂單明細編號
    //public int OrderId { get; set; }//訂單編號
    public int? ProductId { get; set; }//產品編號
    public int? Quantity { get; set; }//數量


    //關於產品
    //public int ProductId { get; set; }//商品編號
    public string? ProductName { get; set; }//商品名稱
    public string? ProductDescription { get; set; }//商品描述
    public int? Price { get; set; }//價格
    public int? TypeId { get; set; }//商品類別編號
    public int? ProductInventory { get; set; }//商品庫存
    public bool? Approved { get; set; }//審核通過與否
    public bool? Status { get; set; }
    public string? Photo { get; set; }//照片
    

    //小計：計算每件商品總數的價格
    public decimal? 小計 { get { return this.Quantity * this.Price; }}



    
    
}
