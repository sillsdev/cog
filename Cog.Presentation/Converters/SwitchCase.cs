using System.Windows;
using System.Windows.Markup;

namespace SIL.Cog.Presentation.Converters
{
	[ContentProperty("Then")]
	public class SwitchCase : DependencyObject
	{
		private static readonly DependencyProperty WhenProperty = DependencyProperty.Register("When", typeof(object), typeof(SwitchCase));
		private static readonly DependencyProperty ThenProperty = DependencyProperty.Register("Then", typeof(object), typeof(SwitchCase));

		public object When
		{
			get { return GetValue(WhenProperty); }
			set { SetValue(WhenProperty, value); }
		}
 
		public object Then
		{
			get { return GetValue(ThenProperty); }
			set { SetValue(ThenProperty, value); }
		}
	}
}
