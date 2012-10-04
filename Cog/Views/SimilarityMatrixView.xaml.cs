using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using SIL.Cog.ViewModels;

namespace SIL.Cog.Views
{
	/// <summary>
	/// Interaction logic for SimilarityMatrixView.xaml
	/// </summary>
	public partial class SimilarityMatrixView
	{
		public SimilarityMatrixView()
		{
			InitializeComponent();
		}

		private void SimilarityMatrixView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var oldVM = e.OldValue as SimilarityMatrixViewModel;
			if (oldVM != null)
			{
				oldVM.PropertyChanged -= ViewModel_PropertyChanged;
				oldVM.PropertyChanging -= ViewModel_PropertyChanging;
				oldVM.Varieties.CollectionChanged -= Varieties_CollectionChanged;
			}
			var vm = e.NewValue as SimilarityMatrixViewModel;
			if (vm != null)
			{
				LoadColumns();
				vm.PropertyChanged += ViewModel_PropertyChanged;
				vm.PropertyChanging += ViewModel_PropertyChanging;
				vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
			}
		}

		private void Varieties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			LoadColumns();
		}

		private void LoadColumns()
		{
			var vm = (SimilarityMatrixViewModel) DataContext;
			if (vm == null)
				return;

			_simMatrixGrid.Columns.Clear();

			for (int i = 0; i < vm.Varieties.Count; i++)
			{
				var runFactory = new FrameworkElementFactory(typeof(Run));
				var textBinding = new Binding(string.Format("VarietyPairs[{0}].LexicalSimilarityScore", i)) {StringFormat = "{0:#0; ;0}", Mode = BindingMode.OneWay};
				runFactory.SetBinding(Run.TextProperty, textBinding);
				var hyperlinkFactory = new FrameworkElementFactory(typeof(Hyperlink));
				var commandBinding = new Binding(string.Format("VarietyPairs[{0}].SwitchToVarietyPairCommand", i));
				hyperlinkFactory.SetBinding(Hyperlink.CommandProperty, commandBinding);
				hyperlinkFactory.SetValue(FrameworkContentElement.StyleProperty, Application.Current.Resources["BlackHyperlinkStyle"]);
				hyperlinkFactory.AppendChild(runFactory);
				var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
				textBlockFactory.AppendChild(hyperlinkFactory);
				var backgroundBinding = new Binding(string.Format("VarietyPairs[{0}].LexicalSimilarityScore", i)) {Converter = new PercentageToColorConverter()};
				textBlockFactory.SetBinding(TextBlock.BackgroundProperty, backgroundBinding);
				//var foregroundBinding = new Binding("Background") {RelativeSource = new RelativeSource(RelativeSourceMode.Self), Converter = new BackgroundToForegroundConverter()};
				//textBlockFactory.SetBinding(TextBlock.ForegroundProperty, foregroundBinding);
				textBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(3, 1, 3, 1));
				var tooltipBinding = new MultiBinding();
				tooltipBinding.Bindings.Add(new Binding("Name"));
				tooltipBinding.Bindings.Add(new Binding(string.Format("VarietyPairs[{0}].OtherVarietyName", i)));
				tooltipBinding.Converter = new StringFormatConverter();
				tooltipBinding.ConverterParameter = "{0} <-> {1}";
				textBlockFactory.SetValue(ToolTipProperty, tooltipBinding);
				var cellTemplate = new DataTemplate {VisualTree = textBlockFactory};

				var column = new DataGridTemplateColumn {Header = vm.Varieties[i].Name, CellTemplate = cellTemplate};

				_simMatrixGrid.Columns.Add(column);
			}
		}

		private void ViewModel_PropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			var vm = (SimilarityMatrixViewModel) sender;
			switch (e.PropertyName)
			{
				case "Varieties":
					vm.Varieties.CollectionChanged -= Varieties_CollectionChanged;
					break;
			}
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var vm = (SimilarityMatrixViewModel) sender;
			switch (e.PropertyName)
			{
				case "Varieties":
					LoadColumns();
					vm.Varieties.CollectionChanged += Varieties_CollectionChanged;
					break;
			}
		}
	}
}
