namespace Naphat_6507_L005.ViewModels;

// ข้อมูลห้องที่ใช้แสดงในหน้า Search / RoomDetail / BookRoom
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
    public string Rating { get; set; }
    public string Img { get; set; }         // ลิงก์รูป (หน้า view เติม ?w= ขนาดเอง)
}
