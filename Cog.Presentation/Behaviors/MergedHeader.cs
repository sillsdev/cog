using System.Collections.Generic;
using System.Windows;

namespace SIL.Cog.Presentation.Behaviors
{
	public class MergedHeader : DependencyObject
	{
		public MergedHeader()
		{
			SetValue(ColumnNamesPropertyKey, new List<string>());
		}

		private static readonly DependencyPropertyKey ColumnNamesPropertyKey = DependencyProperty.RegisterReadOnly("ColumnNames", typeof(List<string>),
			typeof(MergedHeader), new FrameworkPropertyMetadata(new List<string>()));

		public static readonly DependencyProperty ColumnNamesProperty = ColumnNamesPropertyKey.DependencyProperty;

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MergedHeader), new FrameworkPropertyMetadata(null));

		public string Title
		{
			get { return (string) GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public List<string> ColumnNames
		{
			get { return (List<string>) GetValue(ColumnNamesProperty); }
		}
	}
}
