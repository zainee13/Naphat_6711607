namespace Naphat_6507_L005.ViewModels;

// รายการจองที่แสดงในหน้า Profile
public class MyBookingViewModel
{
    public string Id { get; set; }
    public string Room { get; set; }
    public string Zone { get; set; }
    public string Period { get; set; }
    public int Months { get; set; }
    public int Rate { get; set; }
    public string Status { get; set; }     // pending / checking / active / ended / cancelled (ตัวเล็ก ใช้กับ CSS)
    public DateTime BookedDate { get; set; }
    public string BookedOn { get; set; }
}
