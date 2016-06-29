using System;
namespace InterViewer
{
	public class Attachment
	{
		/// <summary>
		/// 附件類型
		/// </summary>
		/// <value>The type.</value>
		public AttachmentTypeEnum Type { get; set; }

		/// <summary>
		/// 附件名稱(含副檔名)
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// 附件儲存路徑
		/// </summary>
		/// <value>The path.</value>
		public string Path { get; set; }

		/// <summary>
		/// 便利貼文字內容
		/// </summary>
		/// <value>The note.</value>
		public string Note { get; set; }

		/// <summary>
		/// 在頁面的X坐標
		/// </summary>
		/// <value>The x.</value>
		public double X { get; set; }

		/// <summary>
		/// 在頁面的Y坐標
		/// </summary>
		/// <value>The y.</value>
		public double Y { get; set; }

		/// <summary>
		/// 附件在頁面的高度
		/// </summary>
		/// <value>The height.</value>
		public double Height { get; set; }

		/// <summary>
		/// 附件在頁面的寬度
		/// </summary>
		/// <value>The width.</value>
		public double Width { get; set; }

		/// <summary>
		/// 頁面索引，指示附件用於第幾頁
		/// </summary>
		/// <value>The index of the page.</value>
		public int PageIndex { get; set;}
	}
}
