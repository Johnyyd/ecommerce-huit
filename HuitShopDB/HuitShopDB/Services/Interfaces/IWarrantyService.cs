using System.Collections.Generic;
using HuitShopDB.Models.DTOs.Warranty;

namespace HuitShopDB.Services.Interfaces
{
    public interface IWarrantyService
    {
        WarrantyDto GetWarrantyBySerial(string serialNumber);
        IEnumerable<WarrantyDto> GetRecentWarranties(int count);
        
        // New methods for warranty claims and management
        bool SubmitWarrantyClaim(int userId, WarrantyClaimRequest request);
        WarrantyClaimDto GetWarrantyClaim(int claimId);
        IEnumerable<WarrantyClaimDto> GetUserClaims(int userId);
        IEnumerable<WarrantyClaimDto> GetAllClaims(string status = null);
        bool UpdateWarrantyClaim(int claimId, WarrantyClaimUpdateRequest request, int adminId);
        bool ApproveClaim(int claimId, int adminId, string notes = null);
        bool RejectClaim(int claimId, int adminId, string reason);
        WarrantyAnalyticsDto GetWarrantyAnalytics();
        IEnumerable<WarrantyPolicyDto> GetPolicies();
        bool CreatePolicy(WarrantyPolicyDto policy);
    }
}

