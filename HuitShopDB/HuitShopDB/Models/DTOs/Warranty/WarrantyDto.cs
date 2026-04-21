using System;
using System.Collections.Generic;

namespace HuitShopDB.Models.DTOs.Warranty
{
    public class WarrantyDto
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string VariantName { get; set; }
        public string CustomerName { get; set; }
        public string OrderCode { get; set; }
        public DateTime? OutboundDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public string Status { get; set; }
        public int DaysRemaining { get; set; }
        public string Notes { get; set; }
        public int WarrantyMonths { get; set; }
    }

    public class WarrantyClaimRequest
    {
        public WarrantyClaimRequest()
        {
            PhotoUrls = new List<string>();
        }

        public string SerialNumber { get; set; }
        public string IssueDescription { get; set; }
        public string ClaimType { get; set; } // REPAIR, REPLACEMENT, REFUND
        public List<string> PhotoUrls { get; set; }
    }

    public class WarrantyClaimDto
    {
        public WarrantyClaimDto()
        {
            PhotoUrls = new List<string>();
        }

        public int Id { get; set; }
        public int WarrantyId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string SerialNumber { get; set; }
        public string IssueDescription { get; set; }
        public string ClaimType { get; set; }
        public string Status { get; set; } // SUBMITTED, APPROVED, REJECTED, COMPLETED
        public string AdminNotes { get; set; }
        public int? AssignedToStaffId { get; set; }
        public string AssignedToStaffName { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public List<string> PhotoUrls { get; set; }
    }

    public class WarrantyClaimUpdateRequest
    {
        public int ClaimId { get; set; }
        public string Status { get; set; }
        public string AdminNotes { get; set; }
        public int? AssignToStaffId { get; set; }
    }

    public class WarrantyPolicyDto
    {
        public WarrantyPolicyDto()
        {
            ExcludedDefects = new List<string>();
        }

        public int Id { get; set; }
        public string ProductType { get; set; }
        public int WarrantyMonths { get; set; }
        public string CoverageDescription { get; set; }
        public List<string> ExcludedDefects { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class WarrantyAnalyticsDto
    {
        public WarrantyAnalyticsDto()
        {
            Trends = new List<ClaimTrendDto>();
        }

        public int TotalClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public int PendingClaims { get; set; }
        public double ApprovalRate { get; set; }
        public int ActiveWarranties { get; set; }
        public int ExpiredWarranties { get; set; }
        public List<ClaimTrendDto> Trends { get; set; }
    }

    public class ClaimTrendDto
    {
        public DateTime Date { get; set; }
        public int SubmittedCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
    }
}
