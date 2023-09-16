using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models;
public partial class CustomerService
{
    public int CustomerServiceId { get; set; }

    public int? SenderId { get; set; }

    public string? MessageContent { get; set; }

    public DateTime? DateTime { get; set; }

    public int? ReceiverId { get; set; }

    public string? Role { get; set; }//Trainer or Member
}
