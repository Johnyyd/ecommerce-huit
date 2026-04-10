using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using HuitShopDB.Models.DTOs.Product;
using HuitShopDB.Models.DTOs.Voucher;

namespace HuitShopDB.Models.DTOs.Cart
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

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "VariantId phải lớn hơn 0")]
        [JsonProperty("variant_id")]
        public int VariantId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Số lượng phải lớn hơn 0 và tối đa 100")]
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }

    public class UpdateCartItemRequest
    {
        public UpdateCartItemRequest()
        {
        }

        [Required]
        [Range(1, 100, ErrorMessage = "Số lượng phải lớn hơn 0 và tối đa 100")]
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }
}

