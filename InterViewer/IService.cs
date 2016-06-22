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

		/// <summary>
		/// 地圖使用的測試資料
		/// </summary>
		/// <returns>The documents for map.</returns>
		List<Document> GetDocumentsForMap();

		void SaveAsJson(Document entity);
	}
}

