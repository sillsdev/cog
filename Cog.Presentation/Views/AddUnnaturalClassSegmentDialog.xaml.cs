using System.Windows;

namespace SIL.Cog.Presentation.Views
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
			if (this.Validate())
				DialogResult = true;
		}
	}
}
