namespace Naphat_6507_L005.ViewModels;

// ViewModel ของฝ่ายผู้จัดการ (ดูอย่างเดียว ไม่แก้ข้อมูล)

// ===================== MonthlyRevenueViewModel =====================
// รายรับ 1 เดือน (หน้า Manager/Revenue และกราฟในหน้า Dashboard)
public class MonthlyRevenueViewModel
{
    public DateTime Month { get; set; }          // วันที่ 1 ของเดือน
    public string MonthText { get; set; } = "";  // เช่น พ.ย. 2569
    public int Count { get; set; }               // จำนวนการชำระที่อนุมัติ
    public decimal Rent { get; set; }            // ค่าเช่า (รายรับจริง)
    public decimal Deposit { get; set; }         // เงินประกันที่รับเข้า
    public decimal Refund { get; set; }          // เงินประกันที่คืน
    public decimal Net => Rent + Deposit - Refund;   // เงินเข้าสุทธิ
    public int BarPercent { get; set; }          // ความยาวแท่งกราฟ (เทียบกับเดือนที่ค่าเช่าสูงสุด)
}

// ===================== ManagerRoomViewModel =====================
// ห้อง 1 ห้อง พร้อมผู้เช่าปัจจุบัน (หน้า Manager/Rooms)
public class ManagerRoomViewModel
{
    public string RoomId { get; set; } = "";
    public string SizeText { get; set; } = "";
    public string Zone { get; set; } = "";
    public int Area { get; set; }
    public decimal MonthlyRate { get; set; }
    public string Status { get; set; } = "";       // Available / Booked / Occupied
    public string StatusText { get; set; } = "";
    public string? BookingId { get; set; }
    public string? TenantName { get; set; }
    public string? EndDateText { get; set; }
    public int? DaysLeft { get; set; }             // วันที่เหลือก่อนหมดสัญญา (เฉพาะห้องที่มีผู้เช่า)
}

// ===================== RoomSizeSummaryViewModel =====================
// สรุปห้องตามขนาด (หน้า Manager/Rooms และ Dashboard)
public class RoomSizeSummaryViewModel
{
    public string SizeText { get; set; } = "";
    public int Total { get; set; }
    public int Available { get; set; }
    public int Booked { get; set; }
    public int Occupied { get; set; }
    public int UsedPercent => Total == 0 ? 0 : (int)Math.Round((Booked + Occupied) * 100.0 / Total);
}

// ===================== PendingWorkViewModel =====================
// งานค้างของแต่ละฝ่าย (หน้า Dashboard)
public class PendingWorkViewModel
{
    public string Department { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Color { get; set; } = "";
    public string Text { get; set; } = "";
    public int Count { get; set; }
}
