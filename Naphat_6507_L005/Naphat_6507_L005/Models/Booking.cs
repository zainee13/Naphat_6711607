using System;
using System.Collections.Generic;

namespace Naphat_6507_L005.Models;

public partial class Booking
{
    public string BookingId { get; set; } = null!;

    public int UserId { get; set; }

    public string RoomId { get; set; } = null!;

    public DateTime BookingDate { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal TotalRent { get; set; }

    public decimal Deposit { get; set; }

    public string? PromoCode { get; set; }

    public decimal? DiscountAmount { get; set; }

    public string? GoodsCategory { get; set; }

    public string? Note { get; set; }

    public string BookingStatus { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Room Room { get; set; } = null!;

    public virtual ICollection<Stockitem> Stockitems { get; set; } = new List<Stockitem>();

    public virtual ICollection<Supportticket> Supporttickets { get; set; } = new List<Supportticket>();

    public virtual User User { get; set; } = null!;
}
