namespace SIL.Cog.ViewModels
{
	public class TaskAreaBooleanViewModel : TaskAreaViewModelBase
	{
		private bool _value;

		public TaskAreaBooleanViewModel(string displayName)
			: base(displayName)
		{
		}

		public bool Value
		{
			get { return _value; }
			set { Set(() => Value, ref _value, value); }
		}
	}
}
