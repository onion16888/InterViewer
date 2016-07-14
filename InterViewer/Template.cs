using System;
namespace InterViewer
{
	/// <summary>
	/// 範本類別
	/// </summary>
	public class Template:RelayInterface
	{
		/// <summary>
		/// 檔案名稱(包含副檔名)
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// 檔案路徑(包含檔名)
		/// </summary>
		/// <value>The path.</value>
		public string Path { get; set; }

		/// <summary>
		/// 範本類型
		/// </summary>
		/// <value>The type.</value>
		public TemplateTypeEnum Type { get; set; }

		/// <summary>
		/// 縮圖(含路徑)
		/// </summary>
		/// <value>The thumbnail.</value>
		public string Thumbnail { get; set; }
	}
}

