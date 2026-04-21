using System.Collections.Generic;
using System.Threading.Tasks;
using HuitShopDB.Models.DTOs.Warranty;

namespace HuitShopDB.Services.Interfaces
{
    public interface IWarrantyService
    {
        Task<WarrantyDto> GetWarrantyBySerialAsync(string serialNumber);
        Task<IEnumerable<WarrantyDto>> GetRecentWarrantiesAsync(int count);
        
        // New methods for warranty claims and management
        Task<bool> SubmitWarrantyClaimAsync(int userId, WarrantyClaimRequest request);
        Task<WarrantyClaimDto> GetWarrantyClaimAsync(int claimId);
        Task<IEnumerable<WarrantyClaimDto>> GetUserClaimsAsync(int userId);
        Task<IEnumerable<WarrantyClaimDto>> GetAllClaimsAsync(string status = null);
        Task<bool> UpdateWarrantyClaimAsync(int claimId, WarrantyClaimUpdateRequest request, int adminId);
        Task<bool> ApproveClaimAsync(int claimId, int adminId, string notes = null);
        Task<bool> RejectClaimAsync(int claimId, int adminId, string reason);
        Task<WarrantyAnalyticsDto> GetWarrantyAnalyticsAsync();
        Task<IEnumerable<WarrantyPolicyDto>> GetPoliciesAsync();
        Task<bool> CreatePolicyAsync(WarrantyPolicyDto policy);
    }
}
