﻿using System;
using System.IO;
using System.Collections.Generic;
namespace InterViewer
{
	public interface IIOService
	{
		string AppPath { get;}

		string GetPdfDirectory();

		string GetTemplateDirectory();

		string GetDocumentDirectory();

		string GetImageDirectory();

		void CheckDirectory(string path);

		IEnumerable<string> EnumerateFiles(string path, string searchPattern);

		string ReadAllText(string path);

		void WriteAllText(string path, string contents);

		bool IsFileExists(string path);
	}
}

