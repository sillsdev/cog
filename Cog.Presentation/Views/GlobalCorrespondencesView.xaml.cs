using System.Windows.Input;
using GraphSharp.Controls;
using SIL.Cog.Applications.ViewModels;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for GlobalCorrespondencesView.xaml
	/// </summary>
	public partial class GlobalCorrespondencesView
	{
		public GlobalCorrespondencesView()
		{
			InitializeComponent();
			BusyCursor.DisplayUntilIdle();
		}

		private void Edge_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			var vm = (GlobalCorrespondencesViewModel) DataContext;
			var edgeControl = (EdgeControl) sender;
			var corr = (GlobalCorrespondenceEdge) edgeControl.DataContext;
			vm.SelectedCorrespondence = vm.SelectedCorrespondence == corr ? null : corr;
		}
	}
}
