using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace PipelineExtensions.EntityFrameworkCore.Tests
{
    public class Tests
    {
        private DbContextOptions<TestContext> _options;

        [OneTimeSetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<TestContext>().UseSqlite($"Filename=test.db").EnableServiceProviderCaching(true).Options;
            using var context = new TestContext(_options);
            context.Database.EnsureCreated();
        }

        private static ServiceProvider GetServiceProvider(Action<DbContextOptionsBuilder> options)
        {
            var c = new ServiceCollection();
            var sqlConnection = new SqliteConnection("Data Source=test.db");
            sqlConnection.Open();
            c.AddDbContext<TestContext>(o =>
            {
                o.UseSqlite(sqlConnection);
                options(o);
            }, ServiceLifetime.Scoped, ServiceLifetime.Singleton);
            var provider = c.BuildServiceProvider();
            return provider;
        }

        private static DbContextOptions<TestContext> GetOptions(Action<DbContextOptionsBuilder> options)
        {
            var builder = new DbContextOptionsBuilder<TestContext>();
            builder.UseSqlite("Data Source=test.db").ConfigureWarnings(a=>a.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));
            options(builder);
            var result = builder.Options;
            return result;
        }

        [Test]
        public void Test1()
        {
            var provider = GetServiceProvider(o => o.AddQueryCompiler<Compiler>());
            using var scope = provider.CreateScope();
            using var context = scope.ServiceProvider.GetService<TestContext>();
            var arr = context.Entities.ToArray();
            Assert.That(arr, Is.Empty);
            Assert.That(Compiler.ExecuteEnumerableCallCount, Is.EqualTo(1));
        }

        [Test]
        public void Test2()
        {
            var provider = GetServiceProvider(o => o.AddQueryCompiler<Compiler>());
            for (var i = 0; i < 100; i++)
            {
                using var scope = provider.CreateScope();
                using var context = scope.ServiceProvider.GetService<TestContext>();
                var count = context.Entities.Count();
                Assert.That(count, Is.Zero);
            }

            Assert.That(Compiler.ExecuteCallCount, Is.EqualTo(100));
        }

        [Test]
        public void Test2b()
        {
            var options = GetOptions(o => o.AddQueryCompiler<Compiler>());
            for (var i = 0; i < 100; i++)
            {
                using var context2 = new TestContext(options);
                context2.Database.EnsureCreated();
                var count = context2.Entities.Count();
                Assert.That(count, Is.Zero);
            }

            Assert.That(Compiler.ExecuteCallCount, Is.EqualTo(100));
        }

        [Test]
        public async Task Test3()
        {
            var provider = GetServiceProvider(o => o.AddQueryCompiler<Compiler>());
            using var scope = provider.CreateScope();
            using var context = scope.ServiceProvider.GetService<TestContext>();
            var count = await context.Entities.CountAsync();
            Assert.That(count, Is.Zero);
            Assert.That(Compiler.ExecuteAsyncCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Test4()
        {
            var provider = GetServiceProvider(o => o.AddQueryCompiler<Compiler>());
            using var scope = provider.CreateScope();
            using var context = scope.ServiceProvider.GetService<TestContext>();
            var count = await context.Entities.ToArrayAsync();
            Assert.That(count, Is.Empty);
            Assert.That(Compiler.ExecuteAsyncEnumerableCallCount, Is.EqualTo(1));
        }

        [Test]
        public void Test5()
        {
            var provider = GetServiceProvider(o => o.AddQueryCompiler<Compiler>());
            using var scope = provider.CreateScope();
            using var context = scope.ServiceProvider.GetService<TestContext>();
            var query = EF.CompileQuery((TestContext c, int value) => c.Entities.Where(a => a.Value == value));
            var arr = query(context, 100).ToArray();
            Assert.That(arr, Is.Empty);
            Assert.That(Compiler.CreateCompiledQueryEnumerableCallCount, Is.EqualTo(1));
        }

        [Test]
        public void Test6()
        {
            var provider = GetServiceProvider(o => o.AddQueryCompiler<Compiler>());
            using var scope = provider.CreateScope();
            using var context = scope.ServiceProvider.GetService<TestContext>();
            var query = EF.CompileQuery((TestContext c, int value) => c.Entities.Where(a => a.Value == value).Any());
            var result = query(context, 100);
            Assert.That(result, Is.False);
            Assert.That(Compiler.CreateCompiledQueryCallCount, Is.EqualTo(1));
        }

        [Test]
        public void Test7()
        {
            var provider = GetServiceProvider(o => o.AddQueryCompiler<Compiler>());
            using var scope = provider.CreateScope();
            using var context = scope.ServiceProvider.GetService<TestContext>();
            var query = EF.CompileQuery((TestContext c, int value) => c.Entities.Where(a => a.Value == value).Count());
            var result = query(context, 100);
            Assert.That(result, Is.Zero);
            Assert.That(Compiler.CreateCompiledQueryCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Test8()
        {
            var provider = GetServiceProvider(o => o.AddQueryCompiler<Compiler>());
            using var scope = provider.CreateScope();
            using var context = scope.ServiceProvider.GetService<TestContext>();
            var query = EF.CompileAsyncQuery((TestContext c, int value) => c.Entities.Where(a => a.Value == value));
            var list = new List<Entity>();
            await foreach (var entity in query(context, 100)) list.Add(entity);
            Assert.That(list, Is.Empty);
            Assert.That(Compiler.CreateCompiledAsyncQueryEnumerableCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Test9()
        {
            var provider = GetServiceProvider(o => o.AddQueryCompiler<Compiler>());
            using var scope = provider.CreateScope();
            using var context = scope.ServiceProvider.GetService<TestContext>();
            var query = EF.CompileAsyncQuery((TestContext c, int value) => c.Entities.Where(a => a.Value == value).Any());
            var result = await query(context, 100);
            Assert.That(result, Is.False);
            Assert.That(Compiler.CreateCompiledAsyncQueryTaskCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Test10()
        {
            var provider = GetServiceProvider(o => o.AddQueryCompiler<Compiler>());
            using var scope = provider.CreateScope();
            using var context = scope.ServiceProvider.GetService<TestContext>();
            var query = EF.CompileAsyncQuery((TestContext c, int value) => c.Entities.Where(a => a.Value == value).Count());
            var result = await query(context, 100);
            Assert.That(result, Is.Zero);
            Assert.That(Compiler.CreateCompiledAsyncQueryTaskCallCount, Is.EqualTo(1));
        }

        [TearDown]
        public void TearDown()
        {
            Compiler.Clear();
        }

        private class Compiler : IPipelineQueryCompiler
        {
            public static int ExecuteCallCount { get; set; }
            public static int ExecuteAsyncCallCount { get; set; }
            public static int ExecuteAsyncEnumerableCallCount { get; set; }
            public static int ExecuteEnumerableCallCount { get; set; }
            public static int CreateCompiledQueryCallCount { get; set; }
            public static int CreateCompiledQueryEnumerableCallCount { get; set; }
            public static int CreateCompiledAsyncQueryCallCount { get; set; }
            public static int CreateCompiledAsyncQueryTaskCallCount { get; set; }
            public static int CreateCompiledAsyncQueryEnumerableCallCount { get; set; }

            public T Execute<T>(Expression query, Func<Expression, T> next)
            {
                ExecuteCallCount++;
                return next(query);
            }

            public Func<QueryContext, T> CreateCompiledQuery<T>(Expression query, Func<Expression, Func<QueryContext, T>> next)
            {
                CreateCompiledQueryCallCount++;
                return next(query);
            }

            public IEnumerable<T> ExecuteEnumerable<T>(Expression query, Func<Expression, IEnumerable<T>> next)
            {
                ExecuteEnumerableCallCount++;
                return next(query);
            }

            public Task<T> ExecuteAsyncTask<T>(Expression query, CancellationToken cancellationToken, Func<Expression, CancellationToken, Task<T>> next)
            {
                ExecuteAsyncCallCount++;
                return next(query, cancellationToken);
            }

            public IAsyncEnumerable<T> ExecuteAsyncEnumerable<T>(Expression query, CancellationToken cancellationToken,
                Func<Expression, CancellationToken, IAsyncEnumerable<T>> next)
            {
                ExecuteAsyncEnumerableCallCount++;
                return next(query, cancellationToken);
            }

            public Func<QueryContext, IEnumerable<T>> CreateCompiledQueryEnumerable<T>(Expression query,
                Func<Expression, Func<QueryContext, IEnumerable<T>>> next)
            {
                CreateCompiledQueryEnumerableCallCount++;
                return next(query);
            }

            public Func<QueryContext, Task<T>> CreateCompiledAsyncQueryTask<T>(Expression query, Func<Expression, Func<QueryContext, Task<T>>> next)
            {
                CreateCompiledAsyncQueryTaskCallCount++;
                return next(query);
            }

            public Func<QueryContext, IAsyncEnumerable<T>> CreateCompiledAsyncQueryEnumerable<T>(Expression query,
                Func<Expression, Func<QueryContext, IAsyncEnumerable<T>>> next)
            {
                CreateCompiledAsyncQueryEnumerableCallCount++;
                return next(query);
            }

            public Func<QueryContext, T> CreateCompiledAsyncQuery<T>(Expression query, Func<Expression, Func<QueryContext, T>> next)
            {
                CreateCompiledAsyncQueryCallCount++;
                return next(query);
            }

            public static void Clear()
            {
                ExecuteCallCount = ExecuteAsyncCallCount = ExecuteAsyncEnumerableCallCount = ExecuteEnumerableCallCount = CreateCompiledQueryCallCount =
                    CreateCompiledAsyncQueryCallCount = CreateCompiledQueryEnumerableCallCount =
                        CreateCompiledAsyncQueryTaskCallCount = CreateCompiledAsyncQueryEnumerableCallCount = 0;
            }
        }
    }
}