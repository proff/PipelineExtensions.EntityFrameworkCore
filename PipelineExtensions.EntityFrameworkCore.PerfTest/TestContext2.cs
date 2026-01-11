using Microsoft.EntityFrameworkCore;
using PipelineExtensions.EntityFrameworkCore.Tests;

namespace PipelineExtensions.EntityFrameworkCore.PerfTest
{
    public class TestContext2 : DbContext
    {
        public TestContext2(DbContextOptions<TestContext2> options) : base(options)
        {
        }

        public DbSet<Entity> Entities { get; set; }
    }
}