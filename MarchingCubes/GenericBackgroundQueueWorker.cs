using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MarchingCubes
{
	/// <summary>
	/// A generic worker that works on generic <see cref="TData"/>.
	/// Data is added into a queue and processed in order. Upon finishing a work item, the function is called.
	/// </summary>
	/// <typeparam name="TData"></typeparam>
	public class GenericBackgroundWorker<TData>
	{
		private class WorkItem
		{
			private readonly Action<TData> _onComplete;

			public WorkItem(TData data, Action<TData> onComplete)
			{
				Item = data;
				_onComplete = onComplete;
			}

			public TData Item { get; }

			public void Finished()
			{
				_onComplete?.Invoke(Item);
			}
		}

		private readonly Action<TData> _work;
		private readonly BackgroundWorker _backgroundWorker;

		private readonly List<WorkItem> _workData;

		/// <summary>
		/// Creates a new worker instance that calls the provided function for each work item.
		/// </summary>
		public GenericBackgroundWorker(Action<TData> work)
		{
			if (work == null)
				throw new ArgumentNullException(nameof(work));

			_work = work;
			_backgroundWorker = new BackgroundWorker();
			_backgroundWorker.RunWorkerCompleted += WorkerComplete;
			_backgroundWorker.DoWork += StartWork;
			_workData = new List<WorkItem>();
		}

		private void StartWork(object sender, DoWorkEventArgs e)
		{
			var workItem = (WorkItem)e.Argument;
			_work(workItem.Item);
			workItem.Finished();
		}

		private void WorkerComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled || e.Error != null)
			{
				throw new NotImplementedException();
			}
			// just run next item right away
			StartWorkerIfIdle();
		}

		/// <summary>
		/// Queues an item for the operation. Upon completion the <see cref="action"/> is called.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="action"></param>
		public void QueueItem(TData data, Action<TData> action)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));
			lock (_workData)
			{
				_workData.Add(new WorkItem(data, action));
			}
			StartWorkerIfIdle();
		}

		/// <summary>
		/// Starts work on a new item if idle, otherwise exits without scheduling work.
		/// </summary>
		private void StartWorkerIfIdle()
		{
			lock (_backgroundWorker)
			{
				if (_backgroundWorker.IsBusy)
					return; // worker is busy and will call this function upon completion of current work again

				// worker is idle
				lock (_workData)
				{
					if (_workData.Count > 0)
					{
						var work = _workData[0];
						_workData.RemoveAt(0);
						_backgroundWorker.RunWorkerAsync(work);
						if (!_backgroundWorker.IsBusy)
							throw new NotSupportedException("is is always true immediately after RunWorkerAsync or not?");
					}
				}
			}
		}
	}
}