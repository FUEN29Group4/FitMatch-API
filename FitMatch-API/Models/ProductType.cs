using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models { 

public partial class ProductType
{
    
    public int TypeId { get; set; }

    public string? TypeName { get; set; }

}
}