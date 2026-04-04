using System;
using System.Collections.Generic;
using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Order()
        {
            Code = string.Empty;
            OrderType = OrderType.ONLINE;
            Discount = 0;
            ShippingFee = 0;
            TaxAmount = 0;
            PaymentMethod = string.Empty;
            PaymentStatus = PaymentStatus.PENDING;
            Status = OrderStatus.PENDING;
            ShippingAddress = string.Empty;
            Items = new List<OrderItem>();
            StatusHistories = new List<OrderStatusHistory>();
            VoucherUsages = new List<VoucherUsage>();
        }

        public string Code { get; set; }
        public int UserId { get; set; }
        public OrderType OrderType { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public OrderStatus Status { get; set; }
        public string ShippingAddress { get; set; } // JSON
        public string Note { get; set; }
        public string StaffNote { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<OrderItem> Items { get; set; }
        public virtual ICollection<OrderStatusHistory> StatusHistories { get; set; }
        public virtual ICollection<VoucherUsage> VoucherUsages { get; set; }
        public virtual Payment Payment { get; set; }
    }
}
