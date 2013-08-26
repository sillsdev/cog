using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SIL.Cog.Domain.Components
{
	public class MultiThreadedPipeline<T> : Pipeline<T>
	{
		private CancellationTokenSource _tokenSource;
		private bool _canceled;
		private Task _task;

		public MultiThreadedPipeline()
		{
		}

		public MultiThreadedPipeline(IEnumerable<IProcessor<T>> processors)
			: base(processors)
		{
		}

		public override void Process(IEnumerable<T> items)
		{
			_canceled = false;
			_tokenSource = new CancellationTokenSource();
			var token = _tokenSource.Token;

			_task = Task.Factory.StartNew(() =>
				{
					T[] itemArray = items.ToArray();
					var countdownEvent = new CountdownEvent(itemArray.Length * Processors.Count);

					foreach (T secondaryData in itemArray)
					{
						T sd = secondaryData;
						Task.Factory.StartNew(() =>
							{
								foreach (IProcessor<T> processor in Processors)
								{
									if (token.IsCancellationRequested)
										break;
									processor.Process(sd);
									countdownEvent.Signal();
								}
							}, token, TaskCreationOptions.AttachedToParent, TaskScheduler.Current);
					}

					int lastPcnt = 0;
					while (!countdownEvent.Wait(100))
					{
						if (token.IsCancellationRequested)
							break;

						int curPcnt = ((countdownEvent.InitialCount - countdownEvent.CurrentCount) * 100) / countdownEvent.InitialCount;
						if (curPcnt != lastPcnt)
						{
							OnProgressUpdated(new ProgressEventArgs(curPcnt));
							lastPcnt = curPcnt;
						}
					}

					OnProgressUpdated(new ProgressEventArgs(100));
					OnCompleted(new EventArgs());
				}, token);
		}

		public bool WaitForComplete(int timeout)
		{
			return _task.Wait(timeout);
		}

		public void WaitForComplete()
		{
			_task.Wait();
		}

		public void Cancel()
		{
			if (_canceled)
				return;

			_tokenSource.Cancel();
			_canceled = true;
		}
	}
}
