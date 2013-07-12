using GalaSoft.MvvmLight.Messaging;

namespace SIL.Cog.ViewModels
{
	internal class ViewChangedMessage : MessageBase
	{
		private readonly object _oldViewModel;
		private readonly object _newViewModel;

		public ViewChangedMessage(object oldViewModel, object newViewModel)
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
