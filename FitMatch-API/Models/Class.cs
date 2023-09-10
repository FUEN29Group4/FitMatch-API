using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models;
public partial class Class
{
    public int ClassId { get; set; }

    public int? ClassTypeId { get; set; }

    public int? TrainerId { get; set; }

    public int? GymId { get; set; }

    public int MemberId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }
    
    public DateTime? BuildTime { get; set; }

    public string CourseStatus { get; set; }
    //public List<Member> Members { get; set; }

    //public List<Gym> Gyms { get; set; }
    //public List<ClassType> ClassTypes { get; set; }
    //public List<Trainer> Trainers { get; set; }


    //public Class()
    //{
    //    Trainers=new List<Trainer>();
    //    Members = new List<Member>();
    //    Gyms = new List<Gym>();
    //    ClassTypes = new List<ClassType>();
    //}


}
