namespace Arkanis.Overlay.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

internal class DevelopmentUexDbContextFactory : IDesignTimeDbContextFactory<OverlayDbContext>
{
    public OverlayDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OverlayDbContext>();
        optionsBuilder.UseSqlite(args.Length == 1 ? args[0] : "Data Source=:memory:");

        return new OverlayDbContext(optionsBuilder.Options);
    }
}
