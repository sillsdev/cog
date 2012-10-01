using System.Windows;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for NewAffixDialog.xaml
	/// </summary>
	public partial class NewAffixDialog
	{
		public NewAffixDialog()
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
