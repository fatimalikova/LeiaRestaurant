using Leia.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Domain.Entities
{
    public class Slider : BaseEntity
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Link { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
    
    
}
