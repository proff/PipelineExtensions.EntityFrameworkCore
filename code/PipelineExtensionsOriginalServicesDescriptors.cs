using Microsoft.Extensions.DependencyInjection;

namespace PipelineExtensions.EntityFrameworkCore
{
    /// <summary>
    /// Stores the original service descriptors replaced by the pipeline extension.
    /// </summary>
    internal class PipelineExtensionsOriginalServicesDescriptors
    {
        /// <summary>
        /// Gets or sets the original <see cref="IQueryCompiler"/> service descriptor.
        /// </summary>
        internal ServiceDescriptor OriginalQueryCompiler { get; set; }
    }
}