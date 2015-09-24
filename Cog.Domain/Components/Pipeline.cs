using System;
using System.Collections.Generic;
using System.Linq;

namespace SIL.Cog.Domain.Components
{
	public class Pipeline<T>
	{
		public event EventHandler<ProgressEventArgs> ProgressUpdated;
		public event EventHandler<EventArgs> Completed;

		private readonly List<IProcessor<T>> _processors;

		public Pipeline()
			: this(null)
		{
		}

		public Pipeline(IEnumerable<IProcessor<T>> processors)
		{
			_processors = processors == null ? new List<IProcessor<T>>() : new List<IProcessor<T>>(processors);
		}

		public IList<IProcessor<T>> Processors
		{
			get { return _processors; }
		}

		public virtual void Process(IEnumerable<T> items)
		{
			T[] itemArray = items.ToArray();
			int tasksCount = itemArray.Length * _processors.Count;
			int tasksCompleted = 0;
			foreach (T item in itemArray)
			{
				foreach (IProcessor<T> processor in _processors)
				{
					processor.Process(item);
					tasksCompleted++;
					OnProgressUpdated(new ProgressEventArgs(tasksCompleted / tasksCount));
				}
			}

			OnCompleted(new EventArgs());
		}

		protected virtual void OnProgressUpdated(ProgressEventArgs pea)
		{
			EventHandler<ProgressEventArgs> handler = ProgressUpdated;
			if (handler != null)
				handler(this, pea);
		}

		protected virtual void OnCompleted(EventArgs e)
		{
			EventHandler<EventArgs> handler = Completed;
			if (handler != null)
				handler(this, e);
		}
	}
}
