namespace Naphat_6507_L005.ViewModels;

// ข้อมูลจากฟอร์มจองห้อง (Customer/BookRoom POST)
public class BookingViewModel
{
    public string RoomId { get; set; }
    public string TenantType { get; set; } = "corp";   // corp = บริษัท, person = บุคคลธรรมดา
    public string CompanyName { get; set; }
    public string TaxId { get; set; }
    public string ContactName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today;
    public int Months { get; set; } = 6;
    public string GoodsType { get; set; }
    public string MoveInService { get; set; }
    public string Note { get; set; }
    public string PromoCode { get; set; }
    public bool AgreeTerms { get; set; }
}
