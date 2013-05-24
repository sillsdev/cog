using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for GeographicalView.xaml
	/// </summary>
	public partial class GeographicalView
	{
		private enum Tool
		{
			Select,
			Region
		}

		private Tool _currentTool;
		private IntermediateRegionMarker _intermediateRegionMarker;
		private GMapMarker _nextPointMarker;
		private GMapMarker _popup;

		public GeographicalView()
		{
			InitializeComponent();

			MapControl.DragButton = MouseButton.Left;
			MapControl.IgnoreMarkerOnMouseWheel = true;
		}

		private void GeographicalView_OnLoaded(object sender, RoutedEventArgs e)
		{
			var vm = DataContext as GeographicalViewModel;
			if (vm != null)
			{
				ResetRegions(vm);
				vm.PropertyChanged += ViewModel_PropertyChanged;
			}
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (GeographicalViewModel) sender;
			switch (e.PropertyName)
			{
				case "Varieties":
					ResetRegions(vm);
					break;
			}
		}

		private void ResetRegions(GeographicalViewModel vm)
		{
			foreach (RegionMarker rm in MapControl.Markers.OfType<RegionMarker>().ToArray())
				rm.Dispose();
			vm.Varieties.CollectionChanged += VarietiesChanged;
			foreach (GeographicalVarietyViewModel variety in vm.Varieties)
			{
				AddRegions(variety.Regions);
				GeographicalVarietyViewModel v = variety;
				variety.Regions.CollectionChanged += (sender, e) => RegionsChanged(v, e);
			}
			GoHome();
		}

		private void GoHome()
		{
			if (ZoomAndCenterRegions(MapControl.Markers.OfType<RegionMarker>()))
			{
				HomeButton.IsChecked = true;
			}
			else
			{
				MapControl.Zoom = MapControl.MinZoom;
				MapControl.Position = new PointLatLng(0, 0);
				FullButton.IsChecked = true;
			}
		}

		private void VarietiesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddVarieties(e.NewItems.Cast<GeographicalVarietyViewModel>());
					break;

				case NotifyCollectionChangedAction.Remove:
					RemoveVarieties(e.OldItems.Cast<GeographicalVarietyViewModel>());
					break;

				case NotifyCollectionChangedAction.Replace:
					RemoveVarieties(e.OldItems.Cast<GeographicalVarietyViewModel>());
					AddVarieties(e.NewItems.Cast<GeographicalVarietyViewModel>());
					break;

				case NotifyCollectionChangedAction.Reset:
					MapControl.Markers.Clear();
					AddVarieties((IEnumerable<GeographicalVarietyViewModel>) sender);
					break;
			}
		}

		private void AddVarieties(IEnumerable<GeographicalVarietyViewModel> varieties)
		{
			foreach (GeographicalVarietyViewModel variety in varieties)
			{
				AddRegions(variety.Regions);
				GeographicalVarietyViewModel v = variety;
				variety.Regions.CollectionChanged += (sender, e) => RegionsChanged(v, e);
			}
		}

		private void RemoveVarieties(IEnumerable<GeographicalVarietyViewModel> varieties)
		{
			RemoveRegions(varieties.SelectMany(v => v.Regions));
		}

		private void RegionsChanged(GeographicalVarietyViewModel variety, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddRegions(e.NewItems.Cast<GeographicalRegionViewModel>());
					break;

				case NotifyCollectionChangedAction.Remove:
					RemoveRegions(e.OldItems.Cast<GeographicalRegionViewModel>());
					break;

				case NotifyCollectionChangedAction.Replace:
					RemoveRegions(e.OldItems.Cast<GeographicalRegionViewModel>());
					AddRegions(e.NewItems.Cast<GeographicalRegionViewModel>());
					break;

				case NotifyCollectionChangedAction.Reset:
					foreach (RegionMarker rm in MapControl.Markers.OfType<RegionMarker>().Where(rm => rm.Region.Variety == variety).ToArray())
						rm.Dispose();
					AddRegions(variety.Regions);
					break;
			}

			if (variety.Regions.Count > 0)
			{
				int oldCount = variety.Regions.Count - (e.NewItems != null ? e.NewItems.Count : 0) + (e.OldItems != null ? e.OldItems.Count : 0);
				if (oldCount == 0)
					SetVarietyChecked(variety, true);
			}
			else
			{
				SetVarietyChecked(variety, false);
			}
		}

		private void SetVarietyChecked(GeographicalVarietyViewModel variety, bool check)
		{
			var varietyItem = (TreeViewItem) RegionsTreeView.ItemContainerGenerator.ContainerFromItem(variety);
			var checkBox = varietyItem.FindVisualChild<CheckBox>();
			checkBox.IsChecked = check;
		}

		private void AddRegions(IEnumerable<GeographicalRegionViewModel> regions)
		{
			foreach (GeographicalRegionViewModel region in regions)
			{
				var marker = new RegionMarker(region) {IsSelectable = _currentTool == Tool.Select};
				marker.Click += Region_Click;
				marker.RegeneratePolygonShape(MapControl);

				MapControl.Markers.Add(marker);
			}
		}

		private void RemoveRegions(IEnumerable<GeographicalRegionViewModel> regions)
		{
			var regionSet = new HashSet<GeographicalRegionViewModel>(regions);
			foreach (RegionMarker rm in MapControl.Markers.OfType<RegionMarker>().Where(rm => regionSet.Contains(rm.Region)).ToArray())
				rm.Dispose();
		}

		private void Region_Click(object sender, EventArgs e)
		{
			var rm = (RegionMarker) sender;
			SelectRegionMarker(rm);
		}

		private void SelectRegionMarker(RegionMarker rm)
		{
			ClosePopup();

			Point centerPoint = CalculateCenter(rm);
			Point p = CalculatePopupPosition(centerPoint);
			_popup = new GMapMarker(MapControl.FromLocalToLatLng((int) p.X, (int) p.Y)) {Tag = rm, Shape = new VarietyRegionView {DataContext = rm.Region}, ZIndex = 100};
			MapControl.Markers.Add(_popup);

			int xOffset = 0;
			if (p.X - 5 < 0)
			    xOffset = (int) -(p.X - 5);
			else if (p.X + 205 > MapControl.ActualWidth)
			    xOffset = (int) (MapControl.ActualWidth - (p.X + 205));

			int yOffset = 0;
			if (p.Y - 5 < 0)
			    yOffset = (int) -(p.Y - 5);
			else if (p.Y + 175 > MapControl.ActualHeight)
			    yOffset = (int) (MapControl.ActualHeight - (p.Y + 175));

			if (xOffset != 0 || yOffset != 0)
			    MapControl.Offset(xOffset, yOffset);

			SelectTreeRegion(rm.Region);
		}

		private void SelectTreeRegion(GeographicalRegionViewModel region)
		{
			SetSelectedTreeRegion(RegionsTreeView, region);
		}

		private bool SetSelectedTreeRegion(ItemsControl parent, GeographicalRegionViewModel region)
		{
			if (parent == null || region == null)
				return false;
 
			var childNode = parent.ItemContainerGenerator.ContainerFromItem(region) as TreeViewItem;
 
			if (childNode != null)
			{
				childNode.Focus();
				return childNode.IsSelected = true;
			}
 
			if (parent.Items.Count > 0)
			{
				foreach (object childItem in parent.Items)
				{
					var childControl = (ItemsControl) parent.ItemContainerGenerator.ContainerFromItem(childItem);
					if (SetSelectedTreeRegion(childControl, region))
						return true;
				}
			}
 
			return false;
		}

		private void ClosePopup()
		{
			if (_popup != null)
			{
				MapControl.Markers.Remove(_popup);
				_popup = null;
			}
		}

		private Point CalculateCenter(RegionMarker rm)
		{
			long areaSum = 0;
			long xSum = 0;
			long ySum = 0;
			for (int i = 0; i < rm.Polygon.Count; i++)
			{
				GPoint curPoint = MapControl.FromLatLngToLocal(rm.Polygon[i]);
				GPoint nextPoint = MapControl.FromLatLngToLocal(i == rm.Polygon.Count - 1 ? rm.Polygon[0] : rm.Polygon[i + 1]);
				long v = (curPoint.X * nextPoint.Y) - (nextPoint.X * curPoint.Y);
				areaSum += v;
				xSum += (curPoint.X + nextPoint.X) * v;
				ySum += (curPoint.Y + nextPoint.Y) * v;
			}

			double areaTerm = 1.0 / (6.0 * (areaSum / 2.0));

			double cx = areaTerm * xSum;
			double cy = areaTerm * ySum;
			return new Point(cx, cy);
		}

		private Point CalculatePopupPosition(Point center)
		{
			return new Point(center.X - 80, center.Y - 170);
		}

		private void SearchButton_OnClick(object sender, RoutedEventArgs e)
		{
			Search();
		}

		private void SearchTextBox_OnKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				Search();
		}

		private void Search()
		{
			MapControl.SetPositionByKeywords(SearchTextBox.Text);
			MapControl.Zoom = 11;
		}

		private void ShapeToolButton_OnChecked(object sender, RoutedEventArgs e)
		{
			if (MapControl == null)
				return;

			MapControl.Cursor = Cursors.Cross;
			MapControl.CanDragMap = false;
			foreach (RegionMarker rm in MapControl.Markers.OfType<RegionMarker>())
				rm.IsSelectable = false;
			_currentTool = Tool.Region;
		}

		private void SelectToolButton_OnChecked(object sender, RoutedEventArgs e)
		{
			if (MapControl == null)
				return;

			MapControl.Cursor = Cursors.Arrow;
			MapControl.CanDragMap = true;
			foreach (RegionMarker rm in MapControl.Markers.OfType<RegionMarker>())
				rm.IsSelectable = true;
			_currentTool = Tool.Select;
		}

		private void MapControl_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			MapControl.Focus();
			if (_currentTool == Tool.Region)
			{
				Point curPoint = e.GetPosition(MapControl);
				if (_intermediateRegionMarker == null)
				{
					PointLatLng latLng = MapControl.FromLocalToLatLng((int) curPoint.X, (int) curPoint.Y);
					_intermediateRegionMarker = new IntermediateRegionMarker(latLng);
					MapControl.Markers.Add(_intermediateRegionMarker);
					_nextPointMarker = new GMapMarker(latLng);
					MapControl.Markers.Add(_nextPointMarker);
				}

				if (_intermediateRegionMarker.AddPoint(curPoint))
				{
					AddCurrentRegion();
				}
				else
				{
					_nextPointMarker.Position = _intermediateRegionMarker.Route.Last();
					_nextPointMarker.Shape = null;
				}
			}
		}

		private void MapControl_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape && _currentTool == Tool.Region)
				ClearCurrentRegion();
		}

		private void MapControl_OnMouseMove(object sender, MouseEventArgs e)
		{
			if (_currentTool == Tool.Region && _intermediateRegionMarker != null)
				UpdateNextPointMarker(e.GetPosition(MapControl));
		}

		private void UpdateNextPointMarker(Point point)
		{
			GPoint p1 = MapControl.FromLatLngToLocal(_nextPointMarker.Position);
			Point p2 = point;

			_nextPointMarker.Shape = new Line
				{
					X1 = 0,
					Y1 = 0,
					X2 = p2.X - p1.X,
					Y2 = p2.Y - p1.Y,
					Stroke = Brushes.Gray,
					StrokeThickness = 3,
					Opacity = 0.5,
					IsHitTestVisible = false,
					StrokeDashArray = new DoubleCollection {2, 2}
				};
		}

		private void MapControl_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (_currentTool == Tool.Region && _intermediateRegionMarker.Route.Count > 1)
			{
				_nextPointMarker.Position = _intermediateRegionMarker.Route.Last();
				GPoint point = MapControl.FromLatLngToLocal(_intermediateRegionMarker.Position);
				UpdateNextPointMarker(new Point(point.X, point.Y));
				AddCurrentRegion();
			}
		}

		private void AddCurrentRegion()
		{
			var vm = (GeographicalViewModel) DataContext;
			vm.NewRegionCommand.Execute(_intermediateRegionMarker.Route.Select(p => Tuple.Create(p.Lat, p.Lng)));
			ClearCurrentRegion();
		}

		private void ClearCurrentRegion()
		{
			_intermediateRegionMarker.Dispose();
			_intermediateRegionMarker = null;
			_nextPointMarker.Clear();
			MapControl.Markers.Remove(_nextPointMarker);
			_nextPointMarker = null;
			SelectToolButton.IsChecked = true;
		}

		private void HomeButton_OnClick(object sender, RoutedEventArgs e)
		{
			GoHome();
		}

		private void FullButton_OnClick(object sender, RoutedEventArgs e)
		{
			MapControl.Zoom = MapControl.MinZoom;
			MapControl.Position = new PointLatLng(0, 0);
		}

		private void MapControl_OnOnPositionChanged(PointLatLng point)
		{
			FullButton.IsChecked = Math.Abs(MapControl.Zoom - MapControl.MinZoom) < double.Epsilon && MapControl.Position == new PointLatLng(0, 0);
			HomeButton.IsChecked = false;
		}

		private void MapControl_OnMapZoomChanged()
		{
			FullButton.IsChecked = Math.Abs(MapControl.Zoom - MapControl.MinZoom) < double.Epsilon && MapControl.Position == new PointLatLng(0, 0);
			HomeButton.IsChecked = false;
			if (_popup != null)
			{
				Point p = CalculatePopupPosition(CalculateCenter((RegionMarker) _popup.Tag));
				_popup.Position = MapControl.FromLocalToLatLng((int) p.X, (int) p.Y);
			}
		}

		private bool ZoomAndCenterRegions(IEnumerable<RegionMarker> regionMarkers)
		{
			RectLatLng rect;
			if (GetRectangle(regionMarkers, out rect))
				return MapControl.SetZoomToFitRect(rect);
			return false;
		}

		private bool GetRectangle(IEnumerable<RegionMarker> regionMarkers, out RectLatLng rect)
		{
			double left = double.MaxValue;
			double top = double.MinValue;
			double right = double.MinValue;
			double bottom = double.MaxValue;

			foreach (PointLatLng p in regionMarkers.SelectMany(rm => rm.Polygon))
			{
				// left
				if (p.Lng < left)
					left = p.Lng;

				// top
				if (p.Lat > top)
					top = p.Lat;

				// right
				if (p.Lng > right)
					right = p.Lng;

				// bottom
				if (p.Lat < bottom)
					bottom = p.Lat;
			}

			if (left != double.MaxValue && right != double.MinValue && top != double.MinValue && bottom != double.MaxValue)
			{
				rect = RectLatLng.FromLTRB(left, top, right, bottom);
				return true;
			}

			rect = new RectLatLng();
			return false;
		}

		private void MapControl_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (!MapControl.IsDragging)
				ClosePopup();
		}

		private void RegionsTreeView_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			object selectedItem = RegionsTreeView.SelectedItem;
			var region = selectedItem as GeographicalRegionViewModel;
			if (region != null)
			{
				RegionMarker marker = MapControl.Markers.OfType<RegionMarker>().First(rm => rm.Region == region);
				if (marker.Shape.IsVisible && (_popup == null || _popup.Tag != marker))
				{
					ZoomAndCenterRegions(new[] {marker});
					SelectRegionMarker(marker);
				}
			}
			else
			{
				var variety = selectedItem as GeographicalVarietyViewModel;
				if (variety != null)
				{
					ClosePopup();
					ZoomAndCenterRegions(MapControl.Markers.OfType<RegionMarker>().Where(rm => rm.Shape.IsVisible && rm.Region.Variety == variety));
				}
			}
		}

		private void CheckBox_OnClick(object sender, RoutedEventArgs e)
		{
			var checkBox = (CheckBox) sender;
			object vm = checkBox.DataContext;
			var variety = vm as GeographicalVarietyViewModel;
			if (variety != null)
			{
				if (!checkBox.IsChecked.HasValue)
					checkBox.IsChecked = false;

				foreach (RegionMarker marker in MapControl.Markers.OfType<RegionMarker>().Where(rm => rm.Region.Variety == variety))
					marker.Shape.Visibility = checkBox.IsChecked.Value ? Visibility.Visible : Visibility.Hidden;
				var item = checkBox.FindVisualAncestor<TreeViewItem>();
				foreach (GeographicalRegionViewModel child in item.Items)
				{
					var childItem = item.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;
					if (childItem != null)
					{
						var childCheckBox = childItem.FindVisualChild<CheckBox>();
						childCheckBox.IsChecked = checkBox.IsChecked;
						SetRegionVisibility(child, checkBox.IsChecked != null && (bool) checkBox.IsChecked);
					}
				}
			}
			else
			{
				var region = vm as GeographicalRegionViewModel;
				if (region != null)
				{
					Debug.Assert(checkBox.IsChecked.HasValue);
					SetRegionVisibility(region, (bool) checkBox.IsChecked);
					var item = checkBox.FindVisualAncestor<TreeViewItem>();
					var parentItem = item.FindVisualAncestor<TreeViewItem>();
					bool? check = null;
					foreach (GeographicalRegionViewModel child in parentItem.Items)
					{
						var childItem = parentItem.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;
						if (childItem != null)
						{
							var childCheckBox = childItem.FindVisualChild<CheckBox>();
							if (!check.HasValue)
							{
								check = childCheckBox.IsChecked;
							}
							else if (check != childCheckBox.IsChecked)
							{
								check = null;
								break;
							}
						}
					}
					var parentCheckBox = parentItem.FindVisualChild<CheckBox>();
					parentCheckBox.IsChecked = check;
				}
			}
		}

		private void SetRegionVisibility(GeographicalRegionViewModel region, bool isVisible)
		{
			var marker = MapControl.Markers.OfType<RegionMarker>().FirstOrDefault(rm => rm.Region == region);
			if (marker != null)
			{
				marker.Shape.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
				if (_popup != null && !isVisible && _popup.Tag == marker)
					ClosePopup();
			}
		}

		private void TreeViewItem_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			var item = (TreeViewItem) sender;
			item.IsSelected = true;
		}

		private void CheckBox_OnLoaded(object sender, RoutedEventArgs e)
		{
			var checkBox = (CheckBox) sender;
			var variety = (GeographicalVarietyViewModel) checkBox.DataContext;
			SetVarietyChecked(variety, variety.Regions.Count > 0);
		}
	}
}
