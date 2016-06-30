using System;
using System.Collections.Generic;

namespace InterViewer
{
	public class Document
	{
		/// <summary>
		/// 文件標題
		/// </summary>
		/// <value>The title.</value>
		public string Title { get; set; }

		/// <summary>
		/// 儲存的json檔名
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// 參考的PDF(含路徑)
		/// </summary>
		/// <value>The reference.</value>
		public string Reference { get; set; }

		/// <summary>
		/// 緯度
		/// </summary>
		/// <value>The latitude.</value>
		public double Latitude { get; set; }

		/// <summary>
		/// 經度
		/// </summary>
		/// <value>The longitude.</value>
		public double Longitude { get; set; }


		/// <summary>
		/// 縮圖(含路徑)
		/// </summary>
		/// <value>The thumbnail.</value>
		public string Thumbnail { get; set; }

		/// <summary>
		/// 附件
		/// </summary>
		/// <value>The attachments.</value>
		public List<Attachment> Attachments { get; set; }

		private const Double EARTH_RADIUS = 6371;

		public Double GetDistance(double latitude, double longitude)
		{
			Double DocLatitude = ConvertDegreeToRadians(Latitude);
			Double DocLongitude = ConvertDegreeToRadians(Longitude);
			Double TargetLatitude = ConvertDegreeToRadians(latitude);
			Double TargetLongitude = ConvertDegreeToRadians(longitude);

			Double distance = Math.Acos(
				(Math.Sin(DocLatitude) * Math.Sin(TargetLatitude)) + 
				(Math.Cos(DocLatitude) * Math.Cos(TargetLatitude) * Math.Cos(DocLongitude - TargetLongitude))
			);

			return distance * EARTH_RADIUS;
		}

		private Double ConvertDegreeToRadians(Double degrees)
		{
			return (Math.PI / 180) * degrees;
		}
	}
}