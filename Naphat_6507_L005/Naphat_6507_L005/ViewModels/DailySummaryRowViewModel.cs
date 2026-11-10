namespace Naphat_6507_L005.ViewModels;

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
