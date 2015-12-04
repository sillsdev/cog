using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SIL.Cog.Presentation.Converters
{
	[ContentProperty("Cases")]
	public class SwitchConverter : DependencyObject, IValueConverter
	{
		private static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(object), typeof(SwitchConverter));
		private readonly List<SwitchCase> _cases;
 
		public SwitchConverter()
		{
			_cases = new List<SwitchCase>();
		}
 
		public List<SwitchCase> Cases
		{
			get { return _cases; }
		}
 
		public object DefaultValue
		{
			get { return GetValue(DefaultValueProperty); }
			set { SetValue(DefaultValueProperty, value); }
		}
 
		// IValueConverter implementation
		public object Convert(object o, Type targetType, object parameter, CultureInfo culture)
		{
			foreach (SwitchCase switchCase in _cases)
			{
				if (EqualityComparer<object>.Default.Equals(switchCase.When, o) || switchCase.When.ToString().Equals(o.ToString(), StringComparison.InvariantCultureIgnoreCase))
					return switchCase.Then;
			}
 
			return DefaultValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
