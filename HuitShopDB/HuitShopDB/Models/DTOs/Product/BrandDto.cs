using System;

namespace HuitShopDB.Models.DTOs.Product
{
    public class BrandDto
    {
        public BrandDto()
        {
            Name = string.Empty;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Origin { get; set; }
        public string LogoUrl { get; set; }
    }
}

