using System.Windows;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for ImportTextWordListsDialog.xaml
	/// </summary>
	public partial class ImportTextWordListsDialog
	{
		public ImportTextWordListsDialog()
		{
			InitializeComponent();
		}

		private void importButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
