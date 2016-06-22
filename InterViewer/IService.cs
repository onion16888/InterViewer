using System;
using System.Collections.Generic;

namespace InterViewer
{
	public interface IService
	{
		string GetPdfDirectory();

		string GetDocumentDirectory();

		string GetImageDirectory();

		List<PdfTemplate> GetPdfTemplates();

		List<Document> GetDocuments();

		void SaveAsJson(Document entity);
	}
}

