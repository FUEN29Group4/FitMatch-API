using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel;
using System.Security.Claims;

namespace FitMatch_API.Models
{
    public class Trainer
    {
        public int TrainerId { get; set; }
        public string? TrainerName { get; set; }
        public bool? Gender { get; set; }
        public DateTime? Birth { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Photo { get; set; }
        public string? Certificate { get; set; }
        public string? Expertise { get; set; }
        public string? Experience { get; set; }
        public int? CourseFee { get; set; }
        public string? Password { get; set; }
        public int? Approved { get; set; }
        //public CApprovalStatus? Approved { get; set; }    //判斷審核通過與否
        public string Salt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Introduce { get; set; }
        //public Class ClassDetails { get; set; }
        //public Gym GymDetails { get; set; }
        public List<Class> Classes { get; set; } 
        public List<Gym> Gyms { get; set; }
        public List<ClassType> ClassTypes { get; set; }

        public Trainer()
        {
            Classes = new List<Class>();
            Gyms = new List<Gym>();
            ClassTypes = new List<ClassType>();
        }
    }
}
