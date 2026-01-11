using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace PipelineExtensions.EntityFrameworkCore
{
    /// <summary>
    /// Provides extension methods for configuring Entity Framework Core to use pipeline-based query compilation.
    /// </summary>
    public static class EntityFrameworkConfigurationExtensions
    {
        /// <summary>
        /// Registers a query compiler of type <typeparamref name="T"/> in the query execution pipeline.
        /// </summary>
        /// <typeparam name="T">The type of the pipeline query compiler to add.</typeparam>
        /// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder"/> being configured.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static DbContextOptionsBuilder AddQueryCompiler<T>(this DbContextOptionsBuilder optionsBuilder) where T : IPipelineQueryCompiler
        {
            var options = GetOptions(optionsBuilder);
            if (options.QueryCompilers.Count == 0)
            {
                optionsBuilder.ReplaceService<IQueryCompiler, PipelineQueryCompiler>();
            }
            options.AddQueryCompiler<T>();
            return optionsBuilder;
        }

        /// <summary>
        /// Registers a query compiler of type <typeparamref name="T"/> in the query execution pipeline.
        /// </summary>
        /// <typeparam name="TOptions">The type of the <see cref="DbContext"/> being configured.</typeparam>
        /// <typeparam name="T">The type of the pipeline query compiler to add.</typeparam>
        /// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder{TOptions}"/> being configured.</param>
        /// <param name="queryCompiler">A factory to create the query compiler instance.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static DbContextOptionsBuilder<TOptions> AddQueryCompiler<TOptions, T>(this DbContextOptionsBuilder<TOptions> optionsBuilder, Func<T> queryCompiler) where TOptions : DbContext where T : IPipelineQueryCompiler
        {
            var options = GetOptions(optionsBuilder);
            if (options.QueryCompilers.Count == 0)
            {
                optionsBuilder.ReplaceService<IQueryCompiler, PipelineQueryCompiler>();
            }
            options.AddQueryCompiler<T>();
            return optionsBuilder;
        }

        private static PipelineExtensionsOptionsExtension GetOptions(this DbContextOptionsBuilder optionsBuilder)
        {
            var extension = optionsBuilder.Options.FindExtension<PipelineExtensionsOptionsExtension>();
            if (extension == null)
            {
                extension = new PipelineExtensionsOptionsExtension();

                if (TryGetReplacedService<IQueryCompiler>(optionsBuilder, out var type)) extension.PreviousReplacedQueryCompiler = type;

                ((IDbContextOptionsBuilderInfrastructure) optionsBuilder).AddOrUpdateExtension(extension);
            }

            return extension;
        }

        private static bool TryGetReplacedService<T>(DbContextOptionsBuilder optionsBuilder, out Type type)
        {
            type = null;
            return optionsBuilder.Options.FindExtension<CoreOptionsExtension>()?.ReplacedServices
#if EFCORE5_OR_GREATER
                ?.TryGetValue((typeof(T), null), out type) == true;
#else
                ?.TryGetValue(typeof(T), out type) == true;
#endif
        }
    }
}