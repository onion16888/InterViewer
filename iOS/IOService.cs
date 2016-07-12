using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InterViewer.iOS
{
	public class IOService : IIOService
	{
		private string appPath;
		private string dataPath;

		public IOService()
		{
			appPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			dataPath = Path.Combine(appPath, "Data");
			CheckDirectory(dataPath);
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
			document.Thumbnail = appPath + "/InterView/Sliders/" + Path.GetFileName(document.Thumbnail);

			document.Reference = appPath + "/InterView/Sliders/" + Path.GetFileName(document.Reference);

			return document;
		}

		public bool IsFileExists(string path)
		{
			return File.Exists(path);
		}

		public string GetDocumentDirectory()
		{
			var dirName = "Documents";
			return GetPath(dirName);
		}

		public string GetTemplateDirectory()
		{
			var dirName = "Template";
			return GetPath(dirName);
		}

		public string ReadAllText(string path)
		{
			return File.ReadAllText(path);
		}

		public void WriteAllText(string path, string contents)
		{
			File.WriteAllText(path, contents);
		}

		public void CopyFile(string sourceFileName, string destFileName)
		{
			File.Copy(sourceFileName, destFileName, true);
		}

		private string GetPath(string directoryName)
		{
			var path = Path.Combine(dataPath, directoryName);
			CheckDirectory(path);
			return path;
		}
	}
}

