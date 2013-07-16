using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SIL.Cog.Presentation.Behaviors
{
	public static class TextBlockBehaviors
	{
		static TextBlockBehaviors()
		{
			EventManager.RegisterClassHandler(typeof(TextBlock), FrameworkElement.SizeChangedEvent, new SizeChangedEventHandler(OnTextBlockSizeChanged), true);
		}

		private static readonly DependencyPropertyKey IsTextTrimmedPropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsTextTrimmed", typeof(bool), typeof(TextBlockBehaviors),
			new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IsTextTrimmedProperty = IsTextTrimmedPropertyKey.DependencyProperty;

		public static bool GetIsTextTrimmed(DependencyObject obj)
		{
			return (bool) obj.GetValue(IsTextTrimmedProperty);
		}

		private static void SetIsTextTrimmed(DependencyObject obj, bool value)
		{
			obj.SetValue(IsTextTrimmedPropertyKey, value);
		}

		private static void OnTextBlockSizeChanged(object sender, SizeChangedEventArgs e)
		{
			var textBlock = (TextBlock) sender;
			SetIsTextTrimmed(textBlock, CalculateIsTextTrimmed(textBlock));
		}

		private static bool CalculateIsTextTrimmed(TextBlock textBlock)
		{
			double width = textBlock.ActualWidth;
			if (textBlock.TextTrimming == TextTrimming.None)
				return false;
			if (textBlock.TextWrapping != TextWrapping.NoWrap)
				return false;

			textBlock.Measure(new Size(double.MaxValue, double.MaxValue));
			double totalWidth = textBlock.DesiredSize.Width;
			return width < totalWidth;
		}

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
