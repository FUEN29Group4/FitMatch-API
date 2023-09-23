namespace FitMatch_API.DTO
{
    public class VenueReservationDTO
    {
        public int VenueReservationId { get; set; }
        public int GymId { get; set; }
        public int TrainerId { get; set; }
        public DateTime VenueReservationDate { get; set; }
        public string? Address { get; set; }

        public string? GymName { get; set; }
        public string? TrainerName { get; set; } // 将MemberName更改为TrainerName

    }
}
