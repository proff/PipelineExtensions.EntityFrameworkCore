using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace PipelineExtensions.EntityFrameworkCore
{
    /// <summary>
    /// Defines methods for intercepting and modifying the query compilation and execution pipeline in Entity Framework Core.
    /// </summary>
    public interface IPipelineQueryCompiler
    {
        /// <summary>
        /// Executes a query described by the specified expression tree.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="query">The expression tree representing the query.</param>
        /// <param name="next">The delegate to the next step in the pipeline.</param>
        /// <returns>The result of the query execution.</returns>
        T Execute<T>(Expression query, Func<Expression, T> next);

        /// <summary>
        /// Creates a delegate that executes a compiled query.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="query">The expression tree representing the query.</param>
        /// <param name="next">The delegate to the next step in the pipeline.</param>
        /// <returns>A delegate that executes the compiled query.</returns>
        Func<QueryContext, T> CreateCompiledQuery<T>(Expression query, Func<Expression, Func<QueryContext, T>> next);

        /// <summary>
        /// Executes a query that returns an enumerable result.
        /// </summary>
        /// <typeparam name="T">The type of the result elements.</typeparam>
        /// <param name="query">The expression tree representing the query.</param>
        /// <param name="next">The delegate to the next step in the pipeline.</param>
        /// <returns>An enumerable of the query results.</returns>
        IEnumerable<T> ExecuteEnumerable<T>(Expression query, Func<Expression, IEnumerable<T>> next);

        /// <summary>
        /// Executes a query asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="query">The expression tree representing the query.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <param name="next">The delegate to the next step in the pipeline.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the query result.</returns>
        Task<T> ExecuteAsyncTask<T>(Expression query, CancellationToken cancellationToken, Func<Expression, CancellationToken, Task<T>> next);

        /// <summary>
        /// Executes a query that returns an asynchronous enumerable result.
        /// </summary>
        /// <typeparam name="T">The type of the result elements.</typeparam>
        /// <param name="query">The expression tree representing the query.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <param name="next">The delegate to the next step in the pipeline.</param>
        /// <returns>An asynchronous enumerable of the query results.</returns>
        IAsyncEnumerable<T> ExecuteAsyncEnumerable<T>(Expression query, CancellationToken cancellationToken,
            Func<Expression, CancellationToken, IAsyncEnumerable<T>> next);

        /// <summary>
        /// Creates a delegate that executes a compiled query returning an enumerable result.
        /// </summary>
        /// <typeparam name="T">The type of the result elements.</typeparam>
        /// <param name="query">The expression tree representing the query.</param>
        /// <param name="next">The delegate to the next step in the pipeline.</param>
        /// <returns>A delegate that executes the compiled query.</returns>
        Func<QueryContext, IEnumerable<T>> CreateCompiledQueryEnumerable<T>(Expression query, Func<Expression, Func<QueryContext, IEnumerable<T>>> next);

        /// <summary>
        /// Creates a delegate that executes a compiled query asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="query">The expression tree representing the query.</param>
        /// <param name="next">The delegate to the next step in the pipeline.</param>
        /// <returns>A delegate that executes the compiled query asynchronously.</returns>
        Func<QueryContext, Task<T>> CreateCompiledAsyncQueryTask<T>(Expression query, Func<Expression, Func<QueryContext, Task<T>>> next);

        /// <summary>
        /// Creates a delegate that executes a compiled query returning an asynchronous enumerable result.
        /// </summary>
        /// <typeparam name="T">The type of the result elements.</typeparam>
        /// <param name="query">The expression tree representing the query.</param>
        /// <param name="next">The delegate to the next step in the pipeline.</param>
        /// <returns>A delegate that executes the compiled query.</returns>
        Func<QueryContext, System.Collections.Generic.IAsyncEnumerable<T>> CreateCompiledAsyncQueryEnumerable<T>(Expression query, Func<Expression, Func<QueryContext, System.Collections.Generic.IAsyncEnumerable<T>>> next);
    }
}