using System.Windows;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for ExportSegmentFrequenciesDialog.xaml
	/// </summary>
	public partial class ExportSegmentFrequenciesDialog
	{
		public ExportSegmentFrequenciesDialog()
		{
			InitializeComponent();
		}

		private void exportButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
