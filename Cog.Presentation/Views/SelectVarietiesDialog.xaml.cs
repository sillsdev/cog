using System.Windows;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for SelectVarietiesDialog.xaml
	/// </summary>
	public partial class SelectVarietiesDialog
	{
		public SelectVarietiesDialog()
		{
			InitializeComponent();
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			if (this.Validate())
				DialogResult = true;
		}
	}
}
