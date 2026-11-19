namespace IPFront.Components.Booking
{
    public class BookingFormModel
    {
        public DateTime Date { get; set; } = DateTime.Today;
        public TimeSpan StartTime { get; set; } = new(9, 0, 0);
        public TimeSpan EndTime { get; set; } = new(10, 0, 0);
        public string Purpose { get; set; } = string.Empty;
        public int? ExpectedAttendees { get; set; }
    }
}
