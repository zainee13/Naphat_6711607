using System;
using System.Collections.Generic;

namespace Naphat_6507_L005.Models;

public partial class Supportticket
{
    public int TicketId { get; set; }

    public int UserId { get; set; }

    public string? BookingId { get; set; }

    public string Subject { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public string TicketStatus { get; set; } = null!;

    public int? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public virtual User? AssignedToNavigation { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual User User { get; set; } = null!;
}
