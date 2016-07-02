namespace MarchingCubes.Content
{
	/// <summary>
	/// Base interface for any dataset that can be loaded.
	/// This is an alternative to the monogame interface, as the monogame content must always
	/// contain the loader type in the file header, thus requiring a custom compiler and not support
	/// existing file types.
	/// </summary>
	public interface IContentLoader<out T>
	{
		T Load(string filepath);
	}
}
