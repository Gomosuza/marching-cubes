using System;
using System.Linq;

namespace MarchingCubes.Content
{
	/// <summary>
	/// Attribute that must be applied to all data classes that want to be loaded via a custom loader.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class ContentLoaderAttribute : Attribute
	{
		public Type LoaderType { get; }

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