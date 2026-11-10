namespace Naphat_6507_L005.ViewModels;

// แถวในหน้า Accounting/Invoices (การชำระที่อนุมัติแล้ว + เลขเอกสาร)
public class InvoiceListItemViewModel
{
    public int PaymentId { get; set; }
    public string PaymentNo { get; set; }
    public string BookingId { get; set; }
    public string RoomId { get; set; }
    public string CustomerName { get; set; }
    public decimal Amount { get; set; }
    public string ApprovedDate { get; set; }
    public string? InvoiceId { get; set; }   // null = ยังไม่ออกเอกสาร
    public string? ReceiptId { get; set; }
}
