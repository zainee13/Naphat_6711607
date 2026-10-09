using System;
using System.Collections.Generic;

namespace Naphat_6507_L005.Models;

public partial class Room
{
    public string RoomId { get; set; } = null!;

    public string SizeCat { get; set; } = null!;

    public string? Description { get; set; }

    public int? Size { get; set; }

    public string? Height { get; set; }

    public string? Zone { get; set; }

    public decimal MonthlyRate { get; set; }

    public string Status { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
