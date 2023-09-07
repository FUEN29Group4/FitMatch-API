using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models;

public partial class Gym
{
    public int GymId { get; set; }

    public string? GymName { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public DateTime? OpentimeStart { get; set; }

    public DateTime? OpentimeEnd { get; set; }

    public bool? Approved { get; set; }

    public string? GymDescription { get; set; }

    public string? Photo { get; set; }

 
}
