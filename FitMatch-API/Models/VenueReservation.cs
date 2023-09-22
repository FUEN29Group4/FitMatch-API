namespace FitMatch_API.Models
{
    public class VenueReservation
    {
        public int VenueReservationID { get; set; }
        public int TrainerID { get; set; }
        public int GymId { get; set; }
        public DateTime VenueReservationDate { get; set; }
    }
}
