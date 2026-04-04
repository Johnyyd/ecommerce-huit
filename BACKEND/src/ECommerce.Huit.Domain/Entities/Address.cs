using System;

namespace ECommerce.Huit.Domain.Entities
{
    public class Address : BaseEntity
    {
        public Address()
        {
            Label = string.Empty;
            ReceiverName = string.Empty;
            ReceiverPhone = string.Empty;
            Province = string.Empty;
            District = string.Empty;
            Ward = string.Empty;
            StreetAddress = string.Empty;
            IsDefault = false;
        }

        public int UserId { get; set; }
        public string Label { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string StreetAddress { get; set; }
        public bool IsDefault { get; set; }

        public virtual User User { get; set; }
    }
}
