using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models;
public partial class Member
{
    public int MemberId { get; set; }

    public string? MemberName { get; set; }



    public bool? Gender { get; set; }

    public DateTime? Birth { get; set; }


    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public string? Photo { get; set; }

    public string? Password { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? Status { get; set; }



}