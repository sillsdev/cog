using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace SIL.Cog.Services
{
	public class DialogService : IDialogService
	{
		private readonly IWindowViewModelMappings _windowViewModelMappings;

		public DialogService(IWindowViewModelMappings windowViewModelMappings)
		{
			_windowViewModelMappings = windowViewModelMappings;
		}

		/// <summary>
		/// Shows a dialog.
		/// </summary>
		/// <remarks>
		/// The dialog used to represent the ViewModel is retrieved from the registered mappings.
		/// </remarks>
		/// <param name="ownerViewModel">
		/// A ViewModel that represents the owner window of the dialog.
		/// </param>
		/// <param name="viewModel">The ViewModel of the new dialog.</param>
		/// <returns>
		/// A nullable value of type bool that signifies how a window was closed by the user.
		/// </returns>
		public bool? ShowDialog(object ownerViewModel, object viewModel)
		{
			Type dialogType = _windowViewModelMappings.GetWindowTypeFromViewModelType(viewModel.GetType());
			return ShowDialog(ownerViewModel, viewModel, dialogType);
		}

		/// <summary>
		/// Shows a dialog.
		/// </summary>
		/// <param name="ownerViewModel">
		/// A ViewModel that represents the owner window of the dialog.
		/// </param>
		/// <param name="viewModel">The ViewModel of the new dialog.</param>
		/// <param name="dialogType">The type of the dialog.</param>
		/// <returns>
		/// A nullable value of type bool that signifies how a window was closed by the user.
		/// </returns>
		private bool? ShowDialog(object ownerViewModel, object viewModel, Type dialogType)
		{
			// Create dialog and set properties
			var dialog = (Window) Activator.CreateInstance(dialogType);
			dialog.Owner = FindOwnerWindow(ownerViewModel);
			dialog.DataContext = viewModel;

			// Show dialog
			return dialog.ShowDialog();
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
                filter += fileType.Description + "|*" + fileType.FileExtension;
            }
            return filter;
        }

		public void ShowMessage(object ownerViewModel, string message, string caption)
		{
			MessageBox.Show(FindOwnerWindow(ownerViewModel), message, caption, MessageBoxButton.OK);
		}

		public void ShowWarning(object ownerViewModel, string message, string caption)
		{
			MessageBox.Show(FindOwnerWindow(ownerViewModel), message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
		}

		public void ShowError(object ownerViewModel, string message, string caption)
		{
			MessageBox.Show(FindOwnerWindow(ownerViewModel), message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
		}

		public bool? ShowQuestion(object ownerViewModel, string message, string caption)
		{
			MessageBoxResult result = MessageBox.Show(FindOwnerWindow(ownerViewModel), message, caption, MessageBoxButton.YesNoCancel,
				MessageBoxImage.Question, MessageBoxResult.Cancel);
			if (result == MessageBoxResult.Yes)
				return true;
			if (result == MessageBoxResult.No)
				return false;
			return null;
		}

		public bool ShowYesNoQuestion(object ownerViewModel, string message, string caption)
		{
			MessageBoxResult result = MessageBox.Show(FindOwnerWindow(ownerViewModel), message, caption, MessageBoxButton.YesNo,
				MessageBoxImage.Question, MessageBoxResult.No);
			return result == MessageBoxResult.Yes;
		}

		/// <summary>
		/// Finds window corresponding to specified ViewModel.
		/// </summary>
		private Window FindOwnerWindow(object viewModel)
		{
			if (Application.Current.Windows.Count == 1)
				return Application.Current.Windows[0];

			foreach (Window window in Application.Current.Windows)
			{
				if (FindViewModelView(window, viewModel))
					return window;
			}

			return Application.Current.MainWindow;
		}

		private bool FindViewModelView(DependencyObject obj, object viewModel)
		{
			// Search immediate children first (breadth-first)
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject childObj = VisualTreeHelper.GetChild(obj, i);

				var child = childObj as FrameworkElement;
				if (child != null && child.DataContext == viewModel)
					return true;

				if (FindViewModelView(childObj, viewModel))
					return true;
			}

			return false;
		}
	}
}
