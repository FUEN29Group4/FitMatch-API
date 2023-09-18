using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models;

public partial class CartItem
{

    public int CartIdemId { get; set; }//購物車項目編號

    public int CartId { get; set; }//購物車編號

    public int? ProductId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }




}
