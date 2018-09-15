using System;

namespace MarchingCubes.SceneGraph
{
    /// <summary>
    /// Interface for <see cref="SceneGraphEntity"/> which report progress during their Initialize call (this usually means they have long running initialize functions).
    /// </summary>
    public interface ISceneGraphEntityInitializeProgressReporter
    {
        /// <summary>
        /// The progress reporter which will be fired with values between 0-100.
        /// If this is implemented, <see cref="InitializeProgress"/> must be called at least once with a value of 100 (progress completed).
        /// </summary>
        event EventHandler<int> InitializeProgress;
    }
}