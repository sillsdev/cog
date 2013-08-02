﻿using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SIL.Cog.Applications.ViewModels;

namespace SIL.Cog.Presentation.Views
{
	/// <summary>
	/// Interaction logic for SoundClassesView.xaml
	/// </summary>
	public partial class SoundClassesView
	{
		public SoundClassesView()
		{
			InitializeComponent();
		}

		private void SoundClassesView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var vm = DataContext as SoundClassesViewModel;
			if (vm == null)
				return;

			vm.SoundClasses.CollectionChanged += SoundClasses_CollectionChanged;
		}

		private void SoundClasses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			FocusCurrentRow();
		}

		private void FocusCurrentRow()
		{
			SoundClassesGrid.Dispatcher.BeginInvoke(new Action(delegate
				{
					if (SoundClassesGrid.SelectedItem != null)
					{
						SoundClassesGrid.ScrollIntoView(SoundClassesGrid.SelectedItem);
						var row = (DataGridRow) SoundClassesGrid.ItemContainerGenerator.ContainerFromIndex(SoundClassesGrid.SelectedIndex);
						if (row != null)
						{
							row.Focusable = true;
							row.Focus();
						}
					}
				}), DispatcherPriority.ApplicationIdle);
		}
	}
}