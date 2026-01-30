using Microsoft.EntityFrameworkCore;
using VeterinaryApi.Common.Abstracions;
using VeterinaryApi.Domain.Sensors;
using VeterinaryApi.Domain.Users;
using VeterinaryApi.Infrastructure.Persistence.Configurations.Users;

namespace VeterinaryApi.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
    ,IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserSession> UserSessions { get; set; } = null!;
    public DbSet<Esp32Readingcs> Esp32Readings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
    }

}
