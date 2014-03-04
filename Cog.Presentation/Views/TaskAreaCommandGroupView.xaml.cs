using System.Windows;
using System.Windows.Documents;
using SIL.Cog.Application.ViewModels;

namespace SIL.Cog.Presentation.Views
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
			var command = (TaskAreaCommandViewModel) hyperlink.DataContext;

			var vm = (TaskAreaCommandGroupViewModel) DataContext;
			vm.SelectedCommand = command;
		}
	}
}
