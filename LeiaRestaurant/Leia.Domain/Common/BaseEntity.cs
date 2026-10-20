using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Domain.Common
{
    abstract class BaseEntity : AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
    }

    abstract class AuditableEntity 
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
