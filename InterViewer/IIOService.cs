using System;
using System.IO;
using System.Collections.Generic;
namespace InterViewer
{
	public interface IIOService
	{
		string AppPath { get;}

		string GetTemplateDirectory();

		string GetDocumentDirectory();

		/// <summary>
		/// 取得縮圖資料夾路徑
		/// </summary>
		/// <returns>The thumbnail directory.</returns>
		string GetThumbnailDirectory();

		void CheckDirectory(string path);

		IEnumerable<string> EnumerateFiles(string path, string searchPattern);

		Document FixDocument(Document doc);

		string ReadAllText(string path);

		void WriteAllText(string path, string contents);

		bool IsFileExists(string path);

		void CopyFile(string sourceFileName, string destFileName);
	}
}

