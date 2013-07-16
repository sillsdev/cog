namespace SIL.Cog.Domain.Components
{
	public abstract class ProcessorBase<T> : IProcessor<T>
	{
		private readonly CogProject _project;

		protected ProcessorBase(CogProject project)
		{
			_project = project;
		}

		protected CogProject Project
		{
			get { return _project; }
		}

		public abstract void Process(T data);
	}
}
