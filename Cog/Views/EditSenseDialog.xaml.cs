using System.Windows;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for EditSenseDialog.xaml
	/// </summary>
	public partial class EditSenseDialog
	{
		public EditSenseDialog()
		{
			InitializeComponent();
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			if (ViewUtilities.IsValid(this))
				DialogResult = true;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_glossTextBox.Focus();
			_glossTextBox.SelectAll();
		}
	}
}
