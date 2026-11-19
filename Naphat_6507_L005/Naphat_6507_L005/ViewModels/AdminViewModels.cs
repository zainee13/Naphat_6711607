namespace Naphat_6507_L005.ViewModels;

// ViewModel ของ Admin เว็บ

// ===================== AddUserViewModel =====================
// ฟอร์มเพิ่มผู้ใช้ใหม่ (Admin/AddUser) ใช้ได้ทั้งลูกค้าและพนักงานทุกฝ่าย
public class AddUserViewModel
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public string? Fullname { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = "Accounting";
}

// ===================== PromoFormViewModel =====================
// ฟอร์มเพิ่ม / แก้ไขโค้ดส่วนลด (Admin/AddPromo, Admin/EditPromo)
public class PromoFormViewModel
{
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string DiscountType { get; set; } = "Percent";   // Percent / Amount
    public decimal DiscountValue { get; set; }
    public int MinMonths { get; set; } = 1;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsEdit { get; set; }                         // true = แก้ไขโค้ดเดิม (เปลี่ยนตัวโค้ดไม่ได้)
}

// ===================== PromoListItemViewModel =====================
// โค้ดส่วนลด 1 แถวในหน้า Admin/Promos
public class PromoListItemViewModel
{
    public string Code { get; set; } = "";
    public string? Description { get; set; }
    public string DiscountText { get; set; } = "";           // 35% / ฿5,000
    public int MinMonths { get; set; }
    public string PeriodText { get; set; } = "";             // ช่วงวันที่ใช้ได้
    public bool IsActive { get; set; }
    public string StatusText { get; set; } = "";             // ใช้งานได้ / ปิด / หมดอายุ / ยังไม่เริ่ม
    public int UsedCount { get; set; }                       // จำนวนการจองที่ใช้โค้ดนี้
}

// ===================== UserRentalViewModel =====================
// ห้องที่ลูกค้ากำลังเช่า / จองไว้ 1 รายการ (คอลัมน์ "การเช่า" ในหน้า Admin/Users)
public class UserRentalViewModel
{
    public string RoomId { get; set; } = "";
    public string BookingId { get; set; } = "";
    public string Status { get; set; } = "";       // Pending / Checking / Active
    public string StatusText { get; set; } = "";   // รอชำระ / รอตรวจ / กำลังเช่า
}
