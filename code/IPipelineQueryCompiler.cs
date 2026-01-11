using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace PipelineExtensions.EntityFrameworkCore
{
    public interface IPipelineQueryCompiler
    {
        T Execute<T>(Expression query, Func<Expression, T> next);
        Func<QueryContext, T> CreateCompiledQuery<T>(Expression query, Func<Expression, Func<QueryContext, T>> next);
        IEnumerable<T> ExecuteEnumerable<T>(Expression query, Func<Expression, IEnumerable<T>> next);
        Task<T> ExecuteAsyncTask<T>(Expression query, CancellationToken cancellationToken, Func<Expression, CancellationToken, Task<T>> next);


        IAsyncEnumerable<T> ExecuteAsyncEnumerable<T>(Expression query, CancellationToken cancellationToken,
            Func<Expression, CancellationToken, IAsyncEnumerable<T>> next);

        Func<QueryContext,IEnumerable<T>> CreateCompiledQueryEnumerable<T>(Expression query, Func<Expression,Func<QueryContext,IEnumerable<T>>> next);
        Func<QueryContext,Task<T>> CreateCompiledAsyncQueryTask<T>(Expression query, Func<Expression,Func<QueryContext,Task<T>>> next);
        Func<QueryContext, System.Collections.Generic.IAsyncEnumerable<T>> CreateCompiledAsyncQueryEnumerable<T>(Expression query, Func<Expression,Func<QueryContext, System.Collections.Generic.IAsyncEnumerable<T>>> next);
    }
}