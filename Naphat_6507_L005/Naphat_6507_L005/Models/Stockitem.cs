using System;
using System.Collections.Generic;

namespace Naphat_6507_L005.Models;

public partial class Stockitem
{
    public int StockId { get; set; }

    public string BookingId { get; set; } = null!;

    public string ItemName { get; set; } = null!;

    public string? Category { get; set; }

    public int Quantity { get; set; }

    public int MinQuantity { get; set; }

    public string? Unit { get; set; }

    public DateTime LastUpdated { get; set; }

    public int UpdatedBy { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual User UpdatedByNavigation { get; set; } = null!;
}
