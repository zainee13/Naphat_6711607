namespace Naphat_6507_L005.ViewModels;

// ประวัติการคืนเงินในหน้า Accounting/Refunds
public class RefundHistoryViewModel
{
    public string RefundId { get; set; }
    public string BookingId { get; set; }
    public string RoomId { get; set; }
    public string CustomerName { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? Method { get; set; }
    public string DateText { get; set; }
    public string StaffName { get; set; }
}
