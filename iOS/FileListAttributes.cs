using System;
namespace InterViewer.iOS
{
	public class FileListAttributes
	{
		/// <summary>
		/// True is Path, False is File.
		/// </summary>
		public bool IsFile { get; set; }
		/// <summary>
		/// Name of File or Folder
		/// </summary>
		public string Name;
		/// <summary>
		/// Initializes a new instance of the <see cref="T:iOSFileManagerNoStoryBoardSmaple.FileAttributes"/> class.
		/// </summary>
		/// <param name="IsFile">when the "Name" is "File", set true. when the "Name" is "Path", set false.</param>
		/// <param name="Name">set the path or file name</param>
		public FileListAttributes(bool IsFile, string Name)
		{
			this.IsFile = IsFile;
			this.Name = Name;
		}
	}
}

