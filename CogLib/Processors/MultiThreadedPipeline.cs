using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SIL.Cog.Processors
{
	public class MultiThreadedPipeline<T> : Pipeline<T>
	{
		private CancellationTokenSource _tokenSource;
		private bool _canceled;

		public MultiThreadedPipeline()
		{
		}

		public MultiThreadedPipeline(IEnumerable<IProcessor<T>> processors)
			: base(processors)
		{
		}

		public override void Process(IEnumerable<T> items)
		{
			T[] itemArray = items.ToArray();
			var countdownEvent = new CountdownEvent(itemArray.Length * Processors.Count);
			_canceled = false;
			_tokenSource = new CancellationTokenSource();
			var token = _tokenSource.Token;
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
					                    }, token);
			}

			Task.Factory.StartNew(() =>
				{
					int lastPcnt = 0;
					while (!countdownEvent.Wait(50))
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

					OnCompleted(new EventArgs());
				}, token);
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
