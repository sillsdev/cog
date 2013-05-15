using System.Windows;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for EditNaturalClassDialog.xaml
	/// </summary>
	public partial class EditNaturalClassDialog
	{
		public EditNaturalClassDialog()
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
