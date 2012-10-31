using System.Windows;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for ExportHierarchicalGraphDialog.xaml
	/// </summary>
	public partial class ExportHierarchicalGraphDialog
	{
		public ExportHierarchicalGraphDialog()
		{
			InitializeComponent();
		}

		private void exportButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
