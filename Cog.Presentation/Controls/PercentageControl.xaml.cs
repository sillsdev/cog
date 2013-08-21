using System.Windows;

namespace SIL.Cog.Presentation.Controls
{
	/// <summary>
	/// Interaction logic for PercentageControl.xaml
	/// </summary>
	public partial class PercentageControl
	{
		public PercentageControl()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty MaxPercentageProperty = DependencyProperty.Register("MaxPercentage", typeof(double), typeof(PercentageControl),
			new FrameworkPropertyMetadata(1.0));

		public double MaxPercentage
		{
			get { return (double) GetValue(MaxPercentageProperty); }
			set { SetValue(MaxPercentageProperty, value); }
		}
	}
}
