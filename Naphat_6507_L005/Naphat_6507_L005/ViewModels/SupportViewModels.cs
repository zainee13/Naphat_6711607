namespace Naphat_6507_L005.ViewModels;

// ViewModels ของระบบแจ้งปัญหา (CustomerServiceController และแท็บแจ้งปัญหาของลูกค้า)
// 1 คลาสต่อ 1 งาน (ฟอร์มหรือสิ่งที่แสดงบนหน้า) รวมไว้ในไฟล์เดียวตามฝ่าย

// ===================== TicketViewModel =====================
// ตั๋ว 1 ใบ ใช้ทั้งในรายการและหน้ารายละเอียด (Messages มีเฉพาะหน้ารายละเอียด)
public class TicketViewModel
{
    public int TicketId { get; set; }
    public string No => "TK-" + TicketId.ToString("0000");
    public string Category { get; set; }        // หมวดที่ลูกค้าเลือก (เก็บไว้หน้าหัวข้อ เช่น "[ไฟฟ้า] ...")
    public string Title { get; set; }           // หัวข้อที่ตัดหมวดออกแล้ว
    public string Priority { get; set; }        // High / Normal / Low
    public string Status { get; set; }          // Open / InProgress / Resolved
    public string CustomerName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? BookingId { get; set; }
    public string? RoomId { get; set; }
    public string CreatedText { get; set; }
    public string? ResolvedText { get; set; }
    public string? AssigneeName { get; set; }
    public List<TicketMessageViewModel> Messages { get; set; } = new();
    public int Unread { get; set; }             // ข้อความใหม่จากเจ้าหน้าที่ที่ลูกค้ายังไม่ได้เปิดอ่าน
}

// ===================== TicketMessageViewModel =====================
// ข้อความ 1 รายการในบทสนทนา (แยกออกมาจากช่อง Description)
public class TicketMessageViewModel
{
    public string Author { get; set; }
    public bool IsStaff { get; set; }
    public string TimeText { get; set; }
    public string Text { get; set; }
}

// ===================== NewTicketViewModel =====================
// ฟอร์มเปิดตั๋วใหม่ของลูกค้า (Customer/CreateTicket POST)
public class NewTicketViewModel
{
    public string? Category { get; set; }
    public string? Subject { get; set; }
    public string? Description { get; set; }
    public string? BookingId { get; set; }      // ไม่บังคับ
}

// ===================== TicketReplyViewModel =====================
// ฟอร์มตอบในตั๋ว (ทั้งลูกค้าและพนักงาน)
public class TicketReplyViewModel
{
    public int TicketId { get; set; }
    public string? Message { get; set; }
}
