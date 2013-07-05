namespace SIL.Cog.ViewModels
{
	public class ViewChangedData
	{
		private readonly object _oldViewModel;
		private readonly object _newViewModel;

		public ViewChangedData(object oldViewModel, object newViewModel)
		{
			_oldViewModel = oldViewModel;
			_newViewModel = newViewModel;
		}

		public object OldViewModel
		{
			get { return _oldViewModel; }
		}

		public object NewViewModel
		{
			get { return _newViewModel; }
		}
	}
}
