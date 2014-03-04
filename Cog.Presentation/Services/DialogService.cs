using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using SIL.Cog.Application.Services;

namespace SIL.Cog.Presentation.Services
{
	public class DialogService : IDialogService
	{
		private readonly IWindowViewModelMappings _windowViewModelMappings;

		public DialogService(IWindowViewModelMappings windowViewModelMappings)
		{
			_windowViewModelMappings = windowViewModelMappings;
		}

		public bool? ShowModalDialog(object ownerViewModel, object viewModel)
		{
			Window dialog = CreateDialog(ownerViewModel, viewModel);
			return dialog.ShowDialog();
		}

		public void ShowModelessDialog(object ownerViewModel, object viewModel, Action closeCallback)
		{
			Window dialog = CreateDialog(ownerViewModel, viewModel);
			dialog.Closed += (sender, args) => closeCallback();
			dialog.Show();
		}

		private Window CreateDialog(object ownerViewModel, object viewModel)
		{
			Type dialogType = _windowViewModelMappings.GetWindowTypeFromViewModelType(viewModel.GetType());
			var dialog = (Window) Activator.CreateInstance(dialogType);
			Window owner = FindOwnerWindow(ownerViewModel);
			if (dialog != owner)
				dialog.Owner = owner;
			else
				dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			dialog.DataContext = viewModel;
			return dialog;
		}

		public bool CloseDialog(object viewModel)
		{
			Window dialog = System.Windows.Application.Current.Windows.Cast<Window>().SingleOrDefault(w => w.DataContext == viewModel);
			if (dialog != null)
			{
				dialog.Close();
				return true;
			}
			return false;
		}

		public FileDialogResult ShowOpenFileDialog(object ownerViewModel, string title, IEnumerable<FileType> fileTypes, FileType defaultFileType, string defaultFileName)
		{
			if (fileTypes == null) { throw new ArgumentNullException("fileTypes"); }
			List<FileType> fileTypesList = fileTypes.ToList();
			if (fileTypesList.Count == 0) { throw new ArgumentException("The fileTypes collection must contain at least one item."); }

			var dialog = new OpenFileDialog();

			return ShowFileDialog(ownerViewModel, dialog, title, fileTypesList, defaultFileType, defaultFileName);
		}

		public FileDialogResult ShowSaveFileDialog(object ownerViewModel, string title, IEnumerable<FileType> fileTypes, FileType defaultFileType, string defaultFileName)
		{
			if (fileTypes == null) { throw new ArgumentNullException("fileTypes"); }
			List<FileType> fileTypesList = fileTypes.ToList();
			if (fileTypesList.Count == 0) { throw new ArgumentException("The fileTypes collection must contain at least one item."); }

			var dialog = new SaveFileDialog();

			return ShowFileDialog(ownerViewModel, dialog, title, fileTypesList, defaultFileType, defaultFileName);
		}

		private FileDialogResult ShowFileDialog(object ownerViewModel, FileDialog dialog, string title, IList<FileType> fileTypes, FileType defaultFileType, string defaultFileName)
		{
			if (!string.IsNullOrEmpty(title))
				dialog.Title = title;
			List<FileType> fileTypesList = fileTypes.ToList();
			int filterIndex = fileTypesList.IndexOf(defaultFileType);
			if (filterIndex >= 0) { dialog.FilterIndex = filterIndex + 1; }
			if (!string.IsNullOrEmpty(defaultFileName))
			{
				dialog.FileName = Path.GetFileName(defaultFileName);
				string directory = Path.GetDirectoryName(defaultFileName);
				if (!string.IsNullOrEmpty(directory))
				{
					dialog.InitialDirectory = directory;
				}
			}

			dialog.Filter = CreateFilter(fileTypesList);
			if (dialog.ShowDialog(FindOwnerWindow(ownerViewModel)) == true)
			{
				filterIndex = dialog.FilterIndex - 1;
				if (filterIndex >= 0 && filterIndex < fileTypesList.Count)
				{
					defaultFileType = fileTypesList[filterIndex];
				}
				else
				{
					defaultFileType = null;
				}
				return new FileDialogResult(dialog.FileName, defaultFileType);
			}

			return new FileDialogResult();
		}

		private static string CreateFilter(IEnumerable<FileType> fileTypes)
		{
			string filter = "";
			foreach (FileType fileType in fileTypes)
			{
				if (!String.IsNullOrEmpty(filter)) { filter += "|"; }
				filter += fileType.Description + "|" + string.Join(";", fileType.FileExtensions.Select(ext => string.Format("*{0}", ext)));
			}
			return filter;
		}

		public void ShowMessage(object ownerViewModel, string message, string caption)
		{
			ShowMessageBox(ownerViewModel, message, caption, MessageBoxImage.Information);
		}

		public void ShowWarning(object ownerViewModel, string message, string caption)
		{
			ShowMessageBox(ownerViewModel, message, caption, MessageBoxImage.Warning);
		}

		public void ShowError(object ownerViewModel, string message, string caption)
		{
			ShowMessageBox(ownerViewModel, message, caption, MessageBoxImage.Error);
		}

		private void ShowMessageBox(object ownerViewModel, string message, string caption, MessageBoxImage icon)
		{
			Window owner = FindOwnerWindow(ownerViewModel);
			if (owner == null)
				MessageBoxEx.Show(message, caption, MessageBoxButton.OK, icon);
			else
				MessageBoxEx.Show(owner, message, caption, MessageBoxButton.OK, icon);
		}

		public bool? ShowQuestion(object ownerViewModel, string message, string caption)
		{
			Window owner = FindOwnerWindow(ownerViewModel);
			MessageBoxResult result;
			if (owner == null)
				result = MessageBoxEx.Show(message, caption, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
			else
				result = MessageBoxEx.Show(owner, message, caption, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
			if (result == MessageBoxResult.Yes)
				return true;
			if (result == MessageBoxResult.No)
				return false;
			return null;
		}

		public bool ShowYesNoQuestion(object ownerViewModel, string message, string caption)
		{
			Window owner = FindOwnerWindow(ownerViewModel);
			MessageBoxResult result;
			if (owner == null)
				result = MessageBoxEx.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
			else
				result = MessageBoxEx.Show(owner, message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
			return result == MessageBoxResult.Yes;
		}

		/// <summary>
		/// Finds window corresponding to specified ViewModel.
		/// </summary>
		private Window FindOwnerWindow(object viewModel)
		{
			if (System.Windows.Application.Current.Windows.Count == 1)
				return System.Windows.Application.Current.Windows[0];

			if (viewModel != null)
			{
				foreach (Window window in System.Windows.Application.Current.Windows)
				{
					if (FindViewModelView(window, viewModel))
						return window;
				}
			}
			return System.Windows.Application.Current.MainWindow;
		}

		private bool FindViewModelView(DependencyObject obj, object viewModel)
		{
			var elem = obj as FrameworkElement;
			if (elem != null && elem.DataContext == viewModel)
				return true;

			// Search immediate children first (breadth-first)
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject childObj = VisualTreeHelper.GetChild(obj, i);
				if (FindViewModelView(childObj, viewModel))
					return true;
			}

			return false;
		}
	}
}
