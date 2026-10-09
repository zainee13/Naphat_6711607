using System;
using System.Collections.Generic;

namespace Naphat_6507_L005.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public string BookingId { get; set; } = null!;

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public DateTime? PaymentDate { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public string? SlipImageUrl { get; set; }

    public DateTime? DueDate { get; set; }

    public int? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual User? VerifiedByNavigation { get; set; }
}
