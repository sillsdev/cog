using System.Windows;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for EditRegionDialog.xaml
	/// </summary>
	public partial class EditRegionDialog
	{
		public EditRegionDialog()
		{
			InitializeComponent();
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			if (ViewUtilities.IsValid(this))
				DialogResult = true;
		}
	}
}
