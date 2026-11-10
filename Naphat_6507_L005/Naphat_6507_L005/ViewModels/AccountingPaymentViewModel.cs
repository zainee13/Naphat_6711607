namespace Naphat_6507_L005.ViewModels;

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
