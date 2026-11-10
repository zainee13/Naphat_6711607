namespace Naphat_6507_L005.ViewModels;

// ข้อมูลจากฟอร์มปฏิเสธการชำระเงิน (Accounting/Reject POST)
public class RejectPaymentViewModel
{
    public int PaymentId { get; set; }
    public string Reason { get; set; }
}
