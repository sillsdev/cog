using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
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

		private void MapView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = e.NewValue as GeographicalViewModel;
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
				case "Regions":
					ResetRegions(vm);
					break;
			}
		}

		private void ResetRegions(GeographicalViewModel vm)
		{
			foreach (RegionMarker rm in MapControl.Markers.OfType<RegionMarker>().ToArray())
				rm.Dispose();
			AddRegions(vm.Regions);
			vm.Regions.CollectionChanged += RegionsChanged;
			Dispatcher.BeginInvoke(new Action(GoHome));
		}

		private void GoHome()
		{
			if (ZoomAndCenterRegions())
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

		private void RegionsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddRegions(e.NewItems.Cast<VarietyRegionViewModel>());
					break;

				case NotifyCollectionChangedAction.Remove:
					RemoveRegions(e.OldItems.Cast<VarietyRegionViewModel>());
					break;

				case NotifyCollectionChangedAction.Replace:
					RemoveRegions(e.OldItems.Cast<VarietyRegionViewModel>());
					AddRegions(e.NewItems.Cast<VarietyRegionViewModel>());
					break;

				case NotifyCollectionChangedAction.Reset:
					MapControl.Markers.Clear();
					AddRegions((IEnumerable<VarietyRegionViewModel>) sender);
					break;
			}
		}

		private void AddRegions(IEnumerable<VarietyRegionViewModel> regions)
		{
			foreach (VarietyRegionViewModel region in regions)
			{
				var marker = new RegionMarker(region) {IsSelectable = _currentTool == Tool.Select};
				marker.Click += Region_Click;
				marker.RegeneratePolygonShape(MapControl);

				MapControl.Markers.Add(marker);
			}
		}

		private void Region_Click(object sender, EventArgs e)
		{
			ClosePopup();

			var rm = (RegionMarker) sender;
			Point p = CalculatePopupPosition(rm);
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
			else if (p.Y + 165 > MapControl.ActualHeight)
				yOffset = (int) (MapControl.ActualHeight - (p.Y + 165));

			if (xOffset != 0 || yOffset != 0)
				MapControl.Offset(xOffset, yOffset);
		}

		private void ClosePopup()
		{
			if (_popup != null)
			{
				MapControl.Markers.Remove(_popup);
				_popup = null;
			}
		}

		private Point CalculatePopupPosition(RegionMarker rm)
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

			return new Point(cx - 80, cy - 160);
		}

		private void RemoveRegions(IEnumerable<VarietyRegionViewModel> regions)
		{
			var regionSet = new HashSet<VarietyRegionViewModel>(regions);
			foreach (RegionMarker rm in MapControl.Markers.OfType<RegionMarker>().Where(rm => regionSet.Contains(rm.Region)).ToArray())
				rm.Dispose();
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
				Point p = CalculatePopupPosition((RegionMarker) _popup.Tag);
				_popup.Position = MapControl.FromLocalToLatLng((int) p.X, (int) p.Y);
			}
		}

		private bool ZoomAndCenterRegions()
		{
			double left = double.MaxValue;
			double top = double.MinValue;
			double right = double.MinValue;
			double bottom = double.MaxValue;

			foreach (PointLatLng p in MapControl.Markers.OfType<RegionMarker>().SelectMany(rm => rm.Polygon))
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
				var rect = RectLatLng.FromLTRB(left, top, right, bottom);
				return MapControl.SetZoomToFitRect(rect);
			}

			return false;
		}

		private void MapControl_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (!MapControl.IsDragging)
				ClosePopup();
		}
	}
}
