using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void TabItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var tabItem = sender as TabItem;
			if (tabItem != null && tabItem.DataContext is MasterViewModel)
			{
				tabItem.SetValue(FrameworkElementBehaviors.IsContextMenuOpenProperty, true);
				ContextMenu menu = tabItem.ContextMenu;
				menu.Closed += menu_Closed;
				menu.PlacementTarget = tabItem;
				menu.IsOpen = true;
			}
		}

		private void menu_Closed(object sender, RoutedEventArgs e)
		{
			var menu = (ContextMenu) sender;
			menu.PlacementTarget.SetValue(FrameworkElementBehaviors.IsContextMenuOpenProperty, false);
			menu.Closed -= menu_Closed;
		}
	}
}
