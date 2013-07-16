using System.Windows;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for NewSegmentMappingDialog.xaml
	/// </summary>
	public partial class NewSegmentMappingDialog
	{
		public NewSegmentMappingDialog()
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
