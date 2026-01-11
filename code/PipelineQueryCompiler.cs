using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace PipelineExtensions.EntityFrameworkCore
{
    internal class PipelineQueryCompiler : IQueryCompiler
    {
        private static readonly ConcurrentDictionary<(Type, QueryCompilerMethodEnum), object> CallersCache =
            new ConcurrentDictionary<(Type, QueryCompilerMethodEnum), object>();

        private readonly IQueryCompiler _compiler;

        private readonly List<IPipelineQueryCompiler> _compilers;

        private readonly Dictionary<(Type, QueryCompilerMethodEnum), object> _pipelines =
            new Dictionary<(Type, QueryCompilerMethodEnum), object>();
#if !EFCORE5_OR_GREATER
        public PipelineQueryCompiler(IDbContextOptions options, ICurrentDbContext dbContext, QueryCompiler compiler)
#else
      public PipelineQueryCompiler(IDbContextOptions options, ICurrentDbContext dbContext, PipelineExtensionsOriginalServicesDescriptors descriptors)
#endif
        {
            var serviceProvider = ((IInfrastructure<IServiceProvider>) dbContext.Context).Instance;
            var pipelineOptions = options.FindExtension<PipelineExtensionsOptionsExtension>();
#if !EFCORE5_OR_GREATER
            _compiler = compiler;
#else
            _compiler = (IQueryCompiler) serviceProvider.GetService(descriptors.OriginalQueryCompiler.ImplementationType);
#endif
            _compilers = new List<IPipelineQueryCompiler>(pipelineOptions.QueryCompilers.Count);
            foreach (var queryCompiler in pipelineOptions.QueryCompilers) _compilers.Add((IPipelineQueryCompiler) serviceProvider.GetService(queryCompiler));
        }

        /// <inheritdoc />
        public T Execute<T>(Expression query)
        {
            var type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
#if NETSTANDARD2_1_OR_GREATER
                var func = (Func<PipelineQueryCompiler, Expression, IEnumerable>)CallersCache.GetOrAdd(
                    (type.GenericTypeArguments[0], QueryCompilerMethodEnum.ExecuteEnumerable), (_, t) => GetCallerExecuteEnumerable(t),
                    type.GenericTypeArguments[0]);
#else
                var func = (Func<PipelineQueryCompiler, Expression, IEnumerable>)CallersCache.GetOrAdd(
                    (type.GenericTypeArguments[0], QueryCompilerMethodEnum.ExecuteEnumerable), _ => GetCallerExecuteEnumerable(type.GenericTypeArguments[0]));
#endif
                var result = func(this, query);
                return result == null ? default : (T)result;
            }

            return GetExecutePipeline<T>()(query);
        }

        /// <inheritdoc />
        public T ExecuteAsync<T>(Expression query, CancellationToken cancellationToken)
        {
            var type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
#if NETSTANDARD2_1_OR_GREATER
                var func = (Func<PipelineQueryCompiler, Expression, CancellationToken, object>) CallersCache.GetOrAdd(
                    (type.GenericTypeArguments[0], QueryCompilerMethodEnum.ExecuteAsyncTask), (_, t) => GetCallerExecuteAsyncTask(t),
                    type.GenericTypeArguments[0]);
#else
                var func = (Func<PipelineQueryCompiler, Expression, CancellationToken, object>)CallersCache.GetOrAdd(
                    (type.GenericTypeArguments[0], QueryCompilerMethodEnum.ExecuteAsyncTask), _ => GetCallerExecuteAsyncTask(type.GenericTypeArguments[0]));
#endif
                var result = func(this, query, cancellationToken);
                return result == null ? default : (T) result;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
            {
#if NETSTANDARD2_1_OR_GREATER
                var func = (Func<PipelineQueryCompiler, Expression, CancellationToken, object>) CallersCache.GetOrAdd(
                    (type.GenericTypeArguments[0], QueryCompilerMethodEnum.ExecuteAsyncEnumerable), (_, t) => GetCallerExecuteAsyncEnumerable(t),
                    type.GenericTypeArguments[0]);
#else
                var func = (Func<PipelineQueryCompiler, Expression, CancellationToken, object>)CallersCache.GetOrAdd(
                    (type.GenericTypeArguments[0], QueryCompilerMethodEnum.ExecuteAsyncEnumerable), _ => GetCallerExecuteAsyncEnumerable(type.GenericTypeArguments[0]));
#endif
                var result = func(this, query, cancellationToken);
                return (T) result;
            }

            throw new InvalidOperationException($"Unknown type: {type.FullName}");
        }

        /// <inheritdoc />
        public Func<QueryContext, T> CreateCompiledQuery<T>(Expression query)
        {
            var type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
#if NETSTANDARD2_1_OR_GREATER
                var func = (Func<PipelineQueryCompiler, Expression, Func<QueryContext, T>>) CallersCache.GetOrAdd(
                    (type.GenericTypeArguments[0], QueryCompilerMethodEnum.CreateCompiledQueryEnumerable), (_, t) => GetCallerCreateCompiledQueryEnumerable(t),
                    type.GenericTypeArguments[0]);
#else
                var func = (Func<PipelineQueryCompiler, Expression, Func<QueryContext, T>>)CallersCache.GetOrAdd(
                    (type.GenericTypeArguments[0], QueryCompilerMethodEnum.CreateCompiledQueryEnumerable), _ => GetCallerCreateCompiledQueryEnumerable(type.GenericTypeArguments[0]));
#endif
                return func(this, query);
            }

            return GetCreateCompiledQueryPipeline<T>()(query);
        }

        /// <inheritdoc />
        public Func<QueryContext, T> CreateCompiledAsyncQuery<T>(Expression query)
        {
            var type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
#if NETSTANDARD2_1_OR_GREATER
                var func = (Func<PipelineQueryCompiler, Expression, Func<QueryContext, T>>) CallersCache.GetOrAdd(
                    (type.GenericTypeArguments[0], QueryCompilerMethodEnum.CreateCompiledAsyncQueryTask), (_, t) => GetCallerCreateCompiledAsyncQueryTask(t),
                    type.GenericTypeArguments[0]);
#else
                var func = (Func<PipelineQueryCompiler, Expression, Func<QueryContext, T>>)CallersCache.GetOrAdd(
                    (type.GenericTypeArguments[0], QueryCompilerMethodEnum.CreateCompiledAsyncQueryTask), _ => GetCallerCreateCompiledAsyncQueryTask(type.GenericTypeArguments[0]));
#endif
                return func(this, query);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
            {
#if NETSTANDARD2_1_OR_GREATER
                var func = (Func<PipelineQueryCompiler, Expression, Func<QueryContext, T>>) CallersCache.GetOrAdd(
                    (type.GenericTypeArguments[0], QueryCompilerMethodEnum.CreateCompiledAsyncQueryEnumerable), (_, t) => GetCallerCreateCompiledAsyncQueryEnumerable(t),
                    type.GenericTypeArguments[0]);
#else
                var func = (Func<PipelineQueryCompiler, Expression, Func<QueryContext, T>>)CallersCache.GetOrAdd(
                    (type.GenericTypeArguments[0], QueryCompilerMethodEnum.CreateCompiledAsyncQueryEnumerable), _ => GetCallerCreateCompiledAsyncQueryEnumerable(type.GenericTypeArguments[0]));
#endif
                return func(this, query);
            }

            throw new InvalidOperationException($"Unknown type: {type.FullName}");
        }

#if NET9_0_OR_GREATER
        [Experimental("EF9100")]
        public Expression<Func<QueryContext, TResult>> PrecompileQuery<TResult>(Expression query, bool async)
        {
            return _compiler.PrecompileQuery<TResult>(query, async);
        }
#endif

        private Func<Expression, T> GetExecutePipeline<T>()
        {
            if (_pipelines.TryGetValue((typeof(T), QueryCompilerMethodEnum.Execute), out var result))
            {
                return (Func<Expression, T>)result;
            }

            Func<Expression, T> pipeline = q => _compiler.Execute<T>(q);
            foreach (var compiler in _compilers)
            {
                var pipeline1 = pipeline;
                pipeline = q => compiler.Execute(q, pipeline1);
            }

            _pipelines[(typeof(T), QueryCompilerMethodEnum.Execute)] = pipeline;
            return pipeline;
        }

        private Func<Expression, IEnumerable<T>> GetExecuteEnumerablePipeline<T>()
        {
            if (_pipelines.TryGetValue((typeof(T), QueryCompilerMethodEnum.ExecuteEnumerable), out var result))
            {
                return (Func<Expression, IEnumerable<T>>)result;
            }

            Func<Expression, IEnumerable<T>> pipeline = q => _compiler.Execute<IEnumerable<T>>(q);
            foreach (var compiler in _compilers)
            {
                var pipeline1 = pipeline;
                pipeline = q => compiler.ExecuteEnumerable(q, pipeline1);
            }

            _pipelines[(typeof(T), QueryCompilerMethodEnum.ExecuteEnumerable)] = pipeline;
            return pipeline;
        }

        private Func<Expression, CancellationToken, Task<T>> GetExecuteAsyncTaskPipeline<T>()
        {
            if (_pipelines.TryGetValue((typeof(T), QueryCompilerMethodEnum.ExecuteAsyncTask), out var result))
            {
                return (Func<Expression, CancellationToken, Task<T>>)result;
            }

            Func<Expression, CancellationToken, Task<T>> pipeline = (q, t) => _compiler.ExecuteAsync<Task<T>>(q, t);
            foreach (var compiler in _compilers)
            {
                var pipeline1 = pipeline;
                pipeline = (q, t) => compiler.ExecuteAsyncTask(q, t, pipeline1);
            }

            _pipelines[(typeof(T), QueryCompilerMethodEnum.ExecuteAsyncTask)] = pipeline;
            return pipeline;
        }

        private Func<Expression, CancellationToken, IAsyncEnumerable<T>> GetExecuteAsyncEnumerablePipeline<T>()
        {
            if (_pipelines.TryGetValue((typeof(T), QueryCompilerMethodEnum.ExecuteAsyncEnumerable), out var result))
            {
                return (Func<Expression, CancellationToken, IAsyncEnumerable<T>>)result;
            }

            Func<Expression, CancellationToken, IAsyncEnumerable<T>> pipeline = (q, c) => _compiler.ExecuteAsync<IAsyncEnumerable<T>>(q, c);
            foreach (var compiler in _compilers)
            {
                var pipeline1 = pipeline;
                pipeline = (q, c) => compiler.ExecuteAsyncEnumerable(q, c, pipeline1);
            }

            _pipelines[(typeof(T), QueryCompilerMethodEnum.ExecuteAsyncEnumerable)] = pipeline;
            return pipeline;
        }

        private Func<Expression, Func<QueryContext, T>> GetCreateCompiledQueryPipeline<T>()
        {
            if (_pipelines.TryGetValue((typeof(T), QueryCompilerMethodEnum.CreateCompiledQuery), out var result))
            {
                return (Func<Expression, Func<QueryContext, T>>)result;
            }

            Func<Expression, Func<QueryContext, T>> pipeline = q => _compiler.CreateCompiledQuery<T>(q);
            foreach (var compiler in _compilers)
            {
                var pipeline1 = pipeline;
                pipeline = q => compiler.CreateCompiledQuery(q, pipeline1);
            }

            _pipelines[(typeof(T), QueryCompilerMethodEnum.CreateCompiledQuery)] = pipeline;
            return pipeline;
        }

        private Func<Expression, Func<QueryContext, IEnumerable<T>>> GetCreateCompiledQueryEnumerablePipeline<T>()
        {
            if (_pipelines.TryGetValue((typeof(T), QueryCompilerMethodEnum.CreateCompiledQueryEnumerable), out var result))
            {
                return (Func<Expression, Func<QueryContext, IEnumerable<T>>>)result;
            }

            Func<Expression, Func<QueryContext, IEnumerable<T>>> pipeline = q => _compiler.CreateCompiledQuery<IEnumerable<T>>(q);
            foreach (var compiler in _compilers)
            {
                var pipeline1 = pipeline;
                pipeline = q => compiler.CreateCompiledQueryEnumerable(q, pipeline1);
            }

            _pipelines[(typeof(T), QueryCompilerMethodEnum.CreateCompiledQueryEnumerable)] = pipeline;
            return pipeline;
        }

        private Func<Expression, Func<QueryContext, Task<T>>> GetCreateCompiledAsyncQueryTaskPipeline<T>()
        {
            if (_pipelines.TryGetValue((typeof(T), QueryCompilerMethodEnum.CreateCompiledAsyncQueryTask), out var result))
            {
                return (Func<Expression, Func<QueryContext, Task<T>>>)result;
            }

            Func<Expression, Func<QueryContext, Task<T>>> pipeline = q => _compiler.CreateCompiledAsyncQuery<Task<T>>(q);
            foreach (var compiler in _compilers)
            {
                var pipeline1 = pipeline;
                pipeline = q => compiler.CreateCompiledAsyncQueryTask(q, pipeline1);
            }

            _pipelines[(typeof(T), QueryCompilerMethodEnum.CreateCompiledAsyncQueryTask)] = pipeline;
            return pipeline;
        }

        private Func<Expression, Func<QueryContext, IAsyncEnumerable<T>>> GetCreateCompiledAsyncQueryEnumerablePipeline<T>()
        {
            if (_pipelines.TryGetValue((typeof(T), QueryCompilerMethodEnum.CreateCompiledAsyncQueryEnumerable), out var result))
            {
                return (Func<Expression, Func<QueryContext, IAsyncEnumerable<T>>>)result;
            }

            Func<Expression, Func<QueryContext, IAsyncEnumerable<T>>> pipeline = q => _compiler.CreateCompiledAsyncQuery<IAsyncEnumerable<T>>(q);
            foreach (var compiler in _compilers)
            {
                var pipeline1 = pipeline;
                pipeline = q => compiler.CreateCompiledAsyncQueryEnumerable(q, pipeline1);
            }

            _pipelines[(typeof(T), QueryCompilerMethodEnum.CreateCompiledAsyncQueryEnumerable)] = pipeline;
            return pipeline;
        }

        private static Func<PipelineQueryCompiler, Expression, IEnumerable> GetCallerExecuteEnumerable(Type type)
        {
            var p1 = Expression.Parameter(typeof(PipelineQueryCompiler));
            var p2 = Expression.Parameter(typeof(Expression));
            var call = Expression.Call(p1, nameof(ExecuteEnumerable), new[] {type}, p2);
            var lambda = Expression.Lambda<Func<PipelineQueryCompiler, Expression, IEnumerable>>(call, p1, p2);
            var func = lambda.Compile();
            return func;
        }

        private static Func<PipelineQueryCompiler, Expression, CancellationToken, object> GetCallerExecuteAsyncTask(Type type)
        {
            var p1 = Expression.Parameter(typeof(PipelineQueryCompiler));
            var p2 = Expression.Parameter(typeof(Expression));
            var p3 = Expression.Parameter(typeof(CancellationToken));
            var call = Expression.Call(p1, nameof(ExecuteAsyncTask), new[] {type}, p2, p3);
            var lambda = Expression.Lambda<Func<PipelineQueryCompiler, Expression, CancellationToken, object>>(call, p1, p2, p3);
            var func = lambda.Compile();
            return func;
        }

        private static Func<PipelineQueryCompiler, Expression, CancellationToken, object> GetCallerExecuteAsyncEnumerable(Type type)
        {
            var p1 = Expression.Parameter(typeof(PipelineQueryCompiler));
            var p2 = Expression.Parameter(typeof(Expression));
            var p3 = Expression.Parameter(typeof(CancellationToken));
            var call = Expression.Call(p1, nameof(ExecuteAsyncEnumerable), new[] {type}, p2, p3);
            var lambda = Expression.Lambda<Func<PipelineQueryCompiler, Expression, CancellationToken, object>>(call, p1, p2, p3);
            var func = lambda.Compile();
            return func;
        }

        private static Delegate GetCallerCreateCompiledQueryEnumerable(Type type)
        {
            var p1 = Expression.Parameter(typeof(PipelineQueryCompiler));
            var p2 = Expression.Parameter(typeof(Expression));
            var call = Expression.Call(p1, nameof(CreateCompiledQueryEnumerable), new[] {type}, p2);
            var lambda = Expression.Lambda(call, p1, p2);
            return lambda.Compile();
        }

        private static Delegate GetCallerCreateCompiledAsyncQueryTask(Type type)
        {
            var p1 = Expression.Parameter(typeof(PipelineQueryCompiler));
            var p2 = Expression.Parameter(typeof(Expression));
            var call = Expression.Call(p1, nameof(CreateCompiledAsyncQueryTask), new[] {type}, p2);
            var lambda = Expression.Lambda(call, p1, p2);
            return lambda.Compile();
        }

        private static Delegate GetCallerCreateCompiledAsyncQueryEnumerable(Type type)
        {
            var p1 = Expression.Parameter(typeof(PipelineQueryCompiler));
            var p2 = Expression.Parameter(typeof(Expression));
            var call = Expression.Call(p1, nameof(CreateCompiledAsyncQueryEnumerable), new[] {type}, p2);
            var lambda = Expression.Lambda(call, p1, p2);
            return lambda.Compile();
        }

        private IEnumerable<T> ExecuteEnumerable<T>(Expression query)
        {
            return GetExecuteEnumerablePipeline<T>()(query);
        }

        private Func<QueryContext, IEnumerable<T>> CreateCompiledQueryEnumerable<T>(Expression query)
        {
            return GetCreateCompiledQueryEnumerablePipeline<T>()(query);
        }

        private Func<QueryContext, IAsyncEnumerable<T>> CreateCompiledAsyncQueryEnumerable<T>(Expression query)
        {
            return GetCreateCompiledAsyncQueryEnumerablePipeline<T>()(query);
        }

        private Func<QueryContext, Task<T>> CreateCompiledAsyncQueryTask<T>(Expression query)
        {
            return GetCreateCompiledAsyncQueryTaskPipeline<T>()(query);
        }

        private Task<T> ExecuteAsyncTask<T>(Expression query, CancellationToken cancellationToken)
        {
            return GetExecuteAsyncTaskPipeline<T>()(query, cancellationToken);
        }

        private IAsyncEnumerable<T> ExecuteAsyncEnumerable<T>(Expression query, CancellationToken cancellationToken)
        {
            return GetExecuteAsyncEnumerablePipeline<T>()(query, cancellationToken);
        }
    }
}