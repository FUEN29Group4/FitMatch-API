using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models;

public partial class Cart
{
    public int CartId { get; set; }//購物車編號

    public int? MemberID{ get; set; }//會員編號

    public DateTime? CreatedDate { get; set; }//購物車時間
    
}
