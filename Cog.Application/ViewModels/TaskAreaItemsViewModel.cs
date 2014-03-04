using SIL.Collections;

namespace SIL.Cog.Application.ViewModels
{
	public class TaskAreaItemsViewModel : TaskAreaViewModelBase
	{
		private readonly ReadOnlyList<TaskAreaViewModelBase> _items;

		public TaskAreaItemsViewModel(string displayName, params TaskAreaViewModelBase[] items)
			: base(displayName)
		{
			_items = new ReadOnlyList<TaskAreaViewModelBase>(items);
		}

		public ReadOnlyList<TaskAreaViewModelBase> Items
		{
			get { return _items; }
		}
	}
}
