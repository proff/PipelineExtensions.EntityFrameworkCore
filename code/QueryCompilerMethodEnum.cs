namespace PipelineExtensions.EntityFrameworkCore
{
    /// <summary>
    /// Specifies the method types that can be intercepted in the query compiler pipeline.
    /// </summary>
    internal enum QueryCompilerMethodEnum
    {
        /// <summary>Synchronous execution.</summary>
        Execute,
        /// <summary>Synchronous enumerable execution.</summary>
        ExecuteEnumerable,
        /// <summary>Asynchronous task execution.</summary>
        ExecuteAsyncTask,
        /// <summary>Asynchronous enumerable execution.</summary>
        ExecuteAsyncEnumerable,
        /// <summary>Compiled query creation.</summary>
        CreateCompiledQuery,
        /// <summary>Compiled enumerable query creation.</summary>
        CreateCompiledQueryEnumerable,
        /// <summary>Compiled asynchronous task query creation.</summary>
        CreateCompiledAsyncQueryTask,
        /// <summary>Compiled asynchronous enumerable query creation.</summary>
        CreateCompiledAsyncQueryEnumerable,
        /// <summary>Query pre-compilation.</summary>
        PrecompileQuery
    }
}
