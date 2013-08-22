using System.Windows;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for EditNaturalClassDialog.xaml
	/// </summary>
	public partial class EditNaturalClassDialog
	{
		public EditNaturalClassDialog()
		{
			InitializeComponent();
			SelectedFeaturesDataGrid.ClipboardExporters.Clear();
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			if (this.Validate())
				DialogResult = true;
		}
	}
}
