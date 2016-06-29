using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace InterViewer.iOS
{
	public class FileSystemHelper
	{
		public List<JsonFileSystem> FileList;
		private string _jsonString = String.Empty;
		private string _filePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"/InterView/Sliders/file.config";
		public FileSystemHelper()
		{
			//FileList = new List<JsonFileSystem>();
			_jsonString = LoadingFile();
			FileList = JsonConvert.DeserializeObject<List<JsonFileSystem>>(_jsonString);
		}
		private bool SavingFile(string source)
		{
			try
			{
				System.IO.File.WriteAllText(_filePath, source);
				log();
				return true;
			}
			catch
			{
				return false;
			}
		}
		private string LoadingFile()
		{
			//log();
			string json;
			FileStream theFile;
			if (!File.Exists(_filePath))
			{
				theFile = System.IO.File.Create(_filePath);
				json = System.IO.File.ReadAllText(_filePath);
				theFile.Close();
			}
			else
			{
				json = System.IO.File.ReadAllText(_filePath);
			}
			return json;
		}
		private void log()
		{
			System.IO.File.WriteAllText("/Users/sean/Desktop/JsonConfigPath", _filePath);
		}
		public void Save()
		{
			_jsonString = JsonConvert.SerializeObject(FileList);
			SavingFile(_jsonString);
		}
	}
	public class JsonFileSystem
	{
		public string FileName;
		public string FilePdfPath;
		public string FilePngPath;
		/// <summary>
		/// File_Type:
		/// "png","pdf"
		/// </summary>
		public string File_Type;
	}
}

