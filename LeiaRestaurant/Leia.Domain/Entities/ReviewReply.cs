using Leia.Domain.Common;

namespace Leia.Domain.Entities;

public class ReviewReply : BaseEntity
{
    public Guid ReviewId { get; set; }
    public Review Review { get; set; } = null!;

    public Guid OwnerId { get; set; }
    public string Content { get; set; } = string.Empty;
}