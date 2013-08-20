using System.Windows;
using System.Windows.Controls;
using SIL.Cog.Presentation.Behaviors;

namespace SIL.Cog.Presentation.Controls
{
	public class MergedHeaderCell : ContentControl
	{
		static MergedHeaderCell()
		{
			FocusableProperty.OverrideMetadata(typeof(MergedHeaderCell), new FrameworkPropertyMetadata(false));
		}

		public MergedHeader MergedHeader { get; set; }
	}
}
