using System;
using System.Collections.Generic;

namespace Naphat_6507_L005.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Fullname { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string Role { get; set; } = null!;

    public string? CompanyName { get; set; }

    public string? TaxId { get; set; }

    public string? Address { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual ICollection<Stockitem> Stockitems { get; set; } = new List<Stockitem>();

    public virtual ICollection<Supportticket> SupportticketAssignedToNavigations { get; set; } = new List<Supportticket>();

    public virtual ICollection<Supportticket> SupportticketUsers { get; set; } = new List<Supportticket>();
}
