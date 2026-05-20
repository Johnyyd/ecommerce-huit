using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HuitShopDB.Models.DTOs.Order
{
    public class OrderResponseDto
    {
        public OrderResponseDto()
        {
            Code = string.Empty;
            PaymentMethod = string.Empty;
            PaymentStatus = string.Empty;
            Status = string.Empty;
            RecipientName = string.Empty;
            RecipientPhone = string.Empty;
            FullAddress = string.Empty;
            UserName = string.Empty;
            UserEmail = string.Empty;
            Items = new List<OrderItemDto>();
            StatusHistory = new List<OrderStatusHistoryDto>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string Status { get; set; }
        public string ShippingAddressJson { get; set; }
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string FullAddress { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public List<OrderStatusHistoryDto> StatusHistory { get; set; }
    }

    public class OrderItemDto
    {
        public OrderItemDto()
        {
            ProductName = string.Empty;
            Sku = string.Empty;
            ThumbnailUrl = string.Empty;
            SerialNumbers = new List<string>();
        }

        public int Id { get; set; }
        public int? VariantId { get; set; }
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public string ThumbnailUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public List<string> SerialNumbers { get; set; }
    }

    public class OrderStatusHistoryDto
    {
        public OrderStatusHistoryDto()
        {
            Status = string.Empty;
        }

        public int Id { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateOrderRequest
    {
        public CreateOrderRequest()
        {
            ShippingAddressJson = string.Empty;
            PaymentMethod = string.Empty;
        }

        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
        public string ShippingAddressJson { get; set; }

        [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc")]
        public string PaymentMethod { get; set; }

        public string Note { get; set; }
    }
}
