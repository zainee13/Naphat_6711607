using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Naphat_6507_L005.Models;
using Naphat_6507_L005.ViewModels;

namespace Naphat_6507_L005.Controllers;

public class AccountingController : Controller
{
    private readonly Csi402dbContext _db;

    public AccountingController(Csi402dbContext db)
    {
        _db = db;
    }

    private static readonly CultureInfo _th = new("th-TH");

    // ===================== ตัวช่วย =====================

    // เข้าได้เฉพาะพนักงานบัญชี
    private bool IsAccounting => HttpContext.Session.GetString("Role") == "Accounting";

    private void SetUserBag()
    {
        ViewBag.FullName = HttpContext.Session.GetString("FullName") ?? "พนักงานบัญชี";
        ViewBag.Role = "Accounting";
    }

    // พนักงานที่ล็อกอินอยู่ (ใช้บันทึกว่าใครอนุมัติ)
    private User? GetCurrentStaff()
    {
        var username = HttpContext.Session.GetString("Username");
        return (from u in _db.Users where u.Username == username && u.Role == "Accounting" select u).FirstOrDefault();
    }

    // ดึงรายการชำระเงิน + การจอง + ห้อง + ลูกค้า แล้วแปลงเป็น ViewModel
    private List<AccountingPaymentViewModel> GetPayments(string? status, int? paymentId)
    {
        var rows = (from p in _db.Payments
                    join b in _db.Bookings on p.BookingId equals b.BookingId
                    join r in _db.Rooms on b.RoomId equals r.RoomId
                    join u in _db.Users on b.UserId equals u.UserId
                    where (status == null || p.PaymentStatus == status)
                          && (paymentId == null || p.PaymentId == paymentId)
                    orderby p.PaymentDate
                    select new { p, b, r, u }).ToList();

        List<AccountingPaymentViewModel> list = new();
        foreach (var row in rows)
        {
            AccountingPaymentViewModel item = new();
            item.PaymentId = row.p.PaymentId;
            item.No = "PAY-" + row.p.PaymentId.ToString("00000");
            item.BookingId = row.b.BookingId;
            item.RoomId = row.r.RoomId;
            item.Zone = row.r.Zone ?? "";
            item.Period = row.b.StartDate.ToString("d MMM yyyy", _th) + " – " + row.b.EndDate.ToString("d MMM yyyy", _th);
            item.MonthlyRate = row.r.MonthlyRate;
            item.Months = row.r.MonthlyRate > 0 ? (int)(row.b.TotalRent / row.r.MonthlyRate) : 0;
            item.Deposit = row.b.Deposit;
            item.Amount = row.p.Amount;
            item.MethodText = CustomerController._payMethodText.ContainsKey(row.p.PaymentMethod)
                              ? CustomerController._payMethodText[row.p.PaymentMethod] : row.p.PaymentMethod;
            item.PaidDate = row.p.PaymentDate?.ToString("d MMM yyyy HH:mm", _th) ?? "-";
            item.DueDate = row.p.DueDate?.ToString("d MMM yyyy", _th) ?? "-";
            item.SlipUrl = row.p.SlipImageUrl;
            item.Status = row.p.PaymentStatus;
            item.CustomerName = row.u.Fullname;
            item.Company = row.u.CompanyName;
            item.TaxId = row.u.TaxId;
            item.Phone = row.u.Phone;
            item.Email = row.u.Email;
            item.Address = row.u.Address;
            item.Note = row.b.Note;
            list.Add(item);
        }
        return list;
    }

    // ===================== หน้า =====================

    // ภาพรวม: ตัวเลขสรุป + รายการชำระที่รอตรวจ
    public IActionResult Dashboard()
    {
        if (!IsAccounting)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        ViewBag.PendingCount = (from p in _db.Payments where p.PaymentStatus == "Pending" select p).Count();
        ViewBag.PendingAmount = (from p in _db.Payments where p.PaymentStatus == "Pending" select p.Amount).Sum();
        ViewBag.ApprovedToday = (from p in _db.Payments
                                 where p.PaymentStatus == "Approved" && p.VerifiedAt >= today && p.VerifiedAt < tomorrow
                                 select p.Amount).Sum();
        ViewBag.RejectedToday = (from p in _db.Payments
                                 where p.PaymentStatus == "Rejected" && p.VerifiedAt >= today && p.VerifiedAt < tomorrow
                                 select p).Count();

        return View(GetPayments("Pending", null));
    }

    // หน้าตรวจการชำระ 1 รายการ (GET) - แบบเดียวกับ Admin/EditUser
    public IActionResult VerifyPayment(int? id)
    {
        if (!IsAccounting)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        var payment = GetPayments(null, id).FirstOrDefault();
        if (payment == null)
        {
            TempData["Error"] = "ไม่พบรายการชำระเงินนี้";
            return RedirectToAction("Dashboard");
        }
        return View(payment);
    }

    // อนุมัติการชำระ (POST)
    [HttpPost]
    public IActionResult Approve(int id)
    {
        if (!IsAccounting)
        {
            return RedirectToAction("Login", "Account");
        }

        var staff = GetCurrentStaff();
        if (staff == null)
        {
            TempData["Error"] = "บัญชีนี้ไม่มีในตาราง user กรุณาเข้าสู่ระบบด้วยบัญชีพนักงานบัญชีในฐานข้อมูล (เช่น acc01)";
            return RedirectToAction("VerifyPayment", new { id });
        }

        var payment = (from p in _db.Payments where p.PaymentId == id select p).FirstOrDefault();
        if (payment == null || payment.PaymentStatus != "Pending")
        {
            TempData["Error"] = "รายการนี้ไม่ได้อยู่ในสถานะรอตรวจสอบ";
            return RedirectToAction("Dashboard");
        }

        var booking = (from b in _db.Bookings where b.BookingId == payment.BookingId select b).First();
        var room = (from r in _db.Rooms where r.RoomId == booking.RoomId select r).First();

        // payment: อนุมัติ + ใครอนุมัติ เมื่อไหร่
        payment.PaymentStatus = "Approved";
        payment.VerifiedBy = staff.UserId;
        payment.VerifiedAt = DateTime.Now;

        // booking: ใช้งานได้ / ห้อง: มีผู้เช่าแล้ว
        booking.BookingStatus = "Active";
        room.Status = "Occupied";

        // ออกใบแจ้งหนี้ + ใบเสร็จอัตโนมัติ
        var receiptId = CreateDocuments(payment, booking, room, staff.UserId);
        _db.SaveChanges();

        TempData["Success"] = "อนุมัติการชำระ PAY-" + payment.PaymentId.ToString("00000") + " แล้ว การจอง " + booking.BookingId
                              + " ใช้งานได้ ออกใบเสร็จ " + receiptId;
        return RedirectToAction("Dashboard");
    }

    // ===================== ใบแจ้งหนี้ / ใบเสร็จ =====================

    // สร้างใบแจ้งหนี้ (การจองละ 1 ใบ) และใบเสร็จ (การชำระละ 1 ใบ) ถ้ายังไม่มี ยังไม่ SaveChanges
    // ไม่คิด VAT: ยอด = ค่าเช่าเดือนแรก + เงินประกัน
    private string CreateDocuments(Payment payment, Booking booking, Room room, int staffId)
    {
        var year = DateTime.Now.Year;

        var hasInvoice = (from i in _db.Invoices where i.BookingId == booking.BookingId select i).Any();
        if (!hasInvoice)
        {
            Invoice invoice = new();
            invoice.InvoiceId = NewDocumentId("INV-" + year + "-", (from i in _db.Invoices select i.InvoiceId).ToList());
            invoice.BookingId = booking.BookingId;
            invoice.RentAmount = room.MonthlyRate;
            invoice.DepositAmount = booking.Deposit;
            invoice.VatAmount = 0;
            invoice.TotalAmount = room.MonthlyRate + booking.Deposit;
            invoice.IssuedDate = DateTime.Now;
            invoice.IssuedBy = staffId;
            _db.Invoices.Add(invoice);
        }

        var receipt = (from r in _db.Receipts where r.PaymentId == payment.PaymentId select r).FirstOrDefault();
        if (receipt == null)
        {
            receipt = new Receipt();
            receipt.ReceiptId = NewDocumentId("RCP-" + year + "-", (from r in _db.Receipts select r.ReceiptId).ToList());
            receipt.PaymentId = payment.PaymentId;
            receipt.IssuedDate = DateTime.Now;
            receipt.IssuedBy = staffId;
            _db.Receipts.Add(receipt);
        }
        return receipt.ReceiptId;
    }

    // เลขเอกสารถัดไป เช่น INV-2026-0001
    private static string NewDocumentId(string prefix, List<string> existing)
    {
        int no = existing.Count(id => id.StartsWith(prefix)) + 1;
        while (existing.Contains(prefix + no.ToString("0000")))
        {
            no++;
        }
        return prefix + no.ToString("0000");
    }

    // รายการชำระที่อนุมัติแล้ว + เลขใบแจ้งหนี้ / ใบเสร็จ
    public IActionResult Invoices()
    {
        if (!IsAccounting)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        var rows = (from p in _db.Payments
                    join b in _db.Bookings on p.BookingId equals b.BookingId
                    join u in _db.Users on b.UserId equals u.UserId
                    where p.PaymentStatus == "Approved"
                    orderby p.VerifiedAt descending
                    select new
                    {
                        p,
                        b,
                        u,
                        InvoiceId = (from i in _db.Invoices where i.BookingId == b.BookingId select i.InvoiceId).FirstOrDefault(),
                        ReceiptId = (from r in _db.Receipts where r.PaymentId == p.PaymentId select r.ReceiptId).FirstOrDefault()
                    }).ToList();

        List<InvoiceListItemViewModel> list = new();
        foreach (var row in rows)
        {
            InvoiceListItemViewModel item = new();
            item.PaymentId = row.p.PaymentId;
            item.PaymentNo = "PAY-" + row.p.PaymentId.ToString("00000");
            item.BookingId = row.b.BookingId;
            item.RoomId = row.b.RoomId;
            item.CustomerName = string.IsNullOrEmpty(row.u.CompanyName) ? row.u.Fullname : row.u.CompanyName;
            item.Amount = row.p.Amount;
            item.ApprovedDate = row.p.VerifiedAt?.ToString("d MMM yyyy", _th) ?? "-";
            item.InvoiceId = row.InvoiceId;
            item.ReceiptId = row.ReceiptId;
            list.Add(item);
        }
        return View(list);
    }

    // ออกเอกสารย้อนหลังให้รายการที่อนุมัติไปก่อนมีระบบใบเสร็จ (POST)
    [HttpPost]
    public IActionResult IssueDocuments(int id)
    {
        if (!IsAccounting)
        {
            return RedirectToAction("Login", "Account");
        }

        var staff = GetCurrentStaff();
        if (staff == null)
        {
            TempData["Error"] = "บัญชีนี้ไม่มีในตาราง user กรุณาเข้าสู่ระบบด้วยบัญชีพนักงานบัญชีในฐานข้อมูล (เช่น acc01)";
            return RedirectToAction("Invoices");
        }

        var payment = (from p in _db.Payments where p.PaymentId == id && p.PaymentStatus == "Approved" select p).FirstOrDefault();
        if (payment == null)
        {
            TempData["Error"] = "ออกเอกสารได้เฉพาะการชำระที่อนุมัติแล้ว";
            return RedirectToAction("Invoices");
        }

        var booking = (from b in _db.Bookings where b.BookingId == payment.BookingId select b).First();
        var room = (from r in _db.Rooms where r.RoomId == booking.RoomId select r).First();
        var receiptId = CreateDocuments(payment, booking, room, staff.UserId);
        _db.SaveChanges();

        TempData["Success"] = "ออกใบเสร็จ " + receiptId + " แล้ว";
        return RedirectToAction("Invoices");
    }

    // หน้าเอกสารสำหรับพิมพ์ (ฝ่ายบัญชีเปิดได้ทุกใบ)
    public IActionResult Document(string id)
    {
        if (!IsAccounting)
        {
            return RedirectToAction("Login", "Account");
        }

        var doc = BuildReceiptDocument(_db, id);
        if (doc == null)
        {
            TempData["Error"] = "ไม่พบใบเสร็จ " + id;
            return RedirectToAction("Invoices");
        }
        doc.BackUrl = Url.Action("Invoices", "Accounting")!;
        return View("ReceiptDocument", doc);
    }

    // รวมข้อมูลใบเสร็จ + ใบแจ้งหนี้ + การชำระ + การจอง + ลูกค้า (CustomerController.Receipt ใช้ด้วย)
    public static ReceiptDocumentViewModel? BuildReceiptDocument(Csi402dbContext db, string receiptId)
    {
        var row = (from r in db.Receipts
                   join p in db.Payments on r.PaymentId equals p.PaymentId
                   join b in db.Bookings on p.BookingId equals b.BookingId
                   join rm in db.Rooms on b.RoomId equals rm.RoomId
                   join u in db.Users on b.UserId equals u.UserId
                   join s in db.Users on r.IssuedBy equals s.UserId
                   where r.ReceiptId == receiptId
                   select new { r, p, b, rm, u, s }).FirstOrDefault();
        if (row == null)
        {
            return null;
        }
        var invoice = (from i in db.Invoices where i.BookingId == row.b.BookingId select i).FirstOrDefault();

        ReceiptDocumentViewModel doc = new();
        doc.ReceiptId = row.r.ReceiptId;
        doc.ReceiptDate = row.r.IssuedDate.ToString("d MMMM yyyy", _th);
        doc.InvoiceId = invoice?.InvoiceId ?? "-";
        doc.InvoiceDate = invoice?.IssuedDate.ToString("d MMMM yyyy", _th) ?? "-";
        doc.IssuedByName = row.s.Fullname;
        doc.PaymentNo = "PAY-" + row.p.PaymentId.ToString("00000");
        doc.PaidDate = row.p.PaymentDate?.ToString("d MMMM yyyy", _th) ?? "-";
        doc.MethodText = CustomerController._payMethodText.ContainsKey(row.p.PaymentMethod)
                         ? CustomerController._payMethodText[row.p.PaymentMethod] : row.p.PaymentMethod;
        doc.BookingId = row.b.BookingId;
        doc.RoomId = row.rm.RoomId;
        doc.Zone = row.rm.Zone ?? "";
        doc.Period = row.b.StartDate.ToString("d MMM yyyy", _th) + " – " + row.b.EndDate.ToString("d MMM yyyy", _th);
        doc.Months = row.rm.MonthlyRate > 0 ? (int)(row.b.TotalRent / row.rm.MonthlyRate) : 0;
        doc.RentAmount = invoice?.RentAmount ?? row.rm.MonthlyRate;
        doc.DepositAmount = invoice?.DepositAmount ?? row.b.Deposit;
        doc.VatAmount = invoice?.VatAmount ?? 0;
        doc.TotalAmount = invoice?.TotalAmount ?? doc.RentAmount + doc.DepositAmount;
        doc.PaidAmount = row.p.Amount;
        doc.CustomerName = row.u.Fullname;
        doc.Company = row.u.CompanyName;
        doc.TaxId = row.u.TaxId;
        doc.Address = row.u.Address;
        doc.Phone = row.u.Phone;
        doc.Email = row.u.Email;
        return doc;
    }

    // ปฏิเสธการชำระ (POST): ลูกค้าต้องชำระใหม่ เหตุผลต่อท้าย Note ของการจอง
    [HttpPost]
    public IActionResult Reject(RejectPaymentViewModel data)
    {
        if (!IsAccounting)
        {
            return RedirectToAction("Login", "Account");
        }

        var staff = GetCurrentStaff();
        if (staff == null)
        {
            TempData["Error"] = "บัญชีนี้ไม่มีในตาราง user กรุณาเข้าสู่ระบบด้วยบัญชีพนักงานบัญชีในฐานข้อมูล (เช่น acc01)";
            return RedirectToAction("VerifyPayment", new { id = data.PaymentId });
        }

        if (string.IsNullOrWhiteSpace(data.Reason))
        {
            TempData["Error"] = "กรุณาใส่เหตุผลที่ปฏิเสธ";
            return RedirectToAction("VerifyPayment", new { id = data.PaymentId });
        }

        var payment = (from p in _db.Payments where p.PaymentId == data.PaymentId select p).FirstOrDefault();
        if (payment == null || payment.PaymentStatus != "Pending")
        {
            TempData["Error"] = "รายการนี้ไม่ได้อยู่ในสถานะรอตรวจสอบ";
            return RedirectToAction("Dashboard");
        }

        var booking = (from b in _db.Bookings where b.BookingId == payment.BookingId select b).First();
        var no = "PAY-" + payment.PaymentId.ToString("00000");

        // payment: ปฏิเสธ + ใครตรวจ เมื่อไหร่
        payment.PaymentStatus = "Rejected";
        payment.VerifiedBy = staff.UserId;
        payment.VerifiedAt = DateTime.Now;

        // booking: กลับไปรอชำระ + เก็บเหตุผลไว้ใน Note ให้ลูกค้าเห็น
        booking.BookingStatus = "Pending";
        var line = RejectNotePrefix + no + "] " + data.Reason.Trim();
        booking.Note = string.IsNullOrEmpty(booking.Note) ? line : booking.Note + "\n" + line;
        _db.SaveChanges();

        TempData["Success"] = "ปฏิเสธการชำระ " + no + " แล้ว ลูกค้าต้องชำระใหม่";
        return RedirectToAction("Dashboard");
    }

    // ข้อความนำหน้าเหตุผลที่ปฏิเสธใน booking.Note (CustomerController ใช้หาเหตุผลมาแสดง)
    public const string RejectNotePrefix = "[ปฏิเสธการชำระ ";

    // ===================== สรุปรายรับ =====================

    // รายรับรายวันในช่วงวันที่ (ค่าเริ่มต้น: วันที่ 1 ของเดือนนี้ ถึงวันนี้)
    // รายรับ = การชำระที่อนุมัติแล้ว นับตามวันที่อนุมัติ / คืนเงิน = ตาราง refund ตามวันที่คืน
    public IActionResult DailySummary(DateTime? from, DateTime? to)
    {
        if (!IsAccounting)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        var start = (from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
        var end = (to ?? DateTime.Today).Date;
        if (end < start)
        {
            TempData["Error"] = "วันที่สิ้นสุดต้องไม่ก่อนวันที่เริ่ม";
            end = start;
        }
        var endExclusive = end.AddDays(1);
        ViewBag.From = start.ToString("yyyy-MM-dd");
        ViewBag.To = end.ToString("yyyy-MM-dd");
        ViewBag.RangeText = start.ToString("d MMM yyyy", _th) + " – " + end.ToString("d MMM yyyy", _th);

        // การชำระที่อนุมัติในช่วง + ยอดค่าเช่าจากใบแจ้งหนี้ (ถ้ายังไม่มีใบแจ้งหนี้ ใช้ยอดชำระ − เงินประกัน)
        var payments = (from p in _db.Payments
                        join b in _db.Bookings on p.BookingId equals b.BookingId
                        where p.PaymentStatus == "Approved" && p.VerifiedAt >= start && p.VerifiedAt < endExclusive
                        select new
                        {
                            Date = p.VerifiedAt!.Value,
                            p.Amount,
                            b.Deposit,
                            InvoiceRent = (from i in _db.Invoices where i.BookingId == b.BookingId select (decimal?)i.RentAmount).FirstOrDefault()
                        }).ToList();

        var refunds = (from rf in _db.Refunds
                       where rf.RefundStatus == "Completed" && rf.RefundDate >= start && rf.RefundDate < endExclusive
                       select new { Date = rf.RefundDate!.Value, rf.RefundAmount }).ToList();

        // รวมเป็นรายวัน
        Dictionary<DateTime, DailySummaryRowViewModel> days = new();
        DailySummaryRowViewModel GetDay(DateTime d)
        {
            if (!days.ContainsKey(d.Date))
            {
                DailySummaryRowViewModel row = new();
                row.Date = d.Date;
                row.DateText = d.ToString("ddd d MMM yyyy", _th);
                days[d.Date] = row;
            }
            return days[d.Date];
        }

        foreach (var p in payments)
        {
            var rent = p.InvoiceRent ?? Math.Max(p.Amount - p.Deposit, 0);
            var day = GetDay(p.Date);
            day.Count++;
            day.Rent += rent;
            day.Deposit += p.Amount - rent;
        }
        foreach (var rf in refunds)
        {
            GetDay(rf.Date).Refund += rf.RefundAmount;
        }

        var list = days.Values.OrderByDescending(d => d.Date).ToList();
        ViewBag.TotalRent = list.Sum(d => d.Rent);
        ViewBag.TotalDeposit = list.Sum(d => d.Deposit);
        ViewBag.TotalRefund = list.Sum(d => d.Refund);
        ViewBag.TotalNet = list.Sum(d => d.Net);
        ViewBag.TotalCount = list.Sum(d => d.Count);
        return View(list);
    }

    // ===================== การคืนเงิน =====================

    // วิธีคืนเงินที่ให้เลือก
    public static readonly List<string> RefundMethods = new() { "โอนเข้าบัญชีเดิม", "โอนบัญชีอื่น", "เงินสด" };

    // การจองที่ใช้งานอยู่ (คืนเงินประกันได้) + ข้อมูลที่ใช้แสดง
    private List<RefundBookingViewModel> GetActiveBookings(string? bookingId)
    {
        var rows = (from b in _db.Bookings
                    join r in _db.Rooms on b.RoomId equals r.RoomId
                    join u in _db.Users on b.UserId equals u.UserId
                    where b.BookingStatus == "Active" && (bookingId == null || b.BookingId == bookingId)
                    orderby b.EndDate
                    select new { b, r, u }).ToList();

        var today = DateOnly.FromDateTime(DateTime.Today);
        List<RefundBookingViewModel> list = new();
        foreach (var row in rows)
        {
            RefundBookingViewModel item = new();
            item.BookingId = row.b.BookingId;
            item.RoomId = row.r.RoomId;
            item.Zone = row.r.Zone ?? "";
            item.CustomerName = string.IsNullOrEmpty(row.u.CompanyName) ? row.u.Fullname : row.u.CompanyName;
            item.Period = row.b.StartDate.ToString("d MMM yyyy", _th) + " – " + row.b.EndDate.ToString("d MMM yyyy", _th);
            item.EndDateText = row.b.EndDate.ToString("d MMM yyyy", _th);
            item.IsEnded = row.b.EndDate < today;
            item.Deposit = row.b.Deposit;
            list.Add(item);
        }
        return list;
    }

    // หน้ารวม: การจองที่คืนเงินประกันได้ + ประวัติการคืนเงิน
    public IActionResult Refunds()
    {
        if (!IsAccounting)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        var history = (from rf in _db.Refunds
                       join p in _db.Payments on rf.PaymentId equals p.PaymentId
                       join b in _db.Bookings on p.BookingId equals b.BookingId
                       join u in _db.Users on b.UserId equals u.UserId
                       join s in _db.Users on rf.ProcessedBy equals s.UserId
                       orderby rf.RefundDate descending
                       select new { rf, b, u, s }).ToList();

        List<RefundHistoryViewModel> list = new();
        foreach (var row in history)
        {
            RefundHistoryViewModel item = new();
            item.RefundId = row.rf.RefundId;
            item.BookingId = row.b.BookingId;
            item.RoomId = row.b.RoomId;
            item.CustomerName = string.IsNullOrEmpty(row.u.CompanyName) ? row.u.Fullname : row.u.CompanyName;
            item.Amount = row.rf.RefundAmount;
            item.Reason = row.rf.RefundReason;
            item.Method = row.rf.RefundMethod;
            item.DateText = row.rf.RefundDate?.ToString("d MMM yyyy", _th) ?? "-";
            item.StaffName = row.s.Fullname;
            list.Add(item);
        }
        ViewBag.History = list;

        return View(GetActiveBookings(null));
    }

    // หน้าคืนเงินประกัน (GET)
    [HttpGet]
    public IActionResult RefundDeposit(string id)
    {
        if (!IsAccounting)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        var booking = GetActiveBookings(id).FirstOrDefault();
        if (booking == null)
        {
            TempData["Error"] = "คืนเงินประกันได้เฉพาะการจองที่ใช้งานอยู่";
            return RedirectToAction("Refunds");
        }
        ViewBag.Booking = booking;

        RefundFormViewModel data = new();
        data.BookingId = booking.BookingId;
        data.Method = RefundMethods[0];
        return View(data);
    }

    // บันทึกการคืนเงินประกัน (POST): ลงตาราง refund, การจองสิ้นสุดสัญญา, ห้องกลับมาว่าง
    [HttpPost]
    public IActionResult RefundDeposit(RefundFormViewModel data)
    {
        if (!IsAccounting)
        {
            return RedirectToAction("Login", "Account");
        }
        SetUserBag();

        var info = GetActiveBookings(data.BookingId).FirstOrDefault();
        if (info == null)
        {
            TempData["Error"] = "คืนเงินประกันได้เฉพาะการจองที่ใช้งานอยู่";
            return RedirectToAction("Refunds");
        }
        ViewBag.Booking = info;

        var staff = GetCurrentStaff();
        if (staff == null)
        {
            ViewBag.Error = "บัญชีนี้ไม่มีในตาราง user กรุณาเข้าสู่ระบบด้วยบัญชีพนักงานบัญชีในฐานข้อมูล (เช่น acc01)";
            return View(data);
        }

        if (data.Deduction < 0 || data.Deduction > info.Deposit)
        {
            ViewBag.Error = "ยอดหักต้องอยู่ระหว่าง 0 ถึง ฿" + info.Deposit.ToString("N0");
            return View(data);
        }

        if (data.Deduction > 0 && string.IsNullOrWhiteSpace(data.DeductionReason))
        {
            ViewBag.Error = "กรุณาใส่เหตุผลที่หักเงินประกัน";
            return View(data);
        }

        if (!RefundMethods.Contains(data.Method))
        {
            ViewBag.Error = "กรุณาเลือกวิธีคืนเงิน";
            return View(data);
        }

        // การชำระที่อนุมัติล่าสุดของการจองนี้ (ตาราง refund ผูกกับ PaymentId)
        var payment = (from p in _db.Payments
                       where p.BookingId == data.BookingId && p.PaymentStatus == "Approved"
                       orderby p.VerifiedAt descending
                       select p).FirstOrDefault();
        if (payment == null)
        {
            ViewBag.Error = "การจองนี้ยังไม่มีการชำระที่อนุมัติ จึงคืนเงินประกันไม่ได้";
            return View(data);
        }

        var reason = "คืนเงินประกันสิ้นสุดสัญญา";
        if (data.Deduction > 0)
        {
            reason += ", หัก ฿" + data.Deduction.ToString("N0") + " " + data.DeductionReason!.Trim();
        }
        if (reason.Length > 255)
        {
            reason = reason.Substring(0, 255);
        }

        Refund refund = new();
        refund.RefundId = NewDocumentId("REF-" + DateTime.Now.Year + "-", (from rf in _db.Refunds select rf.RefundId).ToList());
        refund.PaymentId = payment.PaymentId;
        refund.RefundAmount = info.Deposit - data.Deduction;
        refund.RefundReason = reason;
        refund.RefundMethod = data.Method;
        refund.RefundStatus = "Completed";
        refund.RefundDate = DateTime.Now;
        refund.ProcessedBy = staff.UserId;
        _db.Refunds.Add(refund);

        // การจองสิ้นสุดสัญญา / ห้องกลับมาว่างให้จองต่อได้
        var booking = (from b in _db.Bookings where b.BookingId == data.BookingId select b).First();
        var room = (from r in _db.Rooms where r.RoomId == booking.RoomId select r).First();
        booking.BookingStatus = "Ended";
        room.Status = "Available";
        _db.SaveChanges();

        TempData["Success"] = "คืนเงินประกัน " + refund.RefundId + " ฿" + refund.RefundAmount.ToString("N0")
                              + " แล้ว การจอง " + booking.BookingId + " สิ้นสุดสัญญา ห้อง " + room.RoomId + " กลับมาว่าง";
        return RedirectToAction("Refunds");
    }
}
