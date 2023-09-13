using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;


namespace FitMatch_API.Models
{
    public class MemberClassAPI
    {

        public int MemberClassAPIId { get; set; }
        public int MemberId { get; set; }

        public int? TrainerId { get; set; }

        public int? ClassTypeId { get; set; }

        public int? GymId { get; set; }


        public int ClassId { get; set; }


        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public DateTime? BuildTime { get; set; }

        public string CourseStatus { get; set; }

        public string? ClassName { get; set; }

        public string? Photo { get; set; }

        public string? Introduction { get; set; }

        public int? Status { get; set; }




        public string? GymName { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public DateTime? OpentimeStart { get; set; }

        public DateTime? OpentimeEnd { get; set; }

        public bool? Approved { get; set; }

        public string? GymDescription { get; set; }




        public List<Class> Classs { get; set; } = new List<Class>();

        public List<Member> Members { get; set; } = new List<Member>();

        public List<Order> Orders { get; set; }  = new List<Order>();

        public List<Trainer> Trainers { get; set; } = new List<Trainer>();

        public List<ClassType> ClassTypes { get; set; } = new List<ClassType>();

        public List<Gym> Gyms { get; set; } = new List<Gym>();

        //public MemberClassAPI()
        //{
        //    Trainers = new List<Trainer>();
        //    Members = new List<Member>();
        //    Gyms = new List<Gym>();
        //    ClassTypes = new List<ClassType>();
        //    Orders = new List<Order>();
        //}

    }
}
