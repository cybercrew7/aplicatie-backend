namespace IPFront.Components.Booking
{
    /// <summary>Form state for the booking details form.</summary>
    public class BookingFormModel
    {
        /// <summary>Day of the reservation.</summary>
        public DateTime Date { get; set; } = DateTime.Today;
        /// <summary>Start time on <see cref="Date"/>.</summary>
        public TimeSpan StartTime { get; set; } = new(9, 0, 0);
        /// <summary>End time on <see cref="Date"/>.</summary>
        public TimeSpan EndTime { get; set; } = new(10, 0, 0);
        /// <summary>Reason for the reservation.</summary>
        public string Purpose { get; set; } = string.Empty;
        /// <summary>Expected number of attendees, if known.</summary>
        public int? ExpectedAttendees { get; set; }
    }
}
