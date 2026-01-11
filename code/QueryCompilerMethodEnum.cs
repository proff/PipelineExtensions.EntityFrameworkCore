namespace PipelineExtensions.EntityFrameworkCore
{
    internal enum QueryCompilerMethodEnum
    {
        Execute,
        ExecuteEnumerable,
        ExecuteAsyncTask,
        ExecuteAsyncEnumerable,
        CreateCompiledQuery,
        CreateCompiledQueryEnumerable,
        CreateCompiledAsyncQueryTask,
        CreateCompiledAsyncQueryEnumerable,
        PrecompileQuery
    }
}
