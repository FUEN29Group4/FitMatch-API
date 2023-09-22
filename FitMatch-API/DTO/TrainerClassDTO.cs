namespace FitMatch_API.DTO
{
    public class TrainerClassDTO
    {
        // Class 屬性
        public int ClassId { get; set; }
        public int VenueReservationId {get; set; }
        public DateTime? VenueReservationDate {get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? BuildTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? OpentimeStart { get; set; }
        public DateTime? OpentimeEnd { get; set; }
        public string CourseStatus {get; set;}

        public int GymId {get; set;}
        public int MemberId {get; set;}

        public int ClassTypeId {get; set;}

        public int TrainerID { get; set;}
        public string TrainerName { get; set;}
        public string Address {get; set;}
        // 額外的屬性
        public string GymName { get; set; }
        public string MemberName { get; set; }
        public int CourseUnitPrice { get; set;}
        //public DateTimeOffset StartTimeOffset { get; set; }
        //public DateTimeOffset BuildTimeOffset { get; set; }
        //public DateTimeOffset EndTimeOffset { get; set; }
        //public DateTimeOffset OpentimeStartOffset { get; set; }
        //public DateTimeOffset OpentimeEndOffset { get; set; }
        //public DateTimeOffset VenueReservationDateOffset { get; set; }
    }
}
