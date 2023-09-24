using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models;
public partial class Class
{
    public int ClassId { get; set; }

    //public int? ClassTypeId { get; set; }

    public int? TrainerId { get; set; }

    public int? GymId { get; set; }

    public int? MemberId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }
    
    public DateTime? BuildTime { get; set; }

    public string? CourseStatus { get; set; }
    public int? CourseUnitPrice {get; set; }
    //public List<Member> Members { get; set; }

    //public List<Gym> Gyms { get; set; }
    //public List<ClassType> ClassTypes { get; set; }
    //public List<Trainer> Trainers { get; set; }

    public List<Trainer> Trainers { get; set; } = new List<Trainer>();

    public List<Product> Products { get; set; } = new List<Product>();

    public List<Member> Members { get; set; }
     = new List<Member>();

    public List<Order> Orders { get; set; }
     = new List<Order>();


    public string? MemberName { get; set; }
    public string? Email { get; set; }

    public int MemberClassAPIId { get; set; }




    public string? ClassName { get; set; }

    public string? Photo { get; set; }

    public string? Introduction { get; set; }

    public int? Status { get; set; }
    public int VenueReservationID { get; set; }




    public string? GymName { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public DateTime? OpentimeStart { get; set; }

    public DateTime? OpentimeEnd { get; set; }

    public bool? Approved { get; set; }

    public string? GymDescription { get; set; }




    public List<Class> Classs { get; set; } = new List<Class>();



    public List<ClassType> ClassTypes { get; set; } = new List<ClassType>();

    public List<Gym> Gyms { get; set; } = new List<Gym>();
    public int? TotalPrice { get; set; }//訂單總價
    public Class()
    {
        Trainers = new List<Trainer>();
        Members = new List<Member>();
        Gyms = new List<Gym>();
        ClassTypes = new List<ClassType>();
    }


}
