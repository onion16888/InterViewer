using System;
using System.Collections.Generic;
using NUnit.Framework;
using InterViewer.Droid;
using System.IO;

namespace InterViewer.Droid.Test
{
	[TestFixture]
	public class TestsSample
	{
		private IIOService ioService;

		private IService service;

		public TestsSample()
		{
			ioService = new IOService();

			service = new InterViewerService(ioService);
		}

		[SetUp]
		public void Setup() { }


		[TearDown]
		public void Tear() { }

		[Test]
		public void Pass()
		{
			var documentDir = ioService.GetDocumentDirectory();

			var appPath = ioService.AppPath;
			var path = Path.Combine(appPath, "Documents");



			Console.WriteLine(documentDir);
			Assert.True(documentDir == path);
		}

		[Test]
		public void Fail()
		{
			var documents = service.GetDocuments();
			Assert.False(documents.Count > 0);
		}

		[Test]
		[Ignore("another time")]
		public void Ignore()
		{
			Assert.True(false);
		}

		[Test]
		public void Inconclusive()
		{
			Assert.Inconclusive("Inconclusive");
		}
	}
}

