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

				var document = ioService.FixDocument(JsonConvert.DeserializeObject<Document>(jsonText));

				list.Add(document);
			}

			return list;
		}

		/// <summary>
		/// 傳入經緯度，計算文件距離後，由小到大排序後回傳文件
		/// </summary>
		/// <returns>The documents.</returns>
		/// <param name="latitude">經度</param>
		/// <param name="longitude">緯度</param>
		public List<Document> GetDocumentsOrderBy(double latitude, double longitude)
		{
			return GetDocuments().OrderBy(doc => doc.GetDistance(latitude, longitude)).ToList();
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

		public List<Template> GetTemplates()
		{
			var templates = new List<Template>();

			var exts = new[] { "pdf", "jpg", "jpeg", "png" };

			var path = ioService.GetTemplateDirectory();
			var files = ioService.EnumerateFiles(path, "*.*")
								 .Where(file => exts.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase)));

			foreach (var filePath in files)
			{
				if (ioService.IsFileExists(filePath))
				{
					var template = new Template
					{
						Name = Path.GetFileName(filePath),
						Path = filePath,
						Type=TemplateTypeEnum.Image
					};

					if (template.Name.EndsWith("pdf", StringComparison.OrdinalIgnoreCase))
					{
						template.Type = TemplateTypeEnum.PDF;
					}

					templates.Add(template);
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

		public Document CopyAttachment(Document newDoc, Document oldDoc)
		{
			var attachmentDir = DateTime.Now.Ticks.ToString();
			newDoc.Name = string.Format("{0}.json", attachmentDir);

			if (newDoc.Attachments == null)
			{
				newDoc.Attachments = new List<Attachment>();
			}

			foreach (var item in oldDoc.Attachments)
			{
				if (!string.IsNullOrWhiteSpace(item.Path))
				{
					var sourceFileName = item.Path;
					var fileName = Path.GetFileName(item.Path);
					var destFileName = Path.Combine(attachmentDir, fileName);
					item.Path = destFileName;

					#region 實際複製檔案

					var rootPath = ioService.GetDocumentDirectory();
					if (!sourceFileName.StartsWith(rootPath, StringComparison.Ordinal))
					{
						sourceFileName = Path.Combine(rootPath, sourceFileName);
					}

					if (!destFileName.StartsWith(rootPath, StringComparison.Ordinal))
					{
						ioService.CheckDirectory(Path.Combine(rootPath, attachmentDir));
						destFileName = Path.Combine(rootPath, destFileName);
					}
					ioService.CopyFile(sourceFileName, destFileName);

					#endregion
				}

				newDoc.Attachments.Add(item);
			}

			return newDoc;
		}
	}
}

