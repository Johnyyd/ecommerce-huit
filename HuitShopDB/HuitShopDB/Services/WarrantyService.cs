using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HuitShopDB.Models;
using HuitShopDB.Models.DTOs.Warranty;
using HuitShopDB.Services.Interfaces;

namespace HuitShopDB.Services
{
    public class WarrantyService : IWarrantyService
    {
        private readonly HuitShopDBDataContext _context;

        public WarrantyService()
        {
            _context = new HuitShopDBDataContext();
        }

        public async Task<WarrantyDto> GetWarrantyBySerialAsync(string serialNumber)
        {
            var ps = _context.product_serials.FirstOrDefault(s => s.serial_number == serialNumber);
            if (ps == null) return null;

            return await Task.FromResult(MapToDto(ps));
        }

        public async Task<IEnumerable<WarrantyDto>> GetRecentWarrantiesAsync(int count)
        {
            var recentSerials = _context.product_serials
                .Where(s => s.status == "SOLD")
                .OrderByDescending(s => s.outbound_date)
                .Take(count)
                .ToList();

            var result = recentSerials.Select(s => MapToDto(s)).ToList();
            return await Task.FromResult(result);
        }

        private WarrantyDto MapToDto(product_serial s)
        {
            // Try to find the order associated with this serial
            var ois = _context.order_item_serials.FirstOrDefault(x => x.serial_number == s.serial_number);
            
            var dto = new WarrantyDto
            {
                Id = s.id,
                SerialNumber = s.serial_number,
                ProductId = s.product_variant.product_id,
                ProductName = s.product_variant.product.name,
                VariantName = s.product_variant.variant_name,
                OutboundDate = s.outbound_date,
                ExpireDate = s.warranty_expire_date,
                Notes = s.notes
            };

            if (ois != null && ois.order_item != null)
            {
                dto.OrderCode = ois.order_item.order.code;
                dto.CustomerName = ois.order_item.order.user.full_name;
            }

            // Calculate status
            if (s.status != "SOLD")
            {
                dto.Status = "NOT_SOLD";
                dto.DaysRemaining = 0;
            }
            else if (s.warranty_expire_date.HasValue)
            {
                if (s.warranty_expire_date.Value < DateTime.Now)
                {
                    dto.Status = "EXPIRED";
                    dto.DaysRemaining = 0;
                }
                else
                {
                    dto.Status = "ACTIVE";
                    dto.DaysRemaining = (s.warranty_expire_date.Value - DateTime.Now).Days;
                }
            }
            else
            {
                dto.Status = "UNKNOWN";
                dto.DaysRemaining = 0;
            }

            return dto;
        }
    }
}
