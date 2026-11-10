namespace Naphat_6507_L005.ViewModels;

// ข้อมูลจากฟอร์มแจ้งชำระเงิน (Customer/Pay POST)
public class PaymentViewModel
{
    public string BookingId { get; set; }
    public string PayMethod { get; set; }   // transfer / qr / card
    public IFormFile? Slip { get; set; }
}
