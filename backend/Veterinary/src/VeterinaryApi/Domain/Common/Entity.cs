using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace VeterinaryApi.Domain.Common;

public class Entity : IEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedOnUtc { get;  set; }

    public Entity()
    {
        Id = Guid.NewGuid();
        CreatedOnUtc = DateTime.UtcNow;
    }
}
public interface IEntity
{
    public Guid Id { get; }
    public DateTime CreatedOnUtc { get; }
}
