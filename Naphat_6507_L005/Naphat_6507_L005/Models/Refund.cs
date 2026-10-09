using System;
using System.Collections.Generic;

namespace Naphat_6507_L005.Models;

public partial class Refund
{
    public string RefundId { get; set; } = null!;

    public int PaymentId { get; set; }

    public decimal RefundAmount { get; set; }

    public string? RefundReason { get; set; }

    public string? RefundMethod { get; set; }

    public string RefundStatus { get; set; } = null!;

    public DateTime? RefundDate { get; set; }

    public int ProcessedBy { get; set; }

    public virtual Payment Payment { get; set; } = null!;

    public virtual User ProcessedByNavigation { get; set; } = null!;
}
