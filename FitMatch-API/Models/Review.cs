using System;
using System.Collections.Generic;

namespace FitMatch_API.Models
{
    public partial class Review
    {
        public int ReviewId { get; set; }
        public int? MemberId { get; set; }
        public int? ProductId { get; set; }
        public int? ClassId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? ReviewDateTime { get; set; }
        public string? MemberName { get; set; }
        public string? ProductName { get; set; }
        public string CourseStatus { get; set; }
        public int TrainerId { get; set; }
        public string? TrainerName { get; set; }
        public int ClassTypeId { get; set; }
        public string? ClassName { get; set; }
        public List<Trainer> Trainers { get; set; } = new List<Trainer>();
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Member> Members { get; set; } = new List<Member>();
        public List<Class> Classs { get; set; } = new List<Class>();
        public List<ClassType> ClassTypes { get; set; } = new List<ClassType>();
    }
}
