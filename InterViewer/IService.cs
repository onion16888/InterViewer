using System;
using System.Collections.Generic;

namespace InterViewer
{
	public interface IService
	{
		List<Template> GetTemplates();

		List<Document> GetDocuments();

		List<Document> GetDocumentsOrderBy(double latitude, double longitude);

		/// <summary>
		/// 地圖使用的測試資料
		/// </summary>
		/// <returns>The documents for map.</returns>
		List<Document> GetDocumentsForMap();

		void SaveAsJson(Document entity);

		/// <summary>
		/// 複製舊文件的附件資料到新文件
		/// </summary>
		/// <returns>The attachment.</returns>
		/// <param name="newDoc">New document.</param>
		/// <param name="oldDoc">Old dcoument.</param>
		Document CopyAttachment(Document newDoc, Document oldDoc);
	}
}

