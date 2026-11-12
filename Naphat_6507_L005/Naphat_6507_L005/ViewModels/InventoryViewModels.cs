namespace Naphat_6507_L005.ViewModels;

// ViewModels ของหน้าฝ่ายคลังสินค้า (InventoryController) และแท็บสต็อกของลูกค้า
// 1 คลาสต่อ 1 งาน (ฟอร์มหรือสิ่งที่แสดงบนหน้า) รวมไว้ในไฟล์เดียวตามฝ่าย

// ===================== StockItemViewModel =====================
// แถวสินค้าในตาราง (รายการสต็อก / แจ้งเตือน / แท็บสต็อกของลูกค้า)
public class StockItemViewModel
{
    public int StockId { get; set; }
    public string BookingId { get; set; }
    public string RoomId { get; set; }
    public string CustomerName { get; set; }
    public string ItemName { get; set; }
    public string? Category { get; set; }
    public int Quantity { get; set; }
    public int MinQuantity { get; set; }
    public string? Unit { get; set; }
    public bool IsVerified { get; set; }            // ฝ่ายคลังตรวจรับแล้ว (คนแก้ล่าสุดไม่ใช่ลูกค้า)
    public bool IsLow => IsVerified && Quantity <= MinQuantity;   // ใกล้หมด: ตรวจรับแล้ว และเหลือเท่ากับหรือน้อยกว่าจำนวนขั้นต่ำ
    public string UpdatedText { get; set; }
    public string UpdatedByName { get; set; }
}

// ===================== StockFormViewModel =====================
// ฟอร์มเพิ่ม / แก้ไขสินค้า (Inventory/AddStock POST) StockId = 0 คือเพิ่มใหม่
public class StockFormViewModel
{
    public int StockId { get; set; }
    public string BookingId { get; set; }
    public string ItemName { get; set; }
    public string? Category { get; set; }
    public string? Unit { get; set; }
    public int MinQuantity { get; set; }
    public int Quantity { get; set; }
}

// ===================== StockAdjustViewModel =====================
// ฟอร์มรับเข้า / เบิกออก (Inventory/AdjustStock POST)
public class StockAdjustViewModel
{
    public int StockId { get; set; }
    public int Amount { get; set; }
    public string Direction { get; set; }      // in = รับเข้า, out = เบิกออก
    public string? ReturnTo { get; set; }      // กลับไปหน้าไหนหลังบันทึก (StockList / Alerts)
    public string? FilterBookingId { get; set; }
}

// ===================== ReceiveBookingViewModel =====================
// แถวในหน้า Inventory/Receive: การจองที่มีสินค้ารอตรวจรับ
public class ReceiveBookingViewModel
{
    public string BookingId { get; set; }
    public string RoomId { get; set; }
    public string CustomerName { get; set; }
    public string? Phone { get; set; }
    public string StartDateText { get; set; }
    public string BookingStatus { get; set; }   // Pending / Checking / Active
    public int ItemCount { get; set; }
    public bool CanReceive => BookingStatus == "Active";   // ตรวจรับได้เมื่อชำระเงินแล้ว (การจองใช้งานอยู่)
}

// ===================== ReceiveFormViewModel =====================
// ฟอร์มตรวจรับสินค้าของ 1 การจอง (Inventory/ReceiveItems POST)
public class ReceiveFormViewModel
{
    public string BookingId { get; set; }
    public List<ReceiveItemViewModel> Items { get; set; } = new();
}

public class ReceiveItemViewModel
{
    public int StockId { get; set; }
    public string? ItemName { get; set; }     // ใช้แสดงผล
    public string? Unit { get; set; }         // ใช้แสดงผล
    public int Declared { get; set; }         // จำนวนที่ลูกค้าแจ้ง (ใช้แสดงผล)
    public int Counted { get; set; }          // จำนวนที่นับได้จริง
    public int MinQuantity { get; set; }      // จำนวนขั้นต่ำ (ฝ่ายคลังตั้ง)
}
