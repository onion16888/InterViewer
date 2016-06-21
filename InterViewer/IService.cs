using System;
using System.Collections.Generic;

namespace InterViewer
{
	public interface IService
	{
		List<PdfTemplate> GetPdfTemplates();

		List<Document> GetDocuments();

		void SaveAsJson(Document entity);
	}
}

