using System.Windows;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for RunStemmerDialog.xaml
	/// </summary>
	public partial class RunStemmerDialog
	{
		public RunStemmerDialog()
		{
			InitializeComponent();
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
