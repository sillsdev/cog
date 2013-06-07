namespace SIL.Cog.ViewModels
{
	public class TaskAreaIntegerViewModel : CogViewModelBase
	{
		private int _value; 

		public TaskAreaIntegerViewModel(string displayName)
			: base(displayName)
		{
		}

		public int Value
		{
			get { return _value; }
			set { Set(() => Value, ref _value, value); }
		}
	}
}
