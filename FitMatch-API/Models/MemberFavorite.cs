using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models;
public partial class MemberFavorite
{
    public int MemberFavoriteId { get; set; }

    public int? MemberId { get; set; }

    public int? TrainerId { get; set; }

    public int? ProductId { get; set; }

    public string? Introduce { get; set; }

    public List<Trainer> Trainers { get; set; }

    public List<Product> Products { get; set; }

    public List<Member> Members { get; set; }

    public List<Order> Orders { get; set; }


    //public int TrainerId { get; set; }
    public string? TrainerName { get; set; }
    //public string? Photo { get; set; }



    //public int ProductId { get; set; }//商品編號

    public string? ProductName { get; set; }//商品名稱
    public string? ProductDescription { get; set; }//商品描述



    public string? MemberName { get; set; }
    public string? Email { get; set; }


    public int? TotalPrice { get; set; }//訂單總價

    public MemberFavorite()
    {
        Trainers = new List<Trainer>();

        Products = new List<Product>();
    }
}
