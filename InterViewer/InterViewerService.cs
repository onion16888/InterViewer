using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace InterViewer
{
	public class InterViewerService : IService
	{
		private IIOService ioService;

		public InterViewerService(IIOService ioService)
		{
			this.ioService = ioService;
		}

		public List<Document> GetDocuments()
		{
			var list = new List<Document>();

			var path = ioService.GetDocumentDirectory();

			var files = ioService.EnumerateFiles(path, "*.json");

			foreach (var file in files)
			{
				var jsonText = ioService.ReadAllText(file);

				var document = JsonConvert.DeserializeObject<Document>(jsonText);

				document.Thumbnail = path + "/InterView/Sliders/" + Path.GetFileName(document.Thumbnail);

				document.Reference = path + "/InterView/Sliders/" + Path.GetFileName(document.Reference);

				list.Add(document);
			}

			return list;
		}

		public List<Document> GetDocumentsForMap()
		{
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

			list.Add(new Document
			{
				Title = "073392470",
				Name = "20160216163312.json",
				Latitude = 25.0477376,
				Longitude = 121.5149763
			});

			list.Add(new Document
			{
				Title = "073392470",
				Name = "20160216163312.json",
				Latitude = 24.998855,
				Longitude = 121.581069
			});

			return list;
		}

		public List<PdfTemplate> GetPdfTemplates()
		{
			var templates = new List<PdfTemplate>();

			var exts = new[] { "pdf" };

			var path = ioService.GetDocumentDirectory();
			var files = ioService.EnumerateFiles(path, "*.*")
								 .Where(file => exts.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase)));

			foreach (var filePath in files)
			{
				if (ioService.IsFileExists(filePath))
				{
					templates.Add(new PdfTemplate
					{
						Name = Path.GetFileName(filePath),
						Path = filePath
					});
				}
			}

			return templates;
		}

		public void SaveAsJson(Document entity)
		{
			if (string.IsNullOrWhiteSpace(entity.Name))
			{
				entity.Name = string.Format("{0:yyyyMMddHHmmss}.json", DateTime.Now);
			}

			var json = JsonConvert.SerializeObject(entity, Newtonsoft.Json.Formatting.Indented);

			var path = ioService.GetDocumentDirectory();
			var filename = Path.Combine(path, entity.Name);
			ioService.WriteAllText(filename, json);
		}
	}
}

