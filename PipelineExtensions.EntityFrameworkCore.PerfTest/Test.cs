using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace PipelineExtensions.EntityFrameworkCore.PerfTest
{
    [MemoryDiagnoser]
    public class Test
    {
        private TestContext2 _context2;
        private TestContext1 _context1;
        private DbContextOptions<TestContext1> _options1;
        private DbContextOptions<TestContext2> _options2;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var guid = Guid.NewGuid();
            _options1 = new DbContextOptionsBuilder<TestContext1>().UseSqlite($"Filename=PipelineExtensions.{guid}.db").EnableServiceProviderCaching(true).Options;
            _options2 = new DbContextOptionsBuilder<TestContext2>().UseSqlite($"Filename=PipelineExtensions.{guid}.db").AddQueryCompiler(() => new Compiler())
                .EnableServiceProviderCaching(true).Options;
            _context1 = new TestContext1(_options1);
            _context2 =  new TestContext2(_options2);
            _context1.Database.EnsureCreated();
            _context1.Entities.ToArray();
            _context2.Entities.ToArray();
            _context1.Entities.Count();
            _context2.Entities.Count();
        }

        [Benchmark]
        public void WithoutPipeline_Count()
        {
            _context1.Entities.Count();
        }

        [Benchmark]
        public void WithPipeline_Count()
        {
            _context2.Entities.Count();
        }

        [Benchmark]
        public void WithoutPipeline_NewContext_Count()
        {
            var context = new TestContext1(_options1);
            context.Entities.Count();
        }

        [Benchmark]
        public void WithPipeline_NewContext_Count()
        {
            var context = new TestContext2(_options2);
            context.Entities.Count();
        }
        
        [Benchmark]
        public void WithoutPipeline_ToArray()
        {
            _context1.Entities.ToArray();
        }

        [Benchmark]
        public void WithPipeline_ToArray()
        {
            _context2.Entities.ToArray();
        }
        
        [Benchmark]
        public void WithoutPipeline_NewContext_ToArray()
        {
            var context = new TestContext1(_options1);
            context.Entities.ToArray();
        }

        [Benchmark]
        public void WithPipeline_NewContext_ToArray()
        {
            var context = new TestContext2(_options2);
            context.Entities.ToArray();
        }

        [Benchmark]
        public async Task WithoutPipeline_AnyAsync()
        {
            await _context1.Entities.AnyAsync();
        }

        [Benchmark]
        public async Task WithPipeline_AnyAsync()
        {
            await _context2.Entities.AnyAsync();
        }

        [Benchmark]
        public async Task WithoutPipeline_NewContext_AnyAsync()
        {
            var context = new TestContext1(_options1);
            await context.Entities.AnyAsync();
        }

        [Benchmark]
        public async Task WithPipeline_NewContext_AnyAsync()
        {
            var context = new TestContext2(_options2);
            await context.Entities.AnyAsync();
        }

        [Benchmark]
        public async Task WithoutPipeline_ToArrayAsync()
        {
            await _context1.Entities.ToArrayAsync();
        }

        [Benchmark]
        public async Task WithPipeline_ToArrayAsync()
        {
            await _context2.Entities.ToArrayAsync();
        }

        [Benchmark]
        public async Task WithoutPipeline_NewContext_ToArrayAsync()
        {
            var context = new TestContext1(_options1);
            await context.Entities.ToArrayAsync();
        }

        [Benchmark]
        public async Task WithPipeline_NewContext_ToArrayAsync()
        {
            var context = new TestContext2(_options2);
            await context.Entities.ToArrayAsync();
        }


        private class Compiler : IPipelineQueryCompiler
        {
            public T Execute<T>(Expression query, Func<Expression, T> next)
            {
                return next(query);
            }

            public Func<QueryContext, T> CreateCompiledQuery<T>(Expression query, Func<Expression, Func<QueryContext, T>> next)
            {
                throw new NotImplementedException();
            }

            public Func<QueryContext, T> CreateCompiledAsyncQuery<T>(Expression query, Func<Expression, Func<QueryContext, T>> next)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<T> ExecuteEnumerable<T>(Expression query, Func<Expression, IEnumerable<T>> next)
            {
                return next(query);
            }

            public Task<T> ExecuteAsyncTask<T>(Expression query, CancellationToken cancellationToken, Func<Expression, CancellationToken, Task<T>> next)
            {
                return next(query, cancellationToken);
            }

            public IAsyncEnumerable<T> ExecuteAsyncEnumerable<T>(Expression query, CancellationToken cancellationToken, Func<Expression, CancellationToken, IAsyncEnumerable<T>> next)
            {
                return next(query, cancellationToken);
            }

            public Func<QueryContext, IEnumerable<T>> CreateCompiledQueryEnumerable<T>(Expression query, Func<Expression, Func<QueryContext, IEnumerable<T>>> next)
            {
                throw new NotImplementedException();
            }

            public Func<QueryContext, Task<T>> CreateCompiledAsyncQueryTask<T>(Expression query, Func<Expression, Func<QueryContext, Task<T>>> next)
            {
                throw new NotImplementedException();
            }

            public Func<QueryContext, IAsyncEnumerable<T>> CreateCompiledAsyncQueryEnumerable<T>(Expression query, Func<Expression, Func<QueryContext, IAsyncEnumerable<T>>> next)
            {
                throw new NotImplementedException();
            }
        }
    }
}