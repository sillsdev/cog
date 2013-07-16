using System.Windows;
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
		private InputBinding _findBinding;

		public GlobalCorrespondencesView()
		{
			InitializeComponent();
			BusyCursor.DisplayUntilIdle();
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as GlobalCorrespondencesViewModel;
			if (vm == null)
				return;

			_findBinding = new InputBinding(vm.FindCommand, new KeyGesture(Key.F, ModifierKeys.Control));
		}

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var window = this.FindVisualAncestor<Window>();
			if (IsVisible)
				window.InputBindings.Add(_findBinding);
			else
				window.InputBindings.Remove(_findBinding);
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
