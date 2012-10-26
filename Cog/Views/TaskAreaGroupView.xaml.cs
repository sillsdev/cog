using System.Windows;
using System.Windows.Documents;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for TaskAreaGroupView.xaml
	/// </summary>
	public partial class TaskAreaGroupView
	{
		public TaskAreaGroupView()
		{
			InitializeComponent();
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			var hyperlink = (Hyperlink) sender;
			var command = (CommandViewModel) hyperlink.DataContext;

			var vm = (TaskAreaGroupViewModel) DataContext;
			vm.CurrentCommand = command;
		}
	}
}
