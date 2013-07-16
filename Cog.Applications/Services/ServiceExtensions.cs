using System;
using System.Collections.Generic;

namespace SIL.Cog.Applications.Services
{
	public static class ServiceExtensions
	{
        /// <summary>
        /// Shows the open file dialog box that allows a user to specify a file that should be opened.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="fileType">The supported file type.</param>
        /// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
        /// <exception cref="System.ArgumentNullException">service must not be null.</exception>
        /// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
        public static FileDialogResult ShowOpenFileDialog(this IDialogService service, FileType fileType)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowOpenFileDialog(null, null, new[] { fileType }, fileType, null);
        }

		/// <summary>
		/// Shows the open file dialog box that allows a user to specify a file that should be opened.
		/// </summary>
		/// <param name="service">The file dialog service.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="fileType">The supported file type.</param>
		/// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
		/// <exception cref="System.ArgumentNullException">service must not be null.</exception>
		/// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
		public static FileDialogResult ShowOpenFileDialog(this IDialogService service, string title, FileType fileType)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowOpenFileDialog(null, title, new[] { fileType }, fileType, null);
        }

        /// <summary>
        /// Shows the open file dialog box that allows a user to specify a file that should be opened.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <param name="fileType">The supported file type.</param>
        /// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
        /// <exception cref="System.ArgumentNullException">service must not be null.</exception>
        /// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
        public static FileDialogResult ShowOpenFileDialog(this IDialogService service, object ownerViewModel, FileType fileType)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowOpenFileDialog(ownerViewModel, null, new[] { fileType }, fileType, null);
        }

		/// <summary>
		/// Shows the open file dialog box that allows a user to specify a file that should be opened.
		/// </summary>
		/// <param name="service">The file dialog service.</param>
		/// <param name="ownerViewModel">The owner view model.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="fileType">The supported file type.</param>
		/// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
		/// <exception cref="System.ArgumentNullException">service must not be null.</exception>
		/// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
		public static FileDialogResult ShowOpenFileDialog(this IDialogService service, object ownerViewModel, string title, FileType fileType)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowOpenFileDialog(ownerViewModel, title, new[] { fileType }, fileType, null);
        }

        /// <summary>
        /// Shows the open file dialog box that allows a user to specify a file that should be opened.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="fileType">The supported file type.</param>
        /// <param name="defaultFileName">Default filename. The directory name is used as initial directory when it is specified.</param>
        /// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
        /// <exception cref="System.ArgumentNullException">service must not be null.</exception>
        /// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
        public static FileDialogResult ShowOpenFileDialog(this IDialogService service, FileType fileType, string defaultFileName)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowOpenFileDialog(null, null, new[] { fileType }, fileType, defaultFileName);
        }

		/// <summary>
		/// Shows the open file dialog box that allows a user to specify a file that should be opened.
		/// </summary>
		/// <param name="service">The file dialog service.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="fileType">The supported file type.</param>
		/// <param name="defaultFileName">Default filename. The directory name is used as initial directory when it is specified.</param>
		/// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
		/// <exception cref="System.ArgumentNullException">service must not be null.</exception>
		/// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
		public static FileDialogResult ShowOpenFileDialog(this IDialogService service, string title, FileType fileType, string defaultFileName)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowOpenFileDialog(null, title, new[] { fileType }, fileType, defaultFileName);
        }

        /// <summary>
        /// Shows the open file dialog box that allows a user to specify a file that should be opened.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <param name="fileType">The supported file type.</param>
        /// <param name="defaultFileName">Default filename. The directory name is used as initial directory when it is specified.</param>
        /// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
        /// <exception cref="System.ArgumentNullException">service must not be null.</exception>
        /// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
        public static FileDialogResult ShowOpenFileDialog(this IDialogService service, object ownerViewModel, FileType fileType, string defaultFileName)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowOpenFileDialog(ownerViewModel, null, new[] { fileType }, fileType, defaultFileName);
        }

		/// <summary>
		/// Shows the open file dialog box that allows a user to specify a file that should be opened.
		/// </summary>
		/// <param name="service">The file dialog service.</param>
		/// <param name="ownerViewModel">The owner view model.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="fileType">The supported file type.</param>
		/// <param name="defaultFileName">Default filename. The directory name is used as initial directory when it is specified.</param>
		/// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
		/// <exception cref="System.ArgumentNullException">service must not be null.</exception>
		/// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
		public static FileDialogResult ShowOpenFileDialog(this IDialogService service, object ownerViewModel, string title, FileType fileType, string defaultFileName)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowOpenFileDialog(ownerViewModel, title, new[] { fileType }, fileType, defaultFileName);
        }

        /// <summary>
        /// Shows the open file dialog box that allows a user to specify a file that should be opened.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="fileTypes">The supported file types.</param>
        /// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
        /// <exception cref="System.ArgumentNullException">service must not be null.</exception>
        /// <exception cref="System.ArgumentNullException">fileTypes must not be null.</exception>
        /// <exception cref="System.ArgumentException">fileTypes must contain at least one item.</exception>
        public static FileDialogResult ShowOpenFileDialog(this IDialogService service, IEnumerable<FileType> fileTypes)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            return service.ShowOpenFileDialog(null, null, fileTypes, null, null);
        }

		/// <summary>
		/// Shows the open file dialog box that allows a user to specify a file that should be opened.
		/// </summary>
		/// <param name="service">The file dialog service.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="fileTypes">The supported file types.</param>
		/// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
		/// <exception cref="System.ArgumentNullException">service must not be null.</exception>
		/// <exception cref="System.ArgumentNullException">fileTypes must not be null.</exception>
		/// <exception cref="System.ArgumentException">fileTypes must contain at least one item.</exception>
		public static FileDialogResult ShowOpenFileDialog(this IDialogService service, string title, IEnumerable<FileType> fileTypes)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            return service.ShowOpenFileDialog(null, title, fileTypes, null, null);
        }

        /// <summary>
        /// Shows the open file dialog box that allows a user to specify a file that should be opened.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <param name="fileTypes">The supported file types.</param>
        /// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
        /// <exception cref="System.ArgumentNullException">service must not be null.</exception>
        /// <exception cref="System.ArgumentNullException">fileTypes must not be null.</exception>
        /// <exception cref="System.ArgumentException">fileTypes must contain at least one item.</exception>
        public static FileDialogResult ShowOpenFileDialog(this IDialogService service, object ownerViewModel, IEnumerable<FileType> fileTypes)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            return service.ShowOpenFileDialog(ownerViewModel, null, fileTypes, null, null);
        }

		/// <summary>
		/// Shows the open file dialog box that allows a user to specify a file that should be opened.
		/// </summary>
		/// <param name="service">The file dialog service.</param>
		/// <param name="ownerViewModel">The owner view model.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="fileTypes">The supported file types.</param>
		/// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
		/// <exception cref="System.ArgumentNullException">service must not be null.</exception>
		/// <exception cref="System.ArgumentNullException">fileTypes must not be null.</exception>
		/// <exception cref="System.ArgumentException">fileTypes must contain at least one item.</exception>
		public static FileDialogResult ShowOpenFileDialog(this IDialogService service, object ownerViewModel, string title, IEnumerable<FileType> fileTypes)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            return service.ShowOpenFileDialog(ownerViewModel, title, fileTypes, null, null);
        }

        /// <summary>
        /// Shows the open file dialog box that allows a user to specify a file that should be opened.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="fileTypes">The supported file types.</param>
        /// <param name="defaultFileType">Default file type.</param>
        /// <param name="defaultFileName">Default filename. The directory name is used as initial directory when it is specified.</param>
        /// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
        /// <exception cref="System.ArgumentNullException">service must not be null.</exception>
        /// <exception cref="System.ArgumentNullException">fileTypes must not be null.</exception>
        /// <exception cref="System.ArgumentException">fileTypes must contain at least one item.</exception>
        public static FileDialogResult ShowOpenFileDialog(this IDialogService service, IEnumerable<FileType> fileTypes, 
            FileType defaultFileType, string defaultFileName)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            return service.ShowOpenFileDialog(null, null, fileTypes, defaultFileType, defaultFileName);
        }

		/// <summary>
		/// Shows the open file dialog box that allows a user to specify a file that should be opened.
		/// </summary>
		/// <param name="service">The file dialog service.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="fileTypes">The supported file types.</param>
		/// <param name="defaultFileType">Default file type.</param>
		/// <param name="defaultFileName">Default filename. The directory name is used as initial directory when it is specified.</param>
		/// <returns>A FileDialogResult object which contains the filename selected by the user.</returns>
		/// <exception cref="System.ArgumentNullException">service must not be null.</exception>
		/// <exception cref="System.ArgumentNullException">fileTypes must not be null.</exception>
		/// <exception cref="System.ArgumentException">fileTypes must contain at least one item.</exception>
		public static FileDialogResult ShowOpenFileDialog(this IDialogService service, string title, IEnumerable<FileType> fileTypes, 
            FileType defaultFileType, string defaultFileName)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            return service.ShowOpenFileDialog(null, title, fileTypes, defaultFileType, defaultFileName);
        }

        /// <summary>
        /// Shows the save file dialog box that allows a user to specify a filename to save a file as.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="fileType">The supported file type.</param>
        /// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
        /// <exception cref="System.ArgumentNullException">service must not be null.</exception>
        /// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
        public static FileDialogResult ShowSaveFileDialog(this IDialogService service, FileType fileType)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowSaveFileDialog(null, null, new[] { fileType }, fileType, null);
        }

		/// <summary>
		/// Shows the save file dialog box that allows a user to specify a filename to save a file as.
		/// </summary>
		/// <param name="service">The file dialog service.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="fileType">The supported file type.</param>
		/// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
		/// <exception cref="System.ArgumentNullException">service must not be null.</exception>
		/// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
		public static FileDialogResult ShowSaveFileDialog(this IDialogService service, string title, FileType fileType)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowSaveFileDialog(null, title, new[] { fileType }, fileType, null);
        }

        /// <summary>
        /// Shows the save file dialog box that allows a user to specify a filename to save a file as.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <param name="fileType">The supported file type.</param>
        /// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
        /// <exception cref="System.ArgumentNullException">service must not be null.</exception>
        /// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
        public static FileDialogResult ShowSaveFileDialog(this IDialogService service, object ownerViewModel, FileType fileType)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowSaveFileDialog(ownerViewModel, null, new[] { fileType }, fileType, null);
        }

		/// <summary>
		/// Shows the save file dialog box that allows a user to specify a filename to save a file as.
		/// </summary>
		/// <param name="service">The file dialog service.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="ownerViewModel">The owner view model.</param>
		/// <param name="fileType">The supported file type.</param>
		/// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
		/// <exception cref="System.ArgumentNullException">service must not be null.</exception>
		/// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
		public static FileDialogResult ShowSaveFileDialog(this IDialogService service, string title, object ownerViewModel, FileType fileType)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowSaveFileDialog(ownerViewModel, title, new[] { fileType }, fileType, null);
        }

        /// <summary>
        /// Shows the save file dialog box that allows a user to specify a filename to save a file as.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="fileType">The supported file type.</param>
        /// <param name="defaultFileName">Default filename. The directory name is used as initial directory when it is specified.</param>
        /// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
        /// <exception cref="System.ArgumentNullException">service must not be null.</exception>
        /// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
        public static FileDialogResult ShowSaveFileDialog(this IDialogService service, FileType fileType, string defaultFileName)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowSaveFileDialog(null, null, new[] { fileType }, fileType, defaultFileName);
        }

		/// <summary>
		/// Shows the save file dialog box that allows a user to specify a filename to save a file as.
		/// </summary>
		/// <param name="service">The file dialog service.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="fileType">The supported file type.</param>
		/// <param name="defaultFileName">Default filename. The directory name is used as initial directory when it is specified.</param>
		/// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
		/// <exception cref="System.ArgumentNullException">service must not be null.</exception>
		/// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
		public static FileDialogResult ShowSaveFileDialog(this IDialogService service, string title, FileType fileType, string defaultFileName)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowSaveFileDialog(null, title, new[] { fileType }, fileType, defaultFileName);
        }

        /// <summary>
        /// Shows the save file dialog box that allows a user to specify a filename to save a file as.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <param name="fileType">The supported file type.</param>
        /// <param name="defaultFileName">Default filename. The directory name is used as initial directory when it is specified.</param>
        /// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
        /// <exception cref="System.ArgumentNullException">service must not be null.</exception>
        /// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
        public static FileDialogResult ShowSaveFileDialog(this IDialogService service, object ownerViewModel, FileType fileType, string defaultFileName)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowSaveFileDialog(ownerViewModel, null, new[] { fileType }, fileType, defaultFileName);
        }

		/// <summary>
		/// Shows the save file dialog box that allows a user to specify a filename to save a file as.
		/// </summary>
		/// <param name="service">The file dialog service.</param>
		/// <param name="ownerViewModel">The owner view model.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="fileType">The supported file type.</param>
		/// <param name="defaultFileName">Default filename. The directory name is used as initial directory when it is specified.</param>
		/// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
		/// <exception cref="System.ArgumentNullException">service must not be null.</exception>
		/// <exception cref="System.ArgumentNullException">fileType must not be null.</exception>
		public static FileDialogResult ShowSaveFileDialog(this IDialogService service, object ownerViewModel, string title, FileType fileType, string defaultFileName)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            if (fileType == null) { throw new ArgumentNullException("fileType"); }
            return service.ShowSaveFileDialog(ownerViewModel, title, new[] { fileType }, fileType, defaultFileName);
        }

        /// <summary>
        /// Shows the save file dialog box that allows a user to specify a filename to save a file as.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="fileTypes">The supported file types.</param>
        /// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
        /// <exception cref="System.ArgumentNullException">service must not be null.</exception>
        /// <exception cref="System.ArgumentNullException">fileTypes must not be null.</exception>
        /// <exception cref="System.ArgumentException">fileTypes must contain at least one item.</exception>
        public static FileDialogResult ShowSaveFileDialog(this IDialogService service, IEnumerable<FileType> fileTypes)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            return service.ShowSaveFileDialog(null, null, fileTypes, null, null);
        }

		/// <summary>
		/// Shows the save file dialog box that allows a user to specify a filename to save a file as.
		/// </summary>
		/// <param name="service">The file dialog service.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="fileTypes">The supported file types.</param>
		/// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
		/// <exception cref="System.ArgumentNullException">service must not be null.</exception>
		/// <exception cref="System.ArgumentNullException">fileTypes must not be null.</exception>
		/// <exception cref="System.ArgumentException">fileTypes must contain at least one item.</exception>
		public static FileDialogResult ShowSaveFileDialog(this IDialogService service, string title, IEnumerable<FileType> fileTypes)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            return service.ShowSaveFileDialog(null, title, fileTypes, null, null);
        }

        /// <summary>
        /// Shows the save file dialog box that allows a user to specify a filename to save a file as.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="ownerViewModel">The owner view model.</param>
        /// <param name="fileTypes">The supported file types.</param>
        /// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
        /// <exception cref="System.ArgumentNullException">service must not be null.</exception>
        /// <exception cref="System.ArgumentNullException">fileTypes must not be null.</exception>
        /// <exception cref="System.ArgumentException">fileTypes must contain at least one item.</exception>
        public static FileDialogResult ShowSaveFileDialog(this IDialogService service, object ownerViewModel, IEnumerable<FileType> fileTypes)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            return service.ShowSaveFileDialog(ownerViewModel, null, fileTypes, null, null);
        }

		/// <summary>
		/// Shows the save file dialog box that allows a user to specify a filename to save a file as.
		/// </summary>
		/// <param name="service">The file dialog service.</param>
		/// <param name="ownerViewModel">The owner view model.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="fileTypes">The supported file types.</param>
		/// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
		/// <exception cref="System.ArgumentNullException">service must not be null.</exception>
		/// <exception cref="System.ArgumentNullException">fileTypes must not be null.</exception>
		/// <exception cref="System.ArgumentException">fileTypes must contain at least one item.</exception>
		public static FileDialogResult ShowSaveFileDialog(this IDialogService service, object ownerViewModel, string title, IEnumerable<FileType> fileTypes)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            return service.ShowSaveFileDialog(ownerViewModel, title, fileTypes, null, null);
        }

        /// <summary>
        /// Shows the save file dialog box that allows a user to specify a filename to save a file as.
        /// </summary>
        /// <param name="service">The file dialog service.</param>
        /// <param name="fileTypes">The supported file types.</param>
        /// <param name="defaultFileType">Default file type.</param>
        /// <param name="defaultFileName">Default filename. The directory name is used as initial directory when it is specified.</param>
        /// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
        /// <exception cref="System.ArgumentNullException">service must not be null.</exception>
        /// <exception cref="System.ArgumentNullException">fileTypes must not be null.</exception>
        /// <exception cref="System.ArgumentException">fileTypes must contain at least one item.</exception>
        public static FileDialogResult ShowSaveFileDialog(this IDialogService service, IEnumerable<FileType> fileTypes, 
            FileType defaultFileType, string defaultFileName)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            return service.ShowSaveFileDialog(null, null, fileTypes, defaultFileType, defaultFileName);
        }

		/// <summary>
		/// Shows the save file dialog box that allows a user to specify a filename to save a file as.
		/// </summary>
		/// <param name="service">The file dialog service.</param>
		/// <param name="title">The dialog title.</param>
		/// <param name="fileTypes">The supported file types.</param>
		/// <param name="defaultFileType">Default file type.</param>
		/// <param name="defaultFileName">Default filename. The directory name is used as initial directory when it is specified.</param>
		/// <returns>A FileDialogResult object which contains the filename entered by the user.</returns>
		/// <exception cref="System.ArgumentNullException">service must not be null.</exception>
		/// <exception cref="System.ArgumentNullException">fileTypes must not be null.</exception>
		/// <exception cref="System.ArgumentException">fileTypes must contain at least one item.</exception>
		public static FileDialogResult ShowSaveFileDialog(this IDialogService service, string title, IEnumerable<FileType> fileTypes, 
            FileType defaultFileType, string defaultFileName)
        {
            if (service == null) { throw new ArgumentNullException("service"); }
            return service.ShowSaveFileDialog(null, title, fileTypes, defaultFileType, defaultFileName);
        }
	}
}
