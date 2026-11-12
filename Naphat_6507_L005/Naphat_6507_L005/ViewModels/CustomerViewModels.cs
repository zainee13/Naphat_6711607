namespace Naphat_6507_L005.ViewModels;

// ViewModels ของหน้าฝั่งลูกค้า (CustomerController)
// 1 คลาสต่อ 1 งาน (ฟอร์มหรือสิ่งที่แสดงบนหน้า) รวมไว้ในไฟล์เดียวตามฝ่าย

// ===================== RoomViewModel =====================
// ข้อมูลห้องที่ใช้แสดงในหน้า Search / RoomDetail / BookRoom
// มาจากตาราง room + ข้อมูลเสริมที่ตารางยังไม่มีคอลัมน์ (อยู่ใน CustomerController._roomExtras)
public class RoomViewModel
{
    public string Id { get; set; }
    public string Kind { get; set; }        // s / m / l / xl / cold
    public bool Free { get; set; }
    public string Grid { get; set; }        // ตำแหน่งบนผัง "แถวเริ่ม / คอลัมน์เริ่ม / แถวจบ / คอลัมน์จบ"
    public string Area { get; set; }        // ตร.ม.
    public string Height { get; set; }
    public string Zone { get; set; }
    public string Vehicle { get; set; }
    public string Temp { get; set; }
    public string FloorLoad { get; set; }
    public string Dock { get; set; }
    public string Power { get; set; }
    public string Note { get; set; }        // ข้อความสั้นบนการ์ด
    public string Desc { get; set; }        // คำอธิบายในหน้ารายละเอียด
    public int Price { get; set; }
    public int OldPrice { get; set; }
    public string? Rating { get; set; }    // คะแนนเฉลี่ยจากรีวิว (null = ยังไม่มีรีวิว)
    public string Img { get; set; }         // ลิงก์รูป (หน้า view เติม ?w= ขนาดเอง)
}

// ===================== BookingViewModel =====================
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

// ===================== MyBookingViewModel =====================
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

// ===================== PaymentViewModel =====================
// ข้อมูลจากฟอร์มแจ้งชำระเงิน (Customer/Pay POST)
public class PaymentViewModel
{
    public string BookingId { get; set; }
    public string PayMethod { get; set; }   // transfer / qr / card
    public IFormFile? Slip { get; set; }
}

// ===================== PaymentHistoryViewModel =====================
// ประวัติการชำระในหน้า Profile
public class PaymentHistoryViewModel
{
    public string No { get; set; }
    public string Title { get; set; }
    public int Amount { get; set; }
    public string Detail { get; set; }
    public bool IsRefund { get; set; }
    public string? ReceiptId { get; set; }   // มีใบเสร็จแล้ว ลูกค้ากดดูได้
    public DateTime Date { get; set; }       // ใช้เรียงรายการ (ชำระ + คืนเงิน)
}

// ===================== ReviewViewModel =====================
// รีวิวห้อง ใช้ทั้งแสดงผลและรับค่าจากฟอร์ม (Customer/AddReview POST)
public class ReviewViewModel
{
    public string RoomId { get; set; }
    public string Name { get; set; }
    public int Stars { get; set; } = 5;
    public string Date { get; set; }
    public string Comment { get; set; }
}

// ===================== ProfileViewModel =====================
// ข้อมูลส่วนตัว / ที่อยู่ออกใบกำกับภาษี (Customer/SaveProfile POST)
public class ProfileViewModel
{
    public string Fullname { get; set; }
    public string Company { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string TaxId { get; set; }
    public string Address { get; set; }
}
