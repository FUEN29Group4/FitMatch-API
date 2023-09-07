using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models;
public partial class MemberFavorite
{
    public int MemberFavoriteId { get; set; }

    public int? MemberId { get; set; }

    public int? ClassId { get; set; }

    public int? TrainerId { get; set; }

    public int? ProductId { get; set; }


}
