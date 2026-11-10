namespace Naphat_6507_L005.ViewModels;

// ข้อมูลจากฟอร์มคืนเงินประกัน (Accounting/RefundDeposit POST)
public class RefundFormViewModel
{
    public string BookingId { get; set; }
    public decimal Deduction { get; set; }          // หักค่าเสียหาย (0 = คืนเต็ม)
    public string? DeductionReason { get; set; }
    public string Method { get; set; }
}
