namespace Naphat_6507_L005.ViewModels;

// ViewModels ของหน้าฝั่งลูกค้า (CustomerController)
// 1 คลาสต่อ 1 งาน (ฟอร์มหรือสิ่งที่แสดงบนหน้า) รวมไว้ในไฟล์เดียวตามฝ่าย

// ===================== RoomViewModel =====================
// ข้อมูลห้องที่ใช้แสดงในหน้า Search / RoomDetail / BookRoom
// มาจากตาราง room + ข้อมูลเสริมที่ตารางยังไม่มีคอลัมน์ (อยู่ใน CustomerController._roomExtras)
public class RoomViewModel
{
    public string Id { get; set; } = "";
    public string Kind { get; set; } = "";        // s / m / l / xl / cold
    public bool Free { get; set; }
    public string Grid { get; set; } = "";        // ตำแหน่งบนผัง "แถวเริ่ม / คอลัมน์เริ่ม / แถวจบ / คอลัมน์จบ"
    public string Area { get; set; } = "";        // ตร.ม.
    public string Height { get; set; } = "";
    public string Zone { get; set; } = "";
    public string Vehicle { get; set; } = "";
    public string Temp { get; set; } = "";
    public string FloorLoad { get; set; } = "";
    public string Dock { get; set; } = "";
    public string Power { get; set; } = "";
    public string Note { get; set; } = "";        // ข้อความสั้นบนการ์ด
    public string Desc { get; set; } = "";        // คำอธิบายในหน้ารายละเอียด
    public int Price { get; set; }
    public int OldPrice { get; set; }
    public string? Rating { get; set; }    // คะแนนเฉลี่ยจากรีวิว (null = ยังไม่มีรีวิว)
    public string Img { get; set; } = "";         // ลิงก์รูป (หน้า view เติม ?w= ขนาดเอง)
}

// ===================== BookingViewModel =====================
// ข้อมูลจากฟอร์มจองห้อง (Customer/BookRoom POST)
public class BookingViewModel
{
    public string RoomId { get; set; } = "";
    public string TenantType { get; set; } = "corp";   // corp = บริษัท, person = บุคคลธรรมดา
    public string? CompanyName { get; set; }
    public string? TaxId { get; set; }
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today;
    public int Months { get; set; } = 6;
    public string? GoodsType { get; set; }
    public string? MoveInService { get; set; }
    public string? Note { get; set; }
    public string? PromoCode { get; set; }
    public bool AgreeTerms { get; set; }
    public List<DeclaredItemViewModel> Items { get; set; } = new();   // สินค้าที่ลูกค้าแจ้งว่าจะนำเข้า
}

// ===================== DeclaredItemViewModel =====================
// สินค้า 1 รายการที่ลูกค้าแจ้งตอนจอง (บันทึกลงตาราง stockitem สถานะรอตรวจรับ)
public class DeclaredItemViewModel
{
    public string? ItemName { get; set; }
    public int Quantity { get; set; }
    public string? Unit { get; set; }
}

// ===================== MyBookingViewModel =====================
// รายการจองที่แสดงในหน้า Profile
public class MyBookingViewModel
{
    public string Id { get; set; } = "";
    public string Room { get; set; } = "";
    public string Zone { get; set; } = "";
    public string Period { get; set; } = "";
    public int Months { get; set; }
    public int Rate { get; set; }
    public string Status { get; set; } = "";     // pending / checking / active / ended / cancelled (ตัวเล็ก ใช้กับ CSS)
    public DateTime BookedDate { get; set; }
    public string BookedOn { get; set; } = "";
    public bool CanReview { get; set; }    // เช่าแล้ว (ใช้งานอยู่ / สิ้นสุด) และยังไม่เคยรีวิวการจองนี้
}

// ===================== PaymentViewModel =====================
// ข้อมูลจากฟอร์มแจ้งชำระเงิน (Customer/Pay POST)
public class PaymentViewModel
{
    public string BookingId { get; set; } = "";
    public string PayMethod { get; set; } = "";   // transfer / qr / card
    public IFormFile? Slip { get; set; }
}

// ===================== PaymentHistoryViewModel =====================
// ประวัติการชำระในหน้า Profile
public class PaymentHistoryViewModel
{
    public string No { get; set; } = "";
    public string Title { get; set; } = "";
    public int Amount { get; set; }
    public string Detail { get; set; } = "";
    public bool IsRefund { get; set; }
    public string? ReceiptId { get; set; }   // มีใบเสร็จแล้ว ลูกค้ากดดูได้
    public DateTime Date { get; set; }       // ใช้เรียงรายการ (ชำระ + คืนเงิน)
}

// ===================== ReviewViewModel =====================
// รีวิวห้อง ใช้ทั้งแสดงผลและรับค่าจากฟอร์ม (Customer/AddReview POST)
public class ReviewViewModel
{
    public string RoomId { get; set; } = "";
    public string Name { get; set; } = "";
    public int Stars { get; set; } = 5;
    public string Date { get; set; } = "";
    public string Comment { get; set; } = "";
}

// ===================== ProfileViewModel =====================
// ข้อมูลส่วนตัว / ที่อยู่ออกใบกำกับภาษี (Customer/SaveProfile POST)
public class ProfileViewModel
{
    public string Fullname { get; set; } = "";
    public string Company { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string TaxId { get; set; } = "";
    public string Address { get; set; } = "";
}

// ===================== PromoOptionViewModel =====================
// โปรโมชั่น 1 ตัวในรายการให้เลือกตอนจอง (หน้า BookRoom)
public class PromoOptionViewModel
{
    public string Code { get; set; } = "";
    public string BigText { get; set; } = "";             // เช่น −35% หรือ −฿3,000
    public string Description { get; set; } = "";
    public int MinMonths { get; set; }                     // หน้าจอปิดตัวเลือกถ้าระยะสัญญาที่เลือกน้อยกว่านี้
    public List<string> Terms { get; set; } = new();       // ข้อกำหนดของโค้ดนี้
    public string? BadgeText { get; set; }                 // เช่น เหลือ 30 สิทธิ์
    public bool UserLimitReached { get; set; }             // ลูกค้าคนนี้ใช้ครบสิทธิ์แล้ว (เลือกไม่ได้)
}

// ===================== PriceQuoteViewModel =====================
// ยอดที่ต้องชำระก่อนเข้าใช้ + วันสิ้นสุดสัญญา คิดที่ CustomerController.BuildQuote ที่เดียว
// หน้า RoomDetail / BookRoom แสดงค่านี้ และขอค่าใหม่จาก Customer/PriceQuote เมื่อเปลี่ยนวันหรือระยะสัญญา
public class PriceQuoteViewModel
{
    public int Months { get; set; }
    public string RentText { get; set; } = "";          // ค่าเช่าเดือนแรก
    public int DepositMonths { get; set; }
    public string DepositText { get; set; } = "";       // เงินประกัน
    public string? DiscountText { get; set; }     // ส่วนลดจากโค้ด เช่น "−฿8,750" (null = ไม่มีส่วนลด)
    public string? PromoMessage { get; set; }     // ผลการตรวจโค้ด (ใช้ได้ / ใช้ไม่ได้เพราะอะไร)
    public bool PromoOk { get; set; }
    public string TotalText { get; set; } = "";         // ชำระก่อนเข้าใช้
    public string? NextMonthsText { get; set; }   // "เดือนที่ 2–6 ชำระเดือนละ ..." (สัญญา 1 เดือนไม่มี)
    public string EndDateText { get; set; } = "";       // วันสิ้นสุดสัญญา
}

// ===================== HomeViewModel =====================
// ตัวเลขบนหน้าหลัก (Customer/Dashboard) ดึงจากฐานข้อมูลจริง
public class HomeViewModel
{
    public int FreeCount { get; set; }              // ห้องว่างพร้อมส่งมอบ
    public int RoomCount { get; set; }
    public int TotalArea { get; set; }              // พื้นที่รวมทุกห้อง (ตร.ม.)
    public string? AvgRating { get; set; }          // คะแนนรีวิวเฉลี่ย (null = ยังไม่มีรีวิว)
    public int ReviewCount { get; set; }
    public int UsedPercent { get; set; }            // อัตราการเช่าทั้งคลัง
    public List<HomePromoViewModel> Promos { get; set; } = new();   // โปรโมชั่นทั้งหมดที่โชว์บนหน้าหลัก
    public RoomViewModel? Featured { get; set; }    // ห้องแนะนำ
    public List<HomeSizeCardViewModel> SizeCards { get; set; } = new();
}

// โปรโมชั่น 1 ใบบนหน้าหลัก (มาจากตาราง promocode ที่ Admin สร้าง)
public class HomePromoViewModel
{
    public string Code { get; set; } = "";
    public string BigText { get; set; } = "";       // เช่น −35% หรือ −฿3,000
    public string Description { get; set; } = "";
    public List<string> Conditions { get; set; } = new();   // เงื่อนไขสั้นๆ เช่น สัญญา 6 เดือนขึ้นไป
    public bool IsUpcoming { get; set; }            // ยังไม่ถึงวันเริ่ม (โชว์ว่า "เร็วๆ นี้" กดใช้ไม่ได้)
    public string? BadgeText { get; set; }          // ป้ายมุมการ์ด เช่น เหลือ 3 สิทธิ์ / เริ่ม 12 ธ.ค.
}

// การ์ดขนาดห้อง 1 ใบบนหน้าหลัก
public class HomeSizeCardViewModel
{
    public string Kind { get; set; } = "";          // ลิงก์ไปหน้า Search?size=
    public string Letter { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
    public string RangeText { get; set; } = "";     // เช่น 60–80 ตร.ม. · ว่าง 1 ห้อง
    public int MinPrice { get; set; }
    public bool Popular { get; set; }
}

// ===================== BookedPeriodViewModel =====================
// ช่วงเวลาที่ห้องถูกจองแล้ว 1 ช่วง (แสดงในหน้ารายละเอียดห้อง / หน้าจอง)
public class BookedPeriodViewModel
{
    public string BookingId { get; set; } = "";
    public DateOnly Start { get; set; }
    public DateOnly End { get; set; }
    public string Text { get; set; } = "";   // เช่น 1 ธ.ค. 2569 – 31 พ.ค. 2570
}
