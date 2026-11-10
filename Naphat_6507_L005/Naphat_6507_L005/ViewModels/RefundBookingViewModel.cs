namespace Naphat_6507_L005.ViewModels;

// การจองที่คืนเงินประกันได้ (Accounting/Refunds และหน้า Accounting/RefundDeposit)
public class RefundBookingViewModel
{
    public string BookingId { get; set; }
    public string RoomId { get; set; }
    public string Zone { get; set; }
    public string CustomerName { get; set; }
    public string Period { get; set; }
    public string EndDateText { get; set; }
    public bool IsEnded { get; set; }       // ครบสัญญาแล้ว (เลยวันสิ้นสุด)
    public decimal Deposit { get; set; }
}
