using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models { 

public partial class Restaurant
{
    public int RestaurantsId { get; set; }

    public string? RestaurantsName { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Photo { get; set; }

    public string? RestaurantsDescription { get; set; }

    public DateTime? CreateAt { get; set; }

    public int? Status { get; set; }
}
}