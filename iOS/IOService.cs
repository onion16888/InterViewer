using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InterViewer.iOS
{
	public class IOService:IIOService
	{
		private string appPath;

		public IOService()
		{
			appPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
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
			//return Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories);

			return Directory.EnumerateFiles(path).Where(FilePath => Path.GetExtension(FilePath) == searchPattern).ToList();
		}

		public bool IsFileExists(string path)
		{
			return File.Exists(path);
		}

		public string GetDocumentDirectory()
		{
			return appPath;
		}

		public string GetImageDirectory()
		{
			var imagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
			CheckDirectory(imagePath);
			return imagePath;
		}

		public string GetPdfDirectory()
		{
			return appPath;
		}

		public string GetTemplateDirectory()
		{
			return appPath;
		}

		public string ReadAllText(string path)
		{
			return File.ReadAllText(path);
		}

		public void WriteAllText(string path, string contents)
		{
			File.WriteAllText(path, contents);
		}
	}
}

