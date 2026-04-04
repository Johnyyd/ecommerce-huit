using System;
using System.Collections.Generic;

namespace ECommerce.Huit.Application.DTOs.Product
{
    public class CategoryDto
    {
        public CategoryDto()
        {
            Name = string.Empty;
            Slug = string.Empty;
            Children = new List<CategoryDto>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public List<CategoryDto> Children { get; set; }
    }
}
