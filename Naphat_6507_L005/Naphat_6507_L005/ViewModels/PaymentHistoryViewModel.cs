namespace Naphat_6507_L005.ViewModels;

// ประวัติการชำระในหน้า Profile
public class PaymentHistoryViewModel
{
    public string No { get; set; }
    public string Title { get; set; }
    public int Amount { get; set; }
    public string Detail { get; set; }
    public bool IsRefund { get; set; }
    public string? ReceiptId { get; set; }   // มีใบเสร็จแล้ว ลูกค้ากดดูได้
    public DateTime Date { get; set; }       // ใช้เรียงรายการ (ชำระ + คืนเงิน)
}
