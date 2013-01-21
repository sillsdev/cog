using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SIL.Cog.Views
{
	public static class ViewUtilities
	{
		public static bool IsValid(DependencyObject dp)
		{
		   return !Validation.GetHasError(dp) &&
				LogicalTreeHelper.GetChildren(dp)
				.OfType<DependencyObject>()
				.All(IsValid);
		}

		public static T FindVisualAncestor<T>(DependencyObject child) where T : DependencyObject
		{
			DependencyObject parentObj = VisualTreeHelper.GetParent(child);
			if (parentObj == null)
				return null;
			var parent = parentObj as T;
			if (parent != null)
				return parent;
			return FindVisualAncestor<T>(parentObj);
		}

		public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
		{
			// Search immediate children first (breadth-first)
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject childObj = VisualTreeHelper.GetChild(obj, i);

				var child = childObj as T;
				if (child != null)
					return child;

				var childOfChild = FindVisualChild<T>(childObj);
				if (childOfChild != null)
					return childOfChild;
			}

			return null;
		}
	}
}
