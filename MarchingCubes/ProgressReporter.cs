using System;

namespace MarchingCubes
{
	/// <summary>
	/// Generic progress reporter.
	/// </summary>
	public class ProgressReporter
	{
		/// <summary>
		/// Fired whenever the progress value changes.
		/// </summary>
		public event EventHandler<int> ProgressReported;

		private int _last;

		/// <summary>
		/// Call when the progress needs to be changed.
		/// </summary>
		/// <param name="progress"></param>
		public void SetProgress(int progress)
		{
			if (_last == progress)
				return;

			_last = progress;
			var r = ProgressReported;
			r?.Invoke(this, _last);
		}
	}
}