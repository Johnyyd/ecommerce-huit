using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ECommerce.Huit.Application.DTOs.Product;
using ECommerce.Huit.Application.DTOs.Voucher;

namespace ECommerce.Huit.Application.DTOs.Cart
{
    public class CartDto
    {
        public CartDto()
        {
            Items = new List<CartItemDto>();
        }

        public int Id { get; set; }
        public List<CartItemDto> Items { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public VoucherDto AppliedVoucher { get; set; }
    }

    public class CartItemDto
    {
        public CartItemDto()
        {
        }

        public int Id { get; set; }
        public ProductVariantDto Variant { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class AddCartItemRequest
    {
        public AddCartItemRequest()
        {
            Quantity = 1;
        }

        [JsonProperty("variant_id")]
        public int VariantId { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }

    public class UpdateCartItemRequest
    {
        public UpdateCartItemRequest()
        {
        }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }
}
