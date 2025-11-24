using System;

namespace SmartHub.Domain.Common
{
  public abstract class AuditableEntity : BaseEntity
  {
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
  }
}

/* This allows automatic tracking of :
1. Created Date
2. Updated Date
3. Who created/updated the record
*/