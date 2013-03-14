using System.Windows;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for AddUnnaturalClassSegmentDialog.xaml
	/// </summary>
	public partial class AddUnnaturalClassSegmentDialog
	{
		public AddUnnaturalClassSegmentDialog()
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
