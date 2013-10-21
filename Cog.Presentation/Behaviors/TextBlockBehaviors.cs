using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SIL.Cog.Presentation.Behaviors
{
	public static class TextBlockBehaviors
	{
		public static readonly DependencyProperty InlinesListProperty = DependencyProperty.RegisterAttached("InlinesList", typeof(IEnumerable<Inline>), typeof(TextBlockBehaviors),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));

		public static void SetInlinesList(UIElement element, IEnumerable<Inline> inlines)
		{
			element.SetValue(InlinesListProperty, inlines);
		}

		public static IEnumerable<Inline> GetInlinesList(UIElement element)
		{
			return (IEnumerable<Inline>) element.GetValue(InlinesListProperty);
		}

		private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			var textBlock = (TextBlock) dependencyObject;
			textBlock.Inlines.Clear();
			var inlines = (IEnumerable<Inline>) e.NewValue;
			if (inlines != null)
				textBlock.Inlines.AddRange(inlines);
		}
	}
}
