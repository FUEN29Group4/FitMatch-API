using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models;
public partial class ClassType
{
    public int ClassTypeId { get; set; }

    public string? ClassName { get; set; }

    public string? Photo { get; set; }

    public string? Introduction { get; set; }

    public int? Status { get; set; }

}
