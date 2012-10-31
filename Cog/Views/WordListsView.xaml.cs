using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;
using SIL.Cog.Converters;
using SIL.Cog.ViewModels;

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
		}

		private void _wordListsGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
		{
			var presenter = (ContentPresenter) e.EditingElement;
			var textBox = (TextBox) presenter.ContentTemplate.FindName("textBox", presenter);
			textBox.Focus();
			textBox.SelectAll();
		}

		private void LoadColumns()
		{
			var vm = (WordListsViewModel) DataContext;
			if (vm == null)
				return;

			_wordListsGrid.Columns.Clear();
			foreach (Tuple<SenseViewModel, int> sense in vm.Senses.Select((s, i) => Tuple.Create(s, i)).OrderBy(t => t.Item1.Gloss))
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
				var binding = new Binding(string.Format("Senses[{0}].Words", sense.Item2)) {Converter = new WordsToInlinesConverter(), ConverterParameter = textDecorations};
				textBlockFactory.SetBinding(TextBlockBehaviors.InlinesListProperty, binding);
				textBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(3, 1, 3, 1));
				var cellTemplate = new DataTemplate {VisualTree = textBlockFactory};

				var textBoxFactory = new FrameworkElementFactory(typeof(TextBox));
				textBoxFactory.SetBinding(TextBox.TextProperty, new Binding(string.Format("Senses[{0}].StrRep", sense.Item2)));
				textBoxFactory.SetValue(BorderThicknessProperty, new Thickness(0));
				textBoxFactory.Name = "textBox";
				var cellEditTemplate = new DataTemplate {VisualTree = textBoxFactory};


				var column = new DataGridTemplateColumn
					{
						Header = sense.Item1.Gloss,
						CellTemplate = cellTemplate,
						CellEditingTemplate = cellEditTemplate,
						ClipboardContentBinding = new Binding(string.Format("Senses[{0}].StrRep", sense.Item2)),
						SortMemberPath = string.Format("Senses[{0}].StrRep", sense.Item2)
					};

				_wordListsGrid.Columns.Add(column);
			}
		}

		private void WordListsView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = e.NewValue as WordListsViewModel;
			if (vm != null)
			{
				LoadColumns();
				vm.PropertyChanged += ViewModel_PropertyChanged;
				vm.Senses.CollectionChanged += Senses_CollectionChanged;
				vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
			}
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (WordListsViewModel) sender;
			switch (e.PropertyName)
			{
				case "Senses":
					LoadColumns();
					vm.Senses.CollectionChanged += Senses_CollectionChanged;
					break;

				case "Varieties":
					_wordListsGrid.Items.Refresh();
					vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
					break;
			}
		}

		private void Senses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LoadColumns();
		}

		private void Varieties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_wordListsGrid.Items.Refresh();
		}
	}
}
