// This file has been autogenerated from a class added in the UI designer.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UIKit;
using Debug = System.Diagnostics.Debug;

namespace InterViewer.iOS
{
	public partial class FileManagerController : UIViewController
	{
		QueryMode _queryMode;
		FileManagerTableSource _source;
		List<FileListAttributes> _listFilePathName;
		string _pathRightNow;
		string _folderSlides = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"/InterView/Sliders";
		string _selectedFile;
		public FileManagerController(IntPtr handle) : base(handle)
		{
		}
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			//default setting
			settingMode(new QueryMode() { status = "PDF" });

			//Manager Start
			setQueryPath("/");
			fileManagerDisplay();

			//return to previous page
			ReturnButton.TouchUpInside += returnButton_Click;
		}
		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
		/// <summary>
		/// Returns the button click.
		/// </summary>
		/// <returns>The button click.</returns>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		private void returnButton_Click(object sender, EventArgs e)
		{
			char[] seperater = { '/' };
			string[] pathTemp = _pathRightNow.Split(seperater);
			string pathForQuery = string.Empty;
			if (pathTemp.Length > 2)
			{
				for (int i = 1; i < pathTemp.Length - 1; i++)
				{
					pathForQuery += "/";
					pathForQuery += pathTemp[i];
				}
			}
			else
			{
				pathForQuery += "/";
			}
			setQueryPath(pathForQuery);
			fileManagerDisplay();
			ReturnButton.SetTitle("<< " + pathForQuery, UIControlState.Normal);
		}
		/// <summary>
		/// Lists the button click.
		/// </summary>
		/// <returns>The button click.</returns>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.(the selected item)</param>
		protected virtual void listButton_Click(object sender, FileManagerTableSource.SelectedEventArgs e)
		{
			if (e.SelectedName.IsFile == true)
			{
				selectPath(e.SelectedName.Name);
				listButtonAction();
			}
			else
			{
				setQueryPath(e.SelectedName.Name);
				fileManagerDisplay();
				ReturnButton.SetTitle("<< " + e.SelectedName.Name, UIControlState.Normal);
			}
		}
		/// <summary>
		/// Selects the path.
		/// </summary>
		/// <returns>The path.</returns>
		/// <param name="path">Path.</param>
		protected virtual void selectPath(string path)
		{
			_selectedFile = path;
		}
		/// <summary>
		/// Lists the button action.
		/// </summary>
		/// <returns>The button action.</returns>
		private void listButtonAction()
		{
			if (_queryMode.status == "PDF")
			{
				if (isPDF(_selectedFile))
				{
					copyToDestination(_selectedFile, _folderSlides);
					showAlert("新增PDF", _selectedFile + @" 新增成功!!", @"確定", this);
				}
				else
				{
					showAlert("新增PDF", _selectedFile + @" 不是PDF檔案", @"確定", this);
				}
			}
			if (_queryMode.status == "PNG")
			{
				if (isPNG(_selectedFile))
				{
					copyToDestination(_selectedFile, _folderSlides);
					showAlert("新增影像", _selectedFile + @" 新增成功!!", @"確定", this);
				}
				else
				{
					showAlert("新增影像", _selectedFile + @" 不是PNG檔案", @"確定", this);
				}
			}
		}
		/// <summary>
		/// Settings the mode.(For simulation dependency injection)
		/// </summary>
		/// <returns>The mode.</returns>
		/// <param name="queryMode">Query mode.</param>
		public virtual void settingMode(QueryMode queryMode)
		{
			this._queryMode = queryMode;
		}
		/// <summary>
		/// Simulation a Copy Button Click Event
		/// </summary>
		/// <returns>The copy button click.</returns>
		private void virtualCopyButtonClick()
		{
		}
		/// <summary>
		/// Copies to destination.
		/// </summary>
		/// <returns>The to destination.</returns>
		/// <param name="sourceName">Source name.</param>
		/// <param name="destinationFolder">Destination folder.</param>
		private bool copyToDestination(string sourceName, string destinationFolder)
		{
			try
			{
				if (!File.Exists(destinationFolder))
					Directory.CreateDirectory(destinationFolder);
				File.Copy(sourceName, destinationFolder + "/" + getFileNameFromFullFilePath(sourceName));
				return true;
			}
			catch
			{
				return false;
			}
		}
		/// <summary>
		/// Sets the query path.
		/// </summary>
		/// <returns>The query path.</returns>
		/// <param name="path">Path.</param>
		private void setQueryPath(string path)
		{
			_listFilePathName = getFileAndPathList(path);
			_pathRightNow = path;
		}
		/// <summary>
		/// Files the manager display.
		/// </summary>
		/// <returns>The manager display.</returns>
		private void fileManagerDisplay()
		{
			_source = new FileManagerTableSource(_listFilePathName);
			FileManagerTableView.Source = _source;
			_source.ItemClick += listButton_Click;
			FileManagerTableView.ReloadData();
		}
		/// <summary>
		/// Gets the file and path list.
		/// </summary>
		/// <returns>The file and path list.</returns>
		/// <param name="path">The searching location.Example : "/",is the root place.</param>
		private List<FileListAttributes> getFileAndPathList(string path)
		{
			List<FileListAttributes> theList = new List<FileListAttributes>();
			FileListAttributes theFileAttributes;
			string[] fileList = getFileList(path);
			string[] pathList = getPathList(path);
			for (int i = 0; i < fileList.Length; i++)
			{
				if (isPDF(fileList[i]))
				{
					theFileAttributes = new FileListAttributes(true, fileList[i]);
					theList.Add(theFileAttributes);
				}
			}
			for (int i = 0; i < pathList.Length; i++)
			{
				theFileAttributes = new FileListAttributes(false, pathList[i]);
				theList.Add(theFileAttributes);
			}
			return theList;
		}
		/// <summary>
		/// Gets the file list.
		/// </summary>
		/// <returns>The file list.</returns>
		/// <param name="path">The searching location.Example : "/",is the root place</param>
		private string[] getFileList(string path)
		{
			try
			{
				var files = Directory.EnumerateFiles(path);
				return files.ToArray<String>();
			}
			catch (System.UnauthorizedAccessException)
			{
				showAlert("Permission denied", "無權進入此檔案夾", "確認", this);
				var files = Directory.EnumerateFiles("/");
				return files.ToArray<String>();
			}
		}
		/// <summary>
		/// Gets the path list.
		/// </summary>
		/// <returns>The path list.</returns>
		/// <param name="path">The searching location.Example : "/",is the root place</param>
		private string[] getPathList(string path)
		{
			try
			{
				var paths = Directory.EnumerateDirectories(path);
				return paths.ToArray<String>();
			}
			catch (System.UnauthorizedAccessException)
			{
				showAlert("Permission denied", "無權進入此檔案夾", "確認", this);
				var paths = Directory.EnumerateDirectories("/");
				return paths.ToArray<String>();
			}
		}
		/// <summary>
		/// identifies the file type
		/// </summary>
		/// <returns>The pdf.</returns>
		/// <param name="fullFilePath">Full file path.</param>
		private bool isPDF(string fullFilePath)
		{
			return (Path.GetExtension(fullFilePath) == ".pdf" || Path.GetExtension(fullFilePath) == ".PDF" || Path.GetExtension(fullFilePath) == ".Pdf") ? true : false;
		}
		/// <summary>
		/// Ises the png.
		/// </summary>
		/// <returns>The png.</returns>
		/// <param name="fullFilePath">Full file path.</param>
		private bool isPNG(string fullFilePath)
		{
			return (Path.GetExtension(fullFilePath) == ".png" || Path.GetExtension(fullFilePath) == ".PNG" || Path.GetExtension(fullFilePath) == ".Png") ? true : false;
		}
		/// <summary>
		/// Gets the file name from full file path.
		/// </summary>
		/// <returns>The file name from full file path.</returns>
		/// <param name="fullFilePath">Full file path.</param>
		private string getFileNameFromFullFilePath(string fullFilePath)
		{
			return fullFilePath.Split(new char[] { '/' })[fullFilePath.Split(new char[] { '/' }).Length - 1];
		}
		/// <summary>
		/// Gets the path name from full file path.
		/// </summary>
		/// <returns>The path name from full file path.</returns>
		/// <param name="fullFilePath">Full file path.</param>
		private string getPathNameFromFullFilePath(string fullFilePath)
		{
			string[] pathTemp = fullFilePath.Split(new char[] { '/' });
			string folderPath = "";
			if (pathTemp.Length > 2)
			{
				for (int i = 1; i < pathTemp.Length - 2; i++)
				{
					folderPath += "/";
					folderPath += pathTemp[i];
				}
			}
			else
			{
				folderPath += "/";
			}

			return folderPath;
		}
		/// <summary>
		/// Provide a simple way to show a alert window
		/// </summary>
		/// <returns>The alert.</returns>
		/// <param name="title">Title of the alert window</param>
		/// <param name="message">Message</param>
		/// <param name="actionTitle">The text on the check button</param>
		/// <param name="caller">Base ViewController</param>
		private void showAlert(string title, string message, string actionTitle, UIViewController caller)
		{
			UIAlertController alertController = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
			alertController.AddAction(UIAlertAction.Create(actionTitle, UIAlertActionStyle.Default, null));
			caller.PresentViewController(alertController, true, null);
		}
	}
	/// <summary>
	/// For simulation dependency injection
	/// </summary>
	public class QueryMode
	{
		public string status;
	}
}
