using System;
using System.Linq;

namespace MarchingCubes.Data
{
    /// <summary>
    /// Attribute that must be applied to all data classes that want to be loaded via a custom loader.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ContentLoaderAttribute : Attribute
    {
        /// <summary>
        /// The class type that is attached as a loader. Guaranteed to be of type <see cref="IContentLoader{T}"/>.
        /// </summary>
        public Type LoaderType { get; }

        /// <summary>
        /// Creates a new attribute with the provided type as the loader class.
        /// </summary>
        /// <param name="loaderType">A type that must implement <see cref="IContentLoader{T}"/> and that must be instanciable by its default constructor.</param>
        /// <exception cref="ArgumentException"></exception>
        public ContentLoaderAttribute(Type loaderType)
        {
            if (loaderType.IsAbstract || loaderType.IsInterface)
                throw new ArgumentException($"The provided type {loaderType} must be instanciable.");

            // check if the IContentLoader<T> interface is implemented
            if (!loaderType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IContentLoader<>)))
                throw new ArgumentException($"The provided type {loaderType} does not implement the IContentLoader interface");

            LoaderType = loaderType;
        }
    }
}