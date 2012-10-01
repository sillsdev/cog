using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
	}
}
