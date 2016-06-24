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
		FileManagerTableSource source;
		List<FileListAttributes> listFilePathName;
		public FileManagerController (IntPtr handle) : base (handle)
		{
		}
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			//Manager Start
			listFilePathName = getFileAndPathList();
			//警告,FileManagerTableSource,可能造成記憶體問題
			source = new FileManagerTableSource(listFilePathName);
			FileManagerTableView.Source = source;
			source.ItemClick += ListButtonClick;
			FileManagerTableView.ReloadData();
		}
		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
		/// <summary>
		/// The list button clicked
		/// </summary>
		/// <returns>The button click.</returns>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		private void ListButtonClick(object sender, FileManagerTableSource.SelectedEventArgs e)
		{
			if (e.SelectedName.IsFile == true)
			{
				//Debug.WriteLine(e.SelectedName.Name);
				string savingPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal)+@"/IntervView/Slides";
				showAlert("新增範本", e.SelectedName.Name + @" 新增成功!!", @"確定", this);
			}
			else
			{
				listFilePathName = getFileAndPathList(e.SelectedName.Name);
				source = new FileManagerTableSource(listFilePathName);
				FileManagerTableView.Source = source;
				source.ItemClick += ListButtonClick;
				FileManagerTableView.ReloadData();
			}
		}
		/// <summary>
		/// Gets the file and path list.
		/// </summary>
		/// <returns>FileListAttributes include file's or folder's name and path</returns>
		private List<FileListAttributes> getFileAndPathList()
		{
			List<FileListAttributes> theList = new List<FileListAttributes>();
			FileListAttributes theFileAttributes;
			string[] fileList = getFileList();
			string[] pathList = getPathList();
			for (int i = 0; i < fileList.Length; i++)
			{
				theFileAttributes = new FileListAttributes(true, fileList[i]);
				theList.Add(theFileAttributes);
			}
			for (int i = 0; i < pathList.Length; i++)
			{
				theFileAttributes = new FileListAttributes(false, pathList[i]);
				theList.Add(theFileAttributes);
			}
			return theList;
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
				theFileAttributes = new FileListAttributes(true, fileList[i]);
				theList.Add(theFileAttributes);
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
		private string[] getFileList()
		{
			var files = Directory.EnumerateFiles("/");
			return files.ToArray<String>();
		}
		/// <summary>
		/// Gets the path list.
		/// </summary>
		/// <returns>The path list.</returns>
		private string[] getPathList()
		{
			var paths = Directory.EnumerateDirectories("/");
			return paths.ToArray<String>();
		}
		/// <summary>
		/// Gets the file list.
		/// </summary>
		/// <returns>The file list.</returns>
		/// <param name="path">The searching location.Example : "/",is the root place</param>
		private string[] getFileList(string path)
		{
			var files = Directory.EnumerateFiles(path);
			return files.ToArray<String>();
		}
		/// <summary>
		/// Gets the path list.
		/// </summary>
		/// <returns>The path list.</returns>
		/// <param name="path">The searching location.Example : "/",is the root place</param>
		private string[] getPathList(string path)
		{
			var paths = Directory.EnumerateDirectories(path);
			return paths.ToArray<String>();
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
		/// <summary>
		/// Provide a simple way to show a alert window
		/// </summary>
		/// <returns>The alert.</returns>
		/// <param name="title">Title of the alert window</param>
		/// <param name="message">Message</param>
		/// <param name="actionTitle">The text on the check button</param>
		/// <param name="caller">Base ViewController</param>
		private void showAlert(string title, string message, string actionTitleOK, string actionTitleCancel, UIViewController caller)
		{
			//UIAlertController alertController = AlertViewControllerHelper.PresentOKCancelAlert(title, message, caller,
		}
	}
}
