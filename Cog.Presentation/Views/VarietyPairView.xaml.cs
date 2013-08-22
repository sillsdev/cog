using System.Collections.Specialized;
using System.ComponentModel;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for VarietyPairView.xaml
	/// </summary>
	public partial class VarietyPairView
	{
		public VarietyPairView()
		{
			InitializeComponent();
			CorrespondenceDataGrid.ClipboardExporters.Clear();
			((INotifyCollectionChanged) CorrespondenceDataGrid.Items.SortDescriptions).CollectionChanged += OnSortChanged;
		}

		private void OnSortChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var sortDescs = (SortDescriptionCollection) sender;
			if (e.Action == NotifyCollectionChangedAction.Reset && sortDescs.Count == 0)
				sortDescs.Add(new SortDescription("Lhs", ListSortDirection.Ascending));
		}
	}
}
