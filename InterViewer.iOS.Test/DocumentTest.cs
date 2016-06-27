
using System;
using System.Collections.Generic;
using NUnit.Framework;
using InterViewer.iOS;

namespace InterViewer.iOS.Test
{
	[TestFixture]
	public class DocumentTest
	{
		private IIOService ioService;

		private IService service;

		private List<Document> documents;

		public DocumentTest()
		{
			ioService = new IOService();
			var service = new InterViewerService(ioService);
			documents = service.GetDocuments();
		}

		[Test]
		public void Pass()
		{
			var documentPath = ioService.GetDocumentDirectory();

			var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

			Assert.True(documentPath == path);
		}

		[Test]
		public void Fail()
		{
			Assert.False(documents.Count > 0);
		}

		[Test]
		[Ignore("another time")]
		public void Ignore()
		{
			Assert.True(false);
		}
	}
}
