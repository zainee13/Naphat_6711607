using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Naphat_6507_L005.Models;
using Naphat_6507_L005.ViewModels;

namespace Naphat_6507_L005.Controllers;

public class CustomerServiceController : Controller
{
    private readonly Csi402dbContext _db;

    public CustomerServiceController(Csi402dbContext db)
    {
        _db = db;
    }

    private static readonly CultureInfo _th = new("th-TH");

    // ===================== กติกาของตั๋ว (ใช้ร่วมกับ CustomerController) =====================

    // หมวดที่ลูกค้าเลือก -> ความด่วนที่ระบบตั้งให้
    public static readonly Dictionary<string, string> Categories = new()
    {
        { "ความปลอดภัย", "High" },
        { "ไฟฟ้า", "High" },
        { "ห้องเย็นมีปัญหา", "High" },
        { "สต็อกสินค้า", "Normal" },
        { "การชำระเงิน", "Normal" },
        { "สัญญาเช่า", "Normal" },
        { "อื่นๆ", "Low" },
    };

    public static readonly Dictionary<string, string> PriorityText = new()
    {
        { "High", "ด่วนมาก" },
        { "Normal", "ปกติ" },
        { "Low", "ต่ำ" },
    };

    public static readonly Dictionary<string, string> StatusText = new()
    {
        { "Open", "เปิดใหม่" },
        { "InProgress", "กำลังดำเนินการ" },
        { "Resolved", "ปิดแล้ว" },
    };

    // ตารางไม่มีช่องเก็บข้อความตอบกลับ จึงต่อท้ายในช่อง Description คั่นด้วยบรรทัด "---"
    // แต่ละข้อความตอบขึ้นต้นด้วยหัว "[ชื่อ (ลูกค้า|เจ้าหน้าที่) · วันเวลา]"
    private const string MessageSeparator = "\n---\n";
    private static readonly Regex _header = new(@"^\[(.+) \((ลูกค้า|เจ้าหน้าที่)\) · (.+?)\]\n", RegexOptions.Compiled);

    // ข้อความใหม่ที่จะต่อท้าย Description (กันไม่ให้ผู้ใช้พิมพ์ตัวคั่นเองแล้วทำให้แยกข้อความผิด)
    public static string AppendMessage(string description, string author, bool isStaff, string text)
    {
        var clean = text.Trim().Replace("\r\n", "\n").Replace(MessageSeparator, "\n- - -\n");
        var header = "[" + author + " (" + (isStaff ? "เจ้าหน้าที่" : "ลูกค้า") + ") · " + DateTime.Now.ToString("d MMM yyyy HH:mm", _th) + "]\n";
        return description + MessageSeparator + header + clean;
    }

    // แยกบทสนทนาออกจาก Description: ส่วนแรกคือข้อความตอนเปิดตั๋วของลูกค้า
    private static List<TicketMessageViewModel> ParseMessages(string description, string customerName, string createdText)
    {
        List<TicketMessageViewModel> list = new();
        var parts = description.Replace("\r\n", "\n").Split(MessageSeparator);
        list.Add(new TicketMessageViewModel { Author = customerName, IsStaff = false, TimeText = createdText, Text = parts[0] });

        for (int i = 1; i < parts.Length; i++)
        {
            var m = _header.Match(parts[i]);
            if (m.Success)
            {
                list.Add(new TicketMessageViewModel
                {
                    Author = m.Groups[1].Value,
                    IsStaff = m.Groups[2].Value == "เจ้าหน้าที่",
                    TimeText = m.Groups[3].Value,
                    Text = parts[i].Substring(m.Length)
                });
            }
            else
            {
                list[^1].Text += MessageSeparator + parts[i];   // ไม่ใช่หัวข้อความ: เป็นส่วนหนึ่งของข้อความก่อนหน้า
            }
        }
        return list;
    }

    // ดึงตั๋ว (กรองตามลูกค้า / สถานะ / รหัสตั๋วได้) แล้วแปลงเป็น ViewModel
    public static List<TicketViewModel> GetTickets(Csi402dbContext db, int? userId, string? status, int? ticketId, bool withMessages)
    {
        var rows = (from t in db.Supporttickets
                    join u in db.Users on t.UserId equals u.UserId
                    join b in db.Bookings on t.BookingId equals b.BookingId into bj
                    from b in bj.DefaultIfEmpty()
                    join a in db.Users on t.AssignedTo equals (int?)a.UserId into aj
                    from a in aj.DefaultIfEmpty()
                    where (userId == null || t.UserId == userId)
                          && (status == null || t.TicketStatus == status)
                          && (ticketId == null || t.TicketId == ticketId)
                    select new { t, u, RoomId = b == null ? null : b.RoomId, Assignee = a == null ? null : a.Fullname })
                   .ToList();

        // เรียง: ยังไม่ปิดก่อน, ด่วนมากก่อน, ใหม่สุดก่อน
        var priorityOrder = new Dictionary<string, int> { { "High", 0 }, { "Normal", 1 }, { "Low", 2 } };
        rows = rows.OrderBy(r => r.t.TicketStatus == "Resolved" ? 1 : 0)
                   .ThenBy(r => priorityOrder.ContainsKey(r.t.Priority) ? priorityOrder[r.t.Priority] : 3)
                   .ThenByDescending(r => r.t.CreatedAt)
                   .ToList();

        List<TicketViewModel> list = new();
        foreach (var row in rows)
        {
            TicketViewModel item = new();
            item.TicketId = row.t.TicketId;
            var subject = row.t.Subject;
            var close = subject.StartsWith("[") ? subject.IndexOf(']') : -1;
            item.Category = close > 0 ? subject.Substring(1, close - 1) : "อื่นๆ";
            item.Title = close > 0 ? subject.Substring(close + 1).Trim() : subject;
            item.Priority = row.t.Priority;
            item.Status = row.t.TicketStatus;
            item.CustomerName = string.IsNullOrEmpty(row.u.CompanyName) ? row.u.Fullname : row.u.CompanyName;
            item.Phone = row.u.Phone;
            item.Email = row.u.Email;
            item.BookingId = row.t.BookingId;
            item.RoomId = row.RoomId;
            item.CreatedText = row.t.CreatedAt.ToString("d MMM yyyy HH:mm", _th);
            item.ResolvedText = row.t.ResolvedAt?.ToString("d MMM yyyy HH:mm", _th);
            item.AssigneeName = row.Assignee;
            if (withMessages)
            {
                item.Messages = ParseMessages(row.t.Description, row.u.Fullname, item.CreatedText);
            }
            list.Add(item);
        }
        return list;
    }

    // ===================== ตัวช่วย =====================

    // เข้าได้เฉพาะพนักงานบริการลูกค้า
    private bool IsCustomerService => HttpContext.Session.GetString("Role") == "CustomerService";

    private void SetUserBag()
    {
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "พนักงานบริการลูกค้า";
        ViewBag.Role = "CustomerService";

        // จำนวนตั๋วที่ยังไม่มีใครรับเรื่อง: _Layout ใช้แสดงป้ายตัวเลขที่เมนู
        ViewBag.OpenTicketCount = (from t in _db.Supporttickets where t.TicketStatus == "Open" select t).Count();
    }

    // พนักงานที่ล็อกอินอยู่ (ใช้บันทึก AssignedTo)
    private User? GetCurrentStaff()
    {
        var username = HttpContext.Session.GetString("Username");
        return (from u in _db.Users where u.Username == username && u.Role == "CustomerService" select u).FirstOrDefault();
    }

    private const string NotInDbError = "บัญชีนี้ไม่มีในตาราง user กรุณาเข้าสู่ระบบด้วยบัญชีพนักงานบริการลูกค้าในฐานข้อมูล (เช่น cs01)";

    // ===================== หน้า =====================

    // ภาพรวม: ตัวเลขสรุป + ตั๋วด่วนมากที่ยังไม่ปิด
    public IActionResult Dashboard()
    {
        if (!IsCustomerService)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        ViewBag.InProgressCount = (from t in _db.Supporttickets where t.TicketStatus == "InProgress" select t).Count();
        ViewBag.UrgentCount = (from t in _db.Supporttickets where t.TicketStatus != "Resolved" && t.Priority == "High" select t).Count();
        ViewBag.ResolvedToday = (from t in _db.Supporttickets where t.ResolvedAt >= today && t.ResolvedAt < tomorrow select t).Count();

        var urgent = GetTickets(_db, null, null, null, false)
                     .Where(t => t.Status != "Resolved" && t.Priority == "High")
                     .ToList();
        return View(urgent);
    }

    // รายการตั๋วทั้งหมด (กรองตามสถานะได้)
    public IActionResult Tickets(string? status)
    {
        if (!IsCustomerService)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        if (status != null && !StatusText.ContainsKey(status))
        {
            status = null;
        }
        ViewBag.FilterStatus = status;
        return View(GetTickets(_db, null, status, null, false));
    }

    // หน้าตั๋ว 1 ใบ: ข้อมูลลูกค้า + บทสนทนา + ปุ่มรับเรื่อง/ตอบ/ปรับความด่วน/ปิดตั๋ว
    public IActionResult Ticket(int id)
    {
        if (!IsCustomerService)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        var ticket = GetTickets(_db, null, null, id, true).FirstOrDefault();
        if (ticket == null)
        {
            TempData["Error"] = "ไม่พบตั๋วนี้";
            return RedirectToAction("Tickets");
        }
        return View(ticket);
    }

    // รับเรื่อง (POST): ตัวเองเป็นผู้รับผิดชอบ สถานะกำลังดำเนินการ
    [HttpPost]
    public IActionResult Assign(int id)
    {
        if (!IsCustomerService)
        {
            return RedirectToAction("Login", "Account");
        }

        var staff = GetCurrentStaff();
        if (staff == null)
        {
            TempData["Error"] = NotInDbError;
            return RedirectToAction("Ticket", new { id });
        }

        var ticket = (from t in _db.Supporttickets where t.TicketId == id select t).FirstOrDefault();
        if (ticket == null || ticket.TicketStatus == "Resolved")
        {
            TempData["Error"] = "รับเรื่องได้เฉพาะตั๋วที่ยังไม่ปิด";
            return RedirectToAction("Tickets");
        }

        ticket.AssignedTo = staff.UserId;
        ticket.TicketStatus = "InProgress";
        _db.SaveChanges();

        TempData["Success"] = "รับเรื่องตั๋ว TK-" + id.ToString("0000") + " แล้ว";
        return RedirectToAction("Ticket", new { id });
    }

    // ตอบลูกค้า (POST): ต่อท้ายบทสนทนา ถ้ายังไม่มีผู้รับผิดชอบให้เป็นคนตอบ
    [HttpPost]
    public IActionResult Reply(TicketReplyViewModel data)
    {
        if (!IsCustomerService)
        {
            return RedirectToAction("Login", "Account");
        }

        var staff = GetCurrentStaff();
        if (staff == null)
        {
            TempData["Error"] = NotInDbError;
            return RedirectToAction("Ticket", new { id = data.TicketId });
        }
        if (string.IsNullOrWhiteSpace(data.Message))
        {
            TempData["Error"] = "กรุณาพิมพ์ข้อความก่อนส่ง";
            return RedirectToAction("Ticket", new { id = data.TicketId });
        }

        var ticket = (from t in _db.Supporttickets where t.TicketId == data.TicketId select t).FirstOrDefault();
        if (ticket == null)
        {
            TempData["Error"] = "ไม่พบตั๋วนี้";
            return RedirectToAction("Tickets");
        }

        ticket.Description = AppendMessage(ticket.Description, staff.Fullname, true, data.Message);
        ticket.AssignedTo ??= staff.UserId;
        if (ticket.TicketStatus == "Open")
        {
            ticket.TicketStatus = "InProgress";
        }
        _db.SaveChanges();

        TempData["Success"] = "ส่งข้อความถึงลูกค้าแล้ว";
        return RedirectToAction("Ticket", new { id = data.TicketId });
    }

    // ปรับความด่วน (POST)
    [HttpPost]
    public IActionResult SetPriority(int id, string priority)
    {
        if (!IsCustomerService)
        {
            return RedirectToAction("Login", "Account");
        }
        if (!PriorityText.ContainsKey(priority))
        {
            TempData["Error"] = "ความด่วนไม่ถูกต้อง";
            return RedirectToAction("Ticket", new { id });
        }

        var ticket = (from t in _db.Supporttickets where t.TicketId == id select t).FirstOrDefault();
        if (ticket == null)
        {
            TempData["Error"] = "ไม่พบตั๋วนี้";
            return RedirectToAction("Tickets");
        }

        ticket.Priority = priority;
        _db.SaveChanges();

        TempData["Success"] = "ปรับความด่วนเป็น \"" + PriorityText[priority] + "\" แล้ว";
        return RedirectToAction("Ticket", new { id });
    }

    // ปิดตั๋ว (POST): บันทึกเวลาปิด
    [HttpPost]
    public IActionResult Close(int id)
    {
        if (!IsCustomerService)
        {
            return RedirectToAction("Login", "Account");
        }

        var staff = GetCurrentStaff();
        if (staff == null)
        {
            TempData["Error"] = NotInDbError;
            return RedirectToAction("Ticket", new { id });
        }

        var ticket = (from t in _db.Supporttickets where t.TicketId == id select t).FirstOrDefault();
        if (ticket == null || ticket.TicketStatus == "Resolved")
        {
            TempData["Error"] = "ตั๋วนี้ปิดไปแล้ว";
            return RedirectToAction("Tickets");
        }

        ticket.TicketStatus = "Resolved";
        ticket.ResolvedAt = DateTime.Now;
        ticket.AssignedTo ??= staff.UserId;
        _db.SaveChanges();

        TempData["Success"] = "ปิดตั๋ว TK-" + id.ToString("0000") + " แล้ว";
        return RedirectToAction("Ticket", new { id });
    }
}
