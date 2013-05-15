using System.Windows;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for EditAffixDialog.xaml
	/// </summary>
	public partial class EditAffixDialog
	{
		public EditAffixDialog()
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
