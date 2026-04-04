using System;
using System.Collections.Generic;
using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities
{
    public class User : BaseEntity
    {
        public User()
        {
            FullName = string.Empty;
            Email = string.Empty;
            PasswordHash = string.Empty;
            Role = UserRole.CUSTOMER;
            Status = UserStatus.ACTIVE;
            Addresses = new List<Address>();
            Carts = new List<Cart>();
            Orders = new List<Order>();
            Reviews = new List<Review>();
            SupportTickets = new List<SupportTicket>();
            Returns = new List<Return>();
            AuditLogs = new List<AuditLog>();
            OrderStatusHistories = new List<OrderStatusHistory>();
            VoucherUsages = new List<VoucherUsage>();
        }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime? LastLogin { get; set; }

        // Navigation properties
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<SupportTicket> SupportTickets { get; set; }
        public virtual ICollection<Return> Returns { get; set; }
        public virtual ICollection<AuditLog> AuditLogs { get; set; }
        public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; }
        public virtual ICollection<VoucherUsage> VoucherUsages { get; set; }
    }
}
