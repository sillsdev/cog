using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SIL.Cog.Presentation.Controls
{
	public class ScrollingMergedHeaderCellDecorator : Decorator
	{
		public ScrollingMergedHeaderCellDecorator()
		{
			Focusable = false;
		}

		protected override Size ArrangeOverride( Size arrangeSize )
		{
			// We need to take ScrollingCellsDecoratorClipOffset into consideration
			// in case an animated column reordering is in progress
			double widthOffset = Math.Max(0, arrangeSize.Width);

			// Try to get the RectangleGeometry from the Clip
			// to avoid recreating one per call
			var clip = Clip as RectangleGeometry;
			if (clip == null)
			{
				clip = new RectangleGeometry();
				Clip = clip;
			}

			clip.Rect = new Rect(0, -0.5d, widthOffset, arrangeSize.Height + 1d);
			return base.ArrangeOverride(arrangeSize);
		}
	}
}
