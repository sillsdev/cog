using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using SIL.Cog.Behaviors;
using SIL.Cog.Converters;
using SIL.Cog.ViewModels;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Views;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for WordListsView.xaml
	/// </summary>
	public partial class WordListsView
	{
		public WordListsView()
		{
			InitializeComponent();
			WordListsGrid.ClipboardExporters.Clear();
			WordListsGrid.ClipboardExporters.Add(DataFormats.UnicodeText, new UnicodeCsvClipboardExporter {IncludeColumnHeaders = false, FormatSettings = {TextQualifier = '\0'}});
		}

		private void WordListsView_OnLoaded(object sender, RoutedEventArgs e)
		{
			var vm = (WordListsViewModel) DataContext;
			LoadColumns();
			LoadCollectionView();
			SizeRowSelectorPaneToFit();
			vm.PropertyChanged += ViewModel_PropertyChanged;
			((INotifyCollectionChanged) vm.Senses).CollectionChanged += Senses_CollectionChanged;
			((INotifyCollectionChanged) vm.Varieties).CollectionChanged += Varieties_CollectionChanged;
			SelectFirstCell();
		}

		private void LoadColumns()
		{
			var vm = (WordListsViewModel) DataContext;
			if (vm == null)
				return;

			WordListsGrid.Columns.Clear();
			for (int i = 0; i < vm.Senses.Count; i++)
			{
				//var geometry = Geometry.Parse("M 0,1 C 1,0 2,2 3,1");
				//var path = new Path {Stroke = Brushes.Red, StrokeThickness = 0.5, StrokeEndLineCap = PenLineCap.Square, StrokeStartLineCap = PenLineCap.Square, Data = geometry};
				//var brush = new VisualBrush
				//    {
				//        Viewbox = new Rect(0, 0, 3, 2),
				//        ViewboxUnits = BrushMappingMode.Absolute,
				//        Viewport = new Rect(0, 1.5, 6, 4),
				//        ViewportUnits = BrushMappingMode.Absolute,
				//        TileMode = TileMode.Tile,
				//        Visual = path
				//    };
				//var pen = new Pen(brush, 6);

				var pen = new Pen(new SolidColorBrush(Colors.Red), 2) {DashStyle = DashStyles.Dash};
				var textDecoration = new TextDecoration {Location = TextDecorationLocation.Underline, Pen = pen, PenOffset = 1};
				var textDecorations = new TextDecorationCollection {textDecoration};

				var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
				var textBinding = new Binding(string.Format("Senses[{0}].Words", i)) {Converter = new WordsToInlinesConverter(), ConverterParameter = textDecorations};
				textBlockFactory.SetBinding(TextBlockBehaviors.InlinesListProperty, textBinding);
				textBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(3, 1, 3, 1));
				textBlockFactory.SetValue(TextBlock.FontSizeProperty, 16.0);
				textBlockFactory.SetBinding(FrameworkElement.TagProperty, new Binding(string.Format("Senses[{0}].StrRep", i)));
				textBlockFactory.Name = "textBlock";
				var cellTemplate = new DataTemplate
					{
						VisualTree = textBlockFactory,
						Triggers =
							{
								new Trigger {SourceName = "textBlock", Property = TextBlockBehaviors.IsTextTrimmedProperty, Value = true, Setters =
									{
										new Setter(FrameworkElement.ToolTipProperty, new Binding(string.Format("Senses[{0}].StrRep", i)), "textBlock")
									}}
							}
					};

				var textBoxFactory = new FrameworkElementFactory(typeof(TextBox));
				textBoxFactory.SetBinding(TextBox.TextProperty, new Binding(string.Format("Senses[{0}].StrRep", i)));
				textBoxFactory.SetValue(BorderThicknessProperty, new Thickness(0));
				textBoxFactory.SetValue(Control.FontSizeProperty, 16.0);
				textBoxFactory.Name = "textBox";
				var cellEditTemplate = new DataTemplate {VisualTree = textBoxFactory};

				var c = new Column
					{
						FieldName = vm.Senses[i].Gloss,
						Title = vm.Senses[i].Gloss,
						DisplayMemberBindingInfo = new DataGridBindingInfo { Path = new PropertyPath(".") },
						CellContentTemplate = cellTemplate,
						CellEditor = new CellEditor { EditTemplate = cellEditTemplate },
						Width = new ColumnWidth(100)
					};

				WordListsGrid.Columns.Add(c);
			}
		}

		private void SizeRowSelectorPaneToFit()
		{
			var vm = (WordListsViewModel) DataContext;
			if (vm == null)
				return;

			var textBrush = (Brush) Application.Current.FindResource("HeaderTextBrush");
			double maxWidth = 0;
			foreach (WordListsVarietyViewModel variety in vm.Varieties)
			{
				var formattedText = new FormattedText(variety.Name, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
					new Typeface(WordListsGrid.FontFamily, WordListsGrid.FontStyle, WordListsGrid.FontWeight, WordListsGrid.FontStretch), WordListsGrid.FontSize, textBrush);
				if (formattedText.Width > maxWidth)
					maxWidth = formattedText.Width;
				variety.PropertyChanged -= variety_PropertyChanged;
				variety.PropertyChanged += variety_PropertyChanged;
			}

			var tableView = (TableView) WordListsGrid.View;
			tableView.RowSelectorPaneWidth = maxWidth + 16;
		}

		private void variety_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Name")
				SizeRowSelectorPaneToFit();
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (WordListsViewModel) sender;
			switch (e.PropertyName)
			{
				case "Senses":
					LoadColumns();
					((INotifyCollectionChanged) vm.Senses).CollectionChanged += Senses_CollectionChanged;
					break;

				case "Varieties":
					LoadCollectionView();
					SizeRowSelectorPaneToFit();
					((INotifyCollectionChanged) vm.Varieties).CollectionChanged += Varieties_CollectionChanged;
					WordListsGrid.Dispatcher.BeginInvoke(new Action(SelectFirstCell));
					break;
			}
		}

		private void SelectFirstCell()
		{
			if (WordListsGrid.Items.Count > 0)
				WordListsGrid.SelectedCellRanges.Add(new SelectionCellRange(0, 0));
			WordListsGrid.Focus();
		}

		private void LoadCollectionView()
		{
			var vm = (WordListsViewModel) DataContext;
			if (vm == null)
				return;

			var source = new DataGridCollectionView(vm.Varieties, typeof(WordListsVarietyViewModel), false, false);
			for (int i = 0; i < vm.Senses.Count; i++)
				source.ItemProperties.Add(new DataGridItemProperty(vm.Senses[i].Gloss, string.Format("Senses[{0}].StrRep", i), typeof(string)));
			WordListsGrid.ItemsSource = source;
		}

		private void Senses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LoadColumns();
		}

		private void Varieties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			WordListsGrid.Items.Refresh();
			SizeRowSelectorPaneToFit();
		}
	}
}
