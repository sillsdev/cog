using System;
using System.Collections.Generic;

namespace SIL.Cog.Application.Services
{
	public interface IDialogService
	{
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
		bool? ShowModalDialog(object ownerViewModel, object viewModel);

		void ShowModelessDialog(object ownerViewModel, object viewModel, Action closeCallback);

		bool CloseDialog(object viewModel);

		/// <summary>
		/// Shows the open file dialog box that allows a user to specify a file that should be opened.
		/// </summary>
		/// <param name="ownerViewModel">The ownerViewModel view model.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="fileTypes">The supported file types.</param>
		/// <param name="defaultFileType">Default file type.</param>
		/// <param name="defaultFileName">Default filename. The directory name is used as initial directory when it is specified.</param>
		/// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
		/// <exception cref="System.ArgumentNullException">fileTypes must not be null.</exception>
		/// <exception cref="System.ArgumentException">fileTypes must contain at least one item.</exception>
		FileDialogResult ShowOpenFileDialog(object ownerViewModel, string title, IEnumerable<FileType> fileTypes, FileType defaultFileType, string defaultFileName);

		/// <summary>
		/// Shows the save file dialog box that allows a user to specify a filename to save a file as.
		/// </summary>
		/// <param name="ownerViewModel">The ownerViewModel view model.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="fileTypes">The supported file types.</param>
		/// <param name="defaultFileType">Default file type.</param>
		/// <param name="defaultFileName">Default filename. The directory name is used as initial directory when it is specified.</param>
		/// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
		/// <exception cref="System.ArgumentNullException">fileTypes must not be null.</exception>
		/// <exception cref="System.ArgumentException">fileTypes must contain at least one item.</exception>
		FileDialogResult ShowSaveFileDialog(object ownerViewModel, string title, IEnumerable<FileType> fileTypes, FileType defaultFileType, string defaultFileName);

		/// <summary>
		/// Shows the message.
		/// </summary>
		/// <param name="ownerViewModel">The owner view model.</param>
		/// <param name="message">The message.</param>
		/// <param name="caption">The caption.</param>
		void ShowMessage(object ownerViewModel, string message, string caption);

		/// <summary>
		/// Shows the message as warning.
		/// </summary>
		/// <param name="ownerViewModel">The owner view model.</param>
		/// <param name="message">The message.</param>
		/// <param name="caption">The caption.</param>
		void ShowWarning(object ownerViewModel, string message, string caption);

		/// <summary>
		/// Shows the message as error.
		/// </summary>
		/// <param name="ownerViewModel">The owner view model.</param>
		/// <param name="message">The message.</param>
		/// <param name="caption">The caption.</param>
		void ShowError(object ownerViewModel, string message, string caption);

		/// <summary>
		/// Shows the specified question.
		/// </summary>
		/// <param name="ownerViewModel">The owner view model.</param>
		/// <param name="message">The question.</param>
		/// <param name="caption">The caption.</param>
		/// <returns><c>true</c> for yes, <c>false</c> for no and <c>null</c> for cancel.</returns>
		bool? ShowQuestion(object ownerViewModel, string message, string caption);

		/// <summary>
		/// Shows the specified yes/no question.
		/// </summary>
		/// <param name="ownerViewModel">The owner view model.</param>
		/// <param name="message">The question.</param>
		/// <param name="caption">The caption.</param>
		/// <returns><c>true</c> for yes and <c>false</c> for no.</returns>
		bool ShowYesNoQuestion(object ownerViewModel, string message, string caption);
	}
}
