using System;
namespace InterViewer
{
	public class Attachment
	{
		public AttachmentTypeEnum Type { get; set; }

		public string Name { get; set; }

		public string Path { get; set; }

		public double X { get; set; }

		public double Y { get; set; }

		public double Height { get; set; }

		public double Width { get; set; }

		public int PageIndex { get; set;}
	}
}
