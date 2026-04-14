using System.Collections.Generic;
using System.Threading.Tasks;
using HuitShopDB.Models.DTOs.Warranty;

namespace HuitShopDB.Services.Interfaces
{
    public interface IWarrantyService
    {
        Task<WarrantyDto> GetWarrantyBySerialAsync(string serialNumber);
        Task<IEnumerable<WarrantyDto>> GetRecentWarrantiesAsync(int count);
    }
}
