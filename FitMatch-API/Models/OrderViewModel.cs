using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models;

public partial class OrderViewModel
{

    //關於Order
    public int OrderId { get; set; }//訂單編號

    public int? MemberId { get; set; }//會員編號

    public decimal? TotalPrice { get; set; }//訂單總價

    public DateTime? OrderTime { get; set; }//訂單時間

    public string? PaymentMethod { get; set; }//支付方式

    public string? ShippingMethod { get; set; }//運送方式


    public DateTime? PayTime { get; set; }//付款時間




    //關於Orderdetils
    public int OrderDetailId { get; set; }//訂單明細編號


    public int? ProductId { get; set; }//產品編號

    public int? Quantity { get; set; }//數量

  


    //關於Product

    public string? ProductName { get; set; }//商品名稱

    public string? ProductDescription { get; set; }//商品描述


    public int? Price { get; set; }//價格

    public int? TypeId { get; set; }//商品類別編號

    public int? ProductInventory { get; set; }//商品庫存

    public bool? Approved { get; set; }//審核通過與否

    public bool? Status { get; set; }

    public string? Photo { get; set; }//照片



    //關於ProductType

    public string? TypeName { get; set; }






    //關於Member

    public string? MemberName { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

   

  













}
