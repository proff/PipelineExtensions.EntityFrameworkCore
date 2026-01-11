using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace PipelineExtensions.EntityFrameworkCore
{
    public static class EntityFrameworkConfigurationExtensions
    {
        public static DbContextOptionsBuilder AddQueryCompiler<T>(this DbContextOptionsBuilder optionsBuilder) where T : IPipelineQueryCompiler
        {
            //todo register
            var options = GetOptions(optionsBuilder);
            if (options.QueryCompilers.Count == 0)
            {
                optionsBuilder.ReplaceService<IQueryCompiler, PipelineQueryCompiler>();
            }
            options.AddQueryCompiler<T>();
            return optionsBuilder;
        }

        /*public static DbContextOptionsBuilder AddQueryCompiler<TOptions, T>(this DbContextOptionsBuilder optionsBuilder, Type type)
            where T : IPipelineQueryCompiler
        {
            return AddQueryCompiler(optionsBuilder, () => Activator.CreateInstance(type));
        }*/
        
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
                /*if (TryGetReplacedService<IQueryTranslationPreprocessorFactory>(optionsBuilder, out var type))
                    extension.PreviousReplacedQueryTranslationPreprocessorFactory = type;

                if (TryGetReplacedService<IQueryTranslationPostprocessorFactory>(optionsBuilder, out type))
                    extension.PreviousReplacedQueryTranslationPostprocessorFactory = type;*/

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