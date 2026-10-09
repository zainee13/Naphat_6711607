using System;
using System.Collections.Generic;

namespace Naphat_6507_L005.Models;

public partial class Invoice
{
    public string InvoiceId { get; set; } = null!;

    public string BookingId { get; set; } = null!;

    public decimal RentAmount { get; set; }

    public decimal DepositAmount { get; set; }

    public decimal VatAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime IssuedDate { get; set; }

    public int IssuedBy { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual User IssuedByNavigation { get; set; } = null!;
}
