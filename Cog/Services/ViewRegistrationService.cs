using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace SIL.Cog.Services
{
	public class ViewRegistrationService : IViewRegistrationService
	{
		private static readonly ViewRegistrationService RegistrationService = new ViewRegistrationService();

		public static ViewRegistrationService Instance
		{
			get { return RegistrationService; }
		}

		private readonly HashSet<FrameworkElement> _views; 

		private ViewRegistrationService()
		{
			_views = new HashSet<FrameworkElement>();
		}

		public IEnumerable<FrameworkElement> Views
		{
			get { return _views; }
		}

		/// <summary>
		/// Attached property describing whether a FrameworkElement is acting as a View in MVVM.
		/// </summary>
		public static readonly DependencyProperty IsRegisteredViewProperty =
			DependencyProperty.RegisterAttached(
			"IsRegisteredView",
			typeof(bool),
			typeof(ViewRegistrationService),
			new UIPropertyMetadata(IsRegisteredViewPropertyChanged));


		/// <summary>
		/// Gets value describing whether FrameworkElement is acting as View in MVVM.
		/// </summary>
		public static bool GetIsRegisteredView(FrameworkElement target)
		{
			return (bool)target.GetValue(IsRegisteredViewProperty);
		}


		/// <summary>
		/// Sets value describing whether FrameworkElement is acting as View in MVVM.
		/// </summary>
		public static void SetIsRegisteredView(FrameworkElement target, bool value)
		{
			target.SetValue(IsRegisteredViewProperty, value);
		}

		/// <summary>
		/// Is responsible for handling IsRegisteredViewProperty changes, i.e. whether
		/// FrameworkElement is acting as View in MVVM or not.
		/// </summary>
		private static void IsRegisteredViewPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			// The Visual Studio Designer or Blend will run this code when setting the attached
			// property, however at that point there is no IDialogService registered
			// in the ServiceLocator which will cause the Resolve method to throw a ArgumentException.
			if (DesignerProperties.GetIsInDesignMode(target)) return;

			var view = target as FrameworkElement;
			if (view != null)
			{
				// Cast values
				var newValue = (bool) e.NewValue;
				if (newValue)
					RegistrationService.Register(view);
				else
					RegistrationService.Unregister(view);
			}
		}

		public void Register(FrameworkElement view)
		{
			// Get owner window
			Window owner = view as Window ?? Window.GetWindow(view);
			if (owner == null)
			{
				// Perform a late register when the View hasn't been loaded yet.
				// This will happen if e.g. the View is contained in a Frame.
				view.Loaded += LateRegister;
				return;
			}

			// Register for owner window closing, since we then should unregister View reference,
			// preventing memory leaks
			owner.Closed += OwnerClosed;

			_views.Add(view);
		}

		public void Unregister(FrameworkElement view)
		{
			_views.Remove(view);
		}

		/// <summary>
		/// Callback for late View register. It wasn't possible to do a instant register since the
		/// View wasn't at that point part of the logical nor visual tree.
		/// </summary>
		private void LateRegister(object sender, RoutedEventArgs e)
		{
			var view = sender as FrameworkElement;
			if (view != null)
			{
				// Unregister loaded event
				view.Loaded -= LateRegister;

				// Register the view
				Register(view);
			}
		}

		/// <summary>
		/// Handles owner window closed, View service should then unregister all Views acting
		/// within the closed window.
		/// </summary>
		private void OwnerClosed(object sender, EventArgs e)
		{
			var owner = sender as Window;
			if (owner != null)
			{
				// Find Views acting within closed window
				IEnumerable<FrameworkElement> windowViews = _views.Where(view => owner.Equals(Window.GetWindow(view)));
				// Unregister Views in window
				foreach (FrameworkElement view in windowViews.ToArray())
					Unregister(view);
			}
		}
	}
}
