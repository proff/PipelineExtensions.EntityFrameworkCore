using Microsoft.EntityFrameworkCore;
using PipelineExtensions.EntityFrameworkCore.Tests;

namespace PipelineExtensions.EntityFrameworkCore.PerfTest
{
    public class TestContext1 : DbContext
    {
        public TestContext1(DbContextOptions<TestContext1> options) : base(options)
        {
        }

        public DbSet<Entity> Entities { get; set; }
    }
}