using System;
using System.Collections.Generic;

namespace Naphat_6507_L005.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public string BookingId { get; set; } = null!;

    public int OverallRating { get; set; }

    public string? Comment { get; set; }

    public DateTime ReviewDate { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
