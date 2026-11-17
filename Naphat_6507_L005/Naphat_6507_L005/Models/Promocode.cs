using System;
using System.Collections.Generic;

namespace Naphat_6507_L005.Models;

// โค้ดส่วนลดที่ Admin สร้าง ลดค่าเช่าเดือนแรกตอนลูกค้าจอง (บันทึกโค้ดและยอดส่วนลดไว้ที่ booking.PromoCode / DiscountAmount)
public partial class Promocode
{
    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public string DiscountType { get; set; } = null!;   // Percent = ลดเป็น %, Amount = ลดเป็นบาท

    public decimal DiscountValue { get; set; }

    public int MinMonths { get; set; }                  // ใช้ได้เมื่อสัญญาอย่างน้อยกี่เดือน

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
