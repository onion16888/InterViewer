using System;
using System.Collections.Generic;

namespace InterViewer.iOS
{
	public class InterViewerService : IService
	{
		public InterViewerService()
		{
		}

		public List<PdfTemplate> GetPdfTemplates()
		{
			throw new NotImplementedException();
		}

		public List<Document> GetDocuments()
		{
			//TODO: 整合後要修改為提供實際資料

			var list = new List<Document>();

			#region 生日公園

			list.Add(new Document
			{
				Title = "0912345678",
				Name = "20160620093043.json",
				Latitude = 22.619103,
				Longitude = 120.3029183
			});
			list.Add(new Document
			{
				Title = "0912345678",
				Name = "20160621102433.json",
				Latitude = 22.619103,
				Longitude = 120.3029183
			});

			#endregion

			#region 大遠百星巴克

			list.Add(new Document
			{
				Title = "075360501",
				Name = "20160615164233.json",
				Latitude = 22.6159696,
				Longitude = 120.3030369
			});


			list.Add(new Document
			{
				Title = "075360501",
				Name = "201606181103533.json",
				Latitude = 22.6159696,
				Longitude = 120.3030369
			});


			list.Add(new Document
			{
				Title = "075360501",
				Name = "20160620152433.json",
				Latitude = 22.6159696,
				Longitude = 120.3030369
			});

			#endregion

			#region 新光三越麥當勞

			list.Add(new Document
			{
				Title = "073392162",
				Name = "20160523102312.json",
				Latitude = 22.6160052,
				Longitude = 120.3035156
			});

			list.Add(new Document
			{
				Title = "073392162",
				Name = "20160621154412.json",
				Latitude = 22.6160052,
				Longitude = 120.3035156
			});

			#endregion

			#region 清心福全

			list.Add(new Document
			{
				Title = "072693355",
				Name = "20160321154412.json",
				Latitude = 22.6170184,
				Longitude = 120.2991495
			});

			#endregion

			#region 杯子珈琲館

			list.Add(new Document
			{
				Title = "073392470",
				Name = "20160303154412.json",
				Latitude = 22.6172743,
				Longitude = 120.2993143
			});

			list.Add(new Document
			{
				Title = "073392470",
				Name = "20160422092612.json",
				Latitude = 22.6172743,
				Longitude = 120.2993143
			});
			list.Add(new Document
			{
				Title = "073392470",
				Name = "20160105112012.json",
				Latitude = 22.6172743,
				Longitude = 120.2993143
			});
			list.Add(new Document
			{
				Title = "073392470",
				Name = "20160216163312.json",
				Latitude = 22.6172743,
				Longitude = 120.2993143
			});

			#endregion

			return list;
		}

		public void SaveAsJson(Document entity)
		{
			throw new NotImplementedException();
		}
	}
}

