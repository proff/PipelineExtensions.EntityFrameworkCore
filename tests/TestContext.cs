using Microsoft.EntityFrameworkCore;

namespace PipelineExtensions.EntityFrameworkCore.Tests
{
    public class TestContext : DbContext
    {
        public TestContext(DbContextOptions<TestContext> options) : base(options)
        {
        }

        public DbSet<Entity> Entities { get; set; }
    }
}