using System;
using System.Collections.Generic;

namespace Naphat_6507_L005.Models;

public partial class Receipt
{
    public string ReceiptId { get; set; } = null!;

    public int PaymentId { get; set; }

    public DateTime IssuedDate { get; set; }

    public int IssuedBy { get; set; }

    public virtual User IssuedByNavigation { get; set; } = null!;

    public virtual Payment Payment { get; set; } = null!;
}
