using System;
using System.Collections.Generic;
using System.IO;

namespace InterViewer.Droid
{
	public class IOService:IIOService
	{
		private string appPath;

		public IOService()
		{
			var rootPath = Android.OS.Environment.ExternalStorageDirectory.Path;
			appPath = Path.Combine(rootPath, "InterViewer");
			CheckDirectory(appPath);
		}

		public string AppPath
		{
			get
			{
				return appPath;
			}
		}

		public void CheckDirectory(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
		{
			return Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories);
		}

		public Document FixDocument(Document document)
		{
			return document;
		}

		public string GetDocumentDirectory()
		{
			var dirName = "Documents";
			return GetPath(dirName);
		}

		public string GetImageDirectory()
		{
			var dirName = "Images";
			return GetPath(dirName);
		}

		public string GetPdfDirectory()
		{
			var dirName = "Pdf";
			return GetPath(dirName);
		}

		public string GetTemplateDirectory()
		{
			var dirName = "Template";
			return GetPath(dirName);
		}

		public bool IsFileExists(string path)
		{
			return File.Exists(path);
		}

		public string ReadAllText(string path)
		{
			return File.ReadAllText(path);
		}

		public void WriteAllText(string path, string contents)
		{
			File.WriteAllText(path, contents);
		}

		private string GetPath(string directoryName)
		{
			var path = Path.Combine(appPath, directoryName);
			CheckDirectory(path);
			return path;
		}
	}
}

