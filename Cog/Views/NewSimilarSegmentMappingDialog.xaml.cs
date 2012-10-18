using System.Windows;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for NewSimilarSegmentMappingDialog.xaml
	/// </summary>
	public partial class NewSimilarSegmentMappingDialog
	{
		public NewSimilarSegmentMappingDialog()
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
