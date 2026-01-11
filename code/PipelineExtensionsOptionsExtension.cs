using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace PipelineExtensions.EntityFrameworkCore
{
    /// <summary>
    /// Options for pipeline extension.
    /// </summary>
    internal class PipelineExtensionsOptionsExtension : IDbContextOptionsExtension
    {
        private PipelineExtensionsExtensionInfo _info;

        internal Type PreviousReplacedQueryCompiler { get; set; }

        internal List<Type> QueryCompilers { get; } = new List<Type>();

        /// <inheritdoc />
        public void ApplyServices(IServiceCollection services)
        {
            var descriptors = new PipelineExtensionsOriginalServicesDescriptors()
            {
                OriginalQueryCompiler = services.SingleOrDefault(a => a.ServiceType == typeof(IQueryCompiler)),
            };
            services.AddSingleton(c => descriptors);
            AddScoped(services, descriptors.OriginalQueryCompiler?.ImplementationType);
            AddScoped(services, PreviousReplacedQueryCompiler);

            foreach (var queryCompiler in QueryCompilers)
            {
                AddScoped(services, queryCompiler);
            }
#if !EFCORE5_OR_GREATER
            services.AddTransient(typeof(QueryCompiler));
#endif
        }

        private void AddScoped(IServiceCollection services, Type type)
        {
            if (type != null)
            {
                services.AddScoped(type);
            }
        }

        /// <inheritdoc />
        public void Validate(IDbContextOptions options)
        {
        }

        /// <inheritdoc />
        public DbContextOptionsExtensionInfo Info => _info ??= new PipelineExtensionsExtensionInfo(this);

        internal void AddQueryCompiler<T>() where T: IPipelineQueryCompiler
        {
            QueryCompilers.Add(typeof(T));
        }
    }
}