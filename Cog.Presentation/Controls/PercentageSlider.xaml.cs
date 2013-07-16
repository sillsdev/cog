using System.Windows;

namespace SIL.Cog.Presentation.Controls
{
	/// <summary>
	/// Interaction logic for PercentageSlider.xaml
	/// </summary>
	public partial class PercentageSlider
	{
		public PercentageSlider()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(PercentageSlider),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public double Value
		{
			get { return (double) GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}
	}
}
