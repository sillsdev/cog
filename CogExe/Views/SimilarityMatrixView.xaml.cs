using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using GalaSoft.MvvmLight.Threading;
using SIL.Cog.Converters;
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
			var vm = e.NewValue as SimilarityMatrixViewModel;
			if (vm != null)
			{
				LoadColumns();
				vm.PropertyChanged += ViewModel_PropertyChanged;
			}
		}

		private void LoadColumns()
		{
			var vm = (SimilarityMatrixViewModel) DataContext;
			if (vm == null)
				return;

			SimMatrixGrid.Columns.Clear();

			double width = 0.0;
			foreach (SimilarityMatrixVarietyViewModel variety in vm.Varieties)
			{
				var typeface = new Typeface(SimMatrixGrid.FontFamily, SimMatrixGrid.FontStyle, SimMatrixGrid.FontWeight, SimMatrixGrid.FontStretch);
				var text = new FormattedText(variety.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, SimMatrixGrid.FontSize, SimMatrixGrid.Foreground);
				width = Math.Max(text.Width, width);
			}

			for (int i = 0; i < vm.Varieties.Count; i++)
			{
				var runFactory = new FrameworkElementFactory(typeof(Run));
				var textBinding = new Binding(string.Format("VarietyPairs[{0}].SimilarityScore", i)) {StringFormat = "{0:#0; ;0}", Mode = BindingMode.OneWay};
				runFactory.SetBinding(Run.TextProperty, textBinding);
				var hyperlinkFactory = new FrameworkElementFactory(typeof(Hyperlink));
				var commandBinding = new Binding(string.Format("VarietyPairs[{0}].SwitchToVarietyPairCommand", i));
				hyperlinkFactory.SetBinding(Hyperlink.CommandProperty, commandBinding);
				hyperlinkFactory.SetValue(FrameworkContentElement.StyleProperty, Application.Current.Resources["BlackHyperlinkStyle"]);
				hyperlinkFactory.AppendChild(runFactory);
				var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
				textBlockFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
				textBlockFactory.AppendChild(hyperlinkFactory);
				var backgroundBinding = new Binding(string.Format("VarietyPairs[{0}].SimilarityScore", i)) {Converter = new PercentageToSpectrumColorConverter()};
				textBlockFactory.SetBinding(TextBlock.BackgroundProperty, backgroundBinding);
				textBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(3, 1, 3, 1));
				var tooltipBinding = new MultiBinding();
				tooltipBinding.Bindings.Add(new Binding("Name"));
				tooltipBinding.Bindings.Add(new Binding(string.Format("VarietyPairs[{0}].OtherVarietyName", i)));
				tooltipBinding.Converter = new StringFormatConverter();
				tooltipBinding.ConverterParameter = "{0} <-> {1}";
				textBlockFactory.SetValue(ToolTipProperty, tooltipBinding);
				var cellTemplate = new DataTemplate {VisualTree = textBlockFactory};

				textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
				textBlockFactory.SetValue(TextBlock.TextProperty, vm.Varieties[i].Name);
				textBlockFactory.SetValue(MarginProperty, new Thickness(0, 3, 0, 3));
				textBlockFactory.SetValue(LayoutTransformProperty, new RotateTransform(270.0));
				textBlockFactory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Bottom);
				var gridFactory = new FrameworkElementFactory(typeof(Grid));
				gridFactory.SetValue(HeightProperty, width + 6.0);
				gridFactory.AppendChild(textBlockFactory);
				var headerTemplate = new DataTemplate {VisualTree = gridFactory};

				var column = new DataGridTemplateColumn {HeaderTemplate = headerTemplate, CellTemplate = cellTemplate, Width = new DataGridLength(30)};

				SimMatrixGrid.Columns.Add(column);
			}
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Varieties":
					DispatcherHelper.CheckBeginInvokeOnUI(LoadColumns);
					break;
			}
		}
	}
}
