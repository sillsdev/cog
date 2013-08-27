using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SIL.Cog.Presentation
{
	public static class PresentationExtensions
	{
		public static bool Validate(this DependencyObject dp)
		{
		   return !Validation.GetHasError(dp) &&
				LogicalTreeHelper.GetChildren(dp)
				.OfType<DependencyObject>()
				.All(Validate);
		}

		public static T FindVisualAncestor<T>(this DependencyObject child) where T : DependencyObject
		{
			DependencyObject parentObj = VisualTreeHelper.GetParent(child);
			if (parentObj == null)
				return null;
			var parent = parentObj as T;
			if (parent != null)
				return parent;
			return FindVisualAncestor<T>(parentObj);
		}

		public static T FindVisualChild<T>(this DependencyObject obj) where T : DependencyObject
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

		public static IEnumerable<string> SplitPropertyPath(this string propertyPath)
		{
			if (string.IsNullOrEmpty(propertyPath))
				yield break;

			foreach (string property in propertyPath.Split(new [] {'.'}, StringSplitOptions.RemoveEmptyEntries))
			{
				int bracketIndex = property.IndexOf('[');
				if (bracketIndex > -1)
				{
					yield return property.Substring(0, bracketIndex);
					yield return "Item" + property.Substring(bracketIndex);
				}
				else
				{
					yield return property;
				}
			}
		}

		public static IEnumerable<object> GetPropertyValues(this object obj, string propertyPath)
		{
			object currentObject = obj;
			foreach (string propertyStr in propertyPath.SplitPropertyPath())
			{
				Type currentType = currentObject.GetType();
				string prop = propertyStr;
				int bracketIndex = propertyStr.IndexOf('[');
				object[] indices = null;
				if (bracketIndex > -1)
				{
					prop = propertyStr.Substring(0, bracketIndex);
					string indexStr = propertyStr.Substring(bracketIndex + 1, propertyStr.Length - bracketIndex - 2);
					int index = int.Parse(indexStr);
					var coll = currentObject as ICollection;
					if (coll != null && index >= coll.Count)
						yield break;
					indices = new object[] {index};
				}

				PropertyInfo property = currentType.GetProperty(prop);
				if (property == null)
					yield break;

				currentObject = property.GetValue(currentObject, indices);
				if (currentObject == null)
					yield break;

				yield return currentObject;
			}
		}
	}
}
