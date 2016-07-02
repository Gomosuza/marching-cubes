using MarchingCubes.Content;
using Microsoft.Xna.Framework.Content;
using System;
using System.IO;
using System.Reflection;

namespace MarchingCubes.Extensions
{
	/// <summary>
	/// Class containing extensions to the content manager.
	/// </summary>
	public static class ContentManagerExtensions
	{
		/// <summary>
		/// Loads a file from disk. The provided filepath must be relative to the content directory and must contain the extension.
		/// The return type must specify the attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="manager"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static T LoadWithAttributeParser<T>(this ContentManager manager, string filePath)
		{
			var file = Path.GetFullPath(Path.Combine(manager.RootDirectory, filePath));

			if (!File.Exists(file))
				throw new FileNotFoundException("Missing file: " + file);

			var loader = GetContentLoader<T>();

			return loader.Load(file);
		}

		/// <summary>
		/// Helper to create an instance of the provided loader type.
		/// This checks the attribute <see cref="ContentLoaderAttribute"/> of the provided type T.
		/// And if found will create an instance of the referenced type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		private static IContentLoader<T> GetContentLoader<T>()
		{
			var attribute = typeof(T).GetCustomAttribute(typeof(ContentLoaderAttribute)) as ContentLoaderAttribute;
			if (attribute == null)
				throw new ArgumentException($"Type to load {typeof(T)} did not have correct {typeof(ContentLoaderAttribute)} applied");

			var impl = attribute.LoaderType;
			var instance = Activator.CreateInstance(impl);

			return (IContentLoader<T>)instance;
		}
	}
}