using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for TaskAreaItemsView.xaml
	/// </summary>
	public partial class TaskAreaItemsView
	{
		public TaskAreaItemsView()
		{
			InitializeComponent();
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			var hyperlink = (Hyperlink) sender;
			var vm = (TaskAreaItemsViewModel) hyperlink.DataContext;

			var contextMenu = new ContextMenu {PlacementTarget = (UIElement) hyperlink.Parent, Placement = PlacementMode.Right, HorizontalOffset = 14, DataContext = vm,
				Style = (Style) FindResource("TaskAreaContextMenuStyle")};

			bool prevWasGroup = false;
			foreach (TaskAreaViewModelBase item in vm.Items)
			{
				var group = item as TaskAreaCommandGroupViewModel;
				if (group != null)
				{
					if (contextMenu.Items.Count > 0)
						contextMenu.Items.Add(new Separator {Style = (Style) FindResource("TaskAreaSeparatorStyle")});

					foreach (TaskAreaCommandViewModel command in group.Commands)
					{
						var menuItem = new MenuItem {Header = command.DisplayName, Command = command.Command, DataContext = command, Tag = group, Style = (Style) FindResource("TaskAreaMenuItemStyle")};
						menuItem.Click += menuItem_Click;
						if (command == group.CurrentCommand)
						{
							var geometry = new EllipseGeometry(new Point(0, 0), 3, 3);
							var drawingBrush = new DrawingBrush(new GeometryDrawing {Brush = Brushes.Black, Geometry = geometry}) {Stretch = Stretch.None};
							menuItem.Icon = new Image {Source = new DrawingImage(drawingBrush.Drawing)};
						}
						contextMenu.Items.Add(menuItem);
					}
					prevWasGroup = true;
				}
				else
				{
					if (prevWasGroup)
						contextMenu.Items.Add(new Separator {Style = (Style) FindResource("TaskAreaSeparatorStyle")});

					prevWasGroup = false;
					var command = item as TaskAreaCommandViewModel;
					if (command != null)
					{
						var menuItem = new MenuItem {Header = command.DisplayName, Command = command.Command, DataContext = command, Style = (Style) FindResource("TaskAreaMenuItemStyle")};
						contextMenu.Items.Add(menuItem);
					}
					else
					{
						var booleanItem = item as TaskAreaBooleanViewModel;
						if (booleanItem != null)
						{
							var menuItem = new MenuItem {Header = booleanItem.DisplayName, DataContext = booleanItem, Style = (Style) FindResource("TaskAreaMenuItemStyle"), IsCheckable = true};
							menuItem.SetBinding(MenuItem.IsCheckedProperty, "Value");
							contextMenu.Items.Add(menuItem);
						}
					}
				}
			}
			contextMenu.IsOpen = true;
		}

		private void menuItem_Click(object sender, RoutedEventArgs e)
		{
			var menuItem = (MenuItem) sender;
			var command = (TaskAreaCommandViewModel) menuItem.DataContext;
			var group = (TaskAreaCommandGroupViewModel) menuItem.Tag;
			group.CurrentCommand = command;
		}
	}
}
