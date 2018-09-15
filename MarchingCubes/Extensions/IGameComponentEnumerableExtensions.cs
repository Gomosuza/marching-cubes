using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace MarchingCubes.Extensions
{
    /// <summary>
    /// Extensions for the <see cref="IEnumerable{IGameComponent}"/> provided by the game class.
    /// </summary>
    public static class IGameComponentEnumerableExtensions
    {
        /// <summary>
        /// Returns the first instance of the given type from the current list (or null if none found).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="components"></param>
        /// <returns></returns>
        public static T Get<T>(this IEnumerable<IGameComponent> components) where T : IGameComponent
        {
            return components.OfType<T>().FirstOrDefault();
        }
    }
}