using System.Windows;
using System.Windows.Documents;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for TaskAreaCommandGroupView.xaml
	/// </summary>
	public partial class TaskAreaCommandGroupView
	{
		public TaskAreaCommandGroupView()
		{
			InitializeComponent();
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			var hyperlink = (Hyperlink) sender;
			var command = (CommandViewModel) hyperlink.DataContext;

			var vm = (TaskAreaCommandGroupViewModel) DataContext;
			vm.CurrentCommand = command;
		}
	}
}
