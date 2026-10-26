using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Domain.Common
{
    public abstract class BaseEntity : AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
    }

    
}
