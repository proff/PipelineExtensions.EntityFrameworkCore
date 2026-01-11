using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace PipelineExtensions.EntityFrameworkCore
{
    /// <summary>
    ///     Information/metadata for an <see cref="IDbContextOptionsExtension" />.
    /// </summary>
    internal class PipelineExtensionsExtensionInfo : DbContextOptionsExtensionInfo
    {
        /// <summary>
        ///     Creates a new <see cref="DbContextOptionsExtensionInfo" /> instance containing
        ///     info/metadata for the given extension.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
        ///     for more information and examples.
        /// </remarks>
        /// <param name="extension">The extension.</param>
        public PipelineExtensionsExtensionInfo(PipelineExtensionsOptionsExtension extension) : base(extension)
        {
        }

        private new PipelineExtensionsOptionsExtension Extension => (PipelineExtensionsOptionsExtension) base.Extension;

        /// <inheritdoc/>
        public override bool IsDatabaseProvider => false;

        /// <inheritdoc/>
        public override string LogFragment => string.Empty;

        /// <inheritdoc/>
#if EFCORE6_OR_GREATER
        public override int GetServiceProviderHashCode()
#else
        public override long GetServiceProviderHashCode()
#endif
        {
            var result = HashCode.Combine(Extension.PreviousReplacedQueryCompiler?.GetHashCode() ?? 0);
            return result;
        }

#if EFCORE6_OR_GREATER        
        /// <inheritdoc/>
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            return true;
        }
#endif

        /// <inheritdoc/>
        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
        }
    }
}