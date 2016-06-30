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
		const int FIRST_PAGE = 1;
		string _folderSlides = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"/InterView/Sliders";
		FileManagerTableSource source;
		List<FileListAttributes> listFilePathName;
		string pathRightNow;
		public FileManagerController (IntPtr handle) : base (handle)
		{
		}
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			//Manager Start
			listFilePathName = getFileAndPathList("/");
			pathRightNow = "/";
			//警告,FileManagerTableSource,可能造成記憶體問題
			source = new FileManagerTableSource(listFilePathName);
			FileManagerTableView.Source = source;
			source.ItemClick += ListButton_Click;
			FileManagerTableView.ReloadData();

			ReturnButton.TouchUpInside += ReturnButton_Click;
		}
		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
		/// <summary>
		/// Returns to the previou page
		/// </summary>
		private void ReturnButton_Click(object sender, EventArgs e)
		{
			char[] seperater = { '/' };
			string[] pathTemp = pathRightNow.Split(seperater);
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
			listFilePathName = getFileAndPathList(pathForQuery);
			source = new FileManagerTableSource(listFilePathName);
			FileManagerTableView.Source = source;
			source.ItemClick += ListButton_Click;
			FileManagerTableView.ReloadData();
			ReturnButton.SetTitle("<< " + pathForQuery, UIControlState.Normal);
			pathRightNow = pathForQuery;
		}
		/// <summary>
		/// The list button clicked
		/// </summary>
		/// <returns>The button click.</returns>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		private void ListButton_Click(object sender, FileManagerTableSource.SelectedEventArgs e)
		{
			bool isFilePDF = isPDF(e.SelectedName.Name);
			if (e.SelectedName.IsFile == true)
			{
				if (isFilePDF == true)
				{
					if (!File.Exists(_folderSlides))
						Directory.CreateDirectory(_folderSlides);
					File.Copy(e.SelectedName.Name, _folderSlides + "/" + getFileNameFromFullFilePath(e.SelectedName.Name));
					PDFDocument theChoosedPDF = new PDFDocument(e.SelectedName.Name, FIRST_PAGE);
					theChoosedPDF.SaveAsPng(_folderSlides, getFileNameFromFullFilePath(e.SelectedName.Name).Replace(".pdf",".png"));
					showAlert("新增範本", e.SelectedName.Name + @" 新增成功!!", @"確定", this);
				}
				else
				{
					showAlert("新增範本", e.SelectedName.Name + @" 不是PDF檔案", @"確定", this);
				}
			}
			else
			{
				listFilePathName = getFileAndPathList(e.SelectedName.Name);
				source = new FileManagerTableSource(listFilePathName);
				FileManagerTableView.Source = source;
				source.ItemClick += ListButton_Click;
				FileManagerTableView.ReloadData();
				ReturnButton.SetTitle("<< "+e.SelectedName.Name, UIControlState.Normal);
				pathRightNow = e.SelectedName.Name;
			}
		}
		//private void enviromentCheck()
		//{
		//	if (!File.Exists(_folderSlides))
		//		Directory.CreateDirectory(_folderSlides);
		//}
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
			return (Path.GetExtension(fullFilePath) == ".pdf" || Path.GetExtension(fullFilePath) == ".PDF" || Path.GetExtension(fullFilePath) == ".Pdf")?true:false;
		}
		/// <summary>
		/// Gets the file name from full file path.
		/// </summary>
		/// <returns>The file name from full file path.</returns>
		/// <param name="fullFilePath">Full file path.</param>
		private string getFileNameFromFullFilePath(string fullFilePath)
		{
			return fullFilePath.Split(new char[] { '/' })[fullFilePath.Split(new char[] { '/' }).Length-1];
		}
		/// <summary>
		/// Gets the path name from full file path.
		/// </summary>
		/// <returns>The path name from full file path.</returns>
		/// <param name="fullFilePath">Full file path.</param>
		private string getPathNameFromFullFilePath(string fullFilePath)
		{
			string[] pathTemp = fullFilePath.Split(new char[]{'/'});
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
}
