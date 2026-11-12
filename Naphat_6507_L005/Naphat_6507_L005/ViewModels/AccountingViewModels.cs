namespace Naphat_6507_L005.ViewModels;

// ViewModels ของหน้าฝ่ายบัญชี (AccountingController) และใบเสร็จที่ลูกค้าเปิดดูได้
// 1 คลาสต่อ 1 งาน (ฟอร์มหรือสิ่งที่แสดงบนหน้า) รวมไว้ในไฟล์เดียวตามฝ่าย

// ===================== AccountingPaymentViewModel =====================
// รายการชำระเงินที่ฝ่ายบัญชีตรวจ (Accounting/Dashboard และ Accounting/VerifyPayment)
public class AccountingPaymentViewModel
{
    public int PaymentId { get; set; }
    public string No { get; set; }            // PAY-00001
    public string BookingId { get; set; }
    public string RoomId { get; set; }
    public string Zone { get; set; }
    public string Period { get; set; }
    public int Months { get; set; }
    public decimal MonthlyRate { get; set; }
    public decimal Deposit { get; set; }
    public decimal Amount { get; set; }
    public string MethodText { get; set; }
    public string PaidDate { get; set; }
    public string DueDate { get; set; }
    public string? SlipUrl { get; set; }
    public string Status { get; set; }        // Pending / Approved / Rejected
    public string CustomerName { get; set; }
    public string? Company { get; set; }
    public string? TaxId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Note { get; set; }
}

// ===================== RejectPaymentViewModel =====================
// ข้อมูลจากฟอร์มปฏิเสธการชำระเงิน (Accounting/Reject POST)
public class RejectPaymentViewModel
{
    public int PaymentId { get; set; }
    public string Reason { get; set; }
}

// ===================== InvoiceListItemViewModel =====================
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

// ===================== ReceiptDocumentViewModel =====================
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

// ===================== DailySummaryRowViewModel =====================
// แถวรายวันในหน้า Accounting/DailySummary
public class DailySummaryRowViewModel
{
    public DateTime Date { get; set; }
    public string DateText { get; set; }
    public int Count { get; set; }          // จำนวนการชำระที่อนุมัติวันนั้น
    public decimal Rent { get; set; }       // ค่าเช่า
    public decimal Deposit { get; set; }    // เงินประกัน
    public decimal Refund { get; set; }     // คืนเงิน
    public decimal Net => Rent + Deposit - Refund;
}

// ===================== RefundBookingViewModel =====================
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

// ===================== RefundFormViewModel =====================
// ข้อมูลจากฟอร์มคืนเงินประกัน (Accounting/RefundDeposit POST)
public class RefundFormViewModel
{
    public string BookingId { get; set; }
    public decimal Deduction { get; set; }          // หักค่าเสียหาย (0 = คืนเต็ม)
    public string? DeductionReason { get; set; }
    public string Method { get; set; }
}

// ===================== RefundHistoryViewModel =====================
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
