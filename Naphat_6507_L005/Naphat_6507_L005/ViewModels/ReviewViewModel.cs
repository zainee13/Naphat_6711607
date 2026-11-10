namespace Naphat_6507_L005.ViewModels;

// รีวิวห้อง ใช้ทั้งแสดงผลและรับค่าจากฟอร์ม (Customer/AddReview POST)
public class ReviewViewModel
{
    public string RoomId { get; set; }     // "" = รีวิวตัวอย่าง แสดงทุกห้อง
    public string Name { get; set; }
    public int Stars { get; set; } = 5;
    public string Date { get; set; }
    public string Comment { get; set; }
}
