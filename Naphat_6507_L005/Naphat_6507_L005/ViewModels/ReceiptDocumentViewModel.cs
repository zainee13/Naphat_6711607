namespace Naphat_6507_L005.ViewModels;

// ใบแจ้งหนี้ / ใบเสร็จรับเงิน สำหรับพิมพ์ (Views/Shared/ReceiptDocument.cshtml)
// ใช้ทั้งฝ่ายบัญชี (Accounting/Document) และลูกค้า (Customer/Receipt)
public class ReceiptDocumentViewModel
{
    public string ReceiptId { get; set; }
    public string ReceiptDate { get; set; }
    public string InvoiceId { get; set; }
    public string InvoiceDate { get; set; }
    public string IssuedByName { get; set; }

    public string PaymentNo { get; set; }
    public string PaidDate { get; set; }
    public string MethodText { get; set; }

    public string BookingId { get; set; }
    public string RoomId { get; set; }
    public string Zone { get; set; }
    public string Period { get; set; }
    public int Months { get; set; }

    public decimal RentAmount { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }

    public string CustomerName { get; set; }
    public string? Company { get; set; }
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // ปุ่มย้อนกลับ (ฝ่ายบัญชีกลับหน้ารายการ / ลูกค้ากลับหน้าบัญชีของฉัน)
    public string BackUrl { get; set; }
}
