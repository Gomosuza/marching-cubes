using System.IO;

namespace MarchingCubes.Data
{
    /// <summary>
    /// Base interface for any dataset that can be loaded.
    /// This is an alternative to the monogame interface, as the monogame content must always
    /// contain the loader type in the file header, thus requiring a custom compiler and not support
    /// existing file types.
    /// </summary>
    public interface IContentLoader<out T>
    {
        /// <summary>
        /// When called will load the specific file as the provided file type.
        /// </summary>
        /// <param name="filepath">The path from where to load.</param>
        /// <returns>The loaded file type</returns>
        /// <exception cref="FileLoadException">Thrown when the file is missing, or wrong format.</exception>
        T Load(string filepath);
    }
}
