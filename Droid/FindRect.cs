using System;
using Android.Views;
using Android.Graphics;
namespace InterViewer.Droid
{

	public class FindRect
	{
		public int leftX = 0;
		public int topY = 0;
		public int rightX = 0;
		public int bottomY = 0;
		public Rect GetRect { get; set; }
		public int RectWidth { get; set; }
		public int RectHight { get; set; }
		public void FindBestRect(int pointX, int pointY)
		{
			CheckMinMaxPoint(pointX, ref leftX, CheckStatus.Min);
			CheckMinMaxPoint(pointY, ref topY, CheckStatus.Min);
			CheckMinMaxPoint(pointX, ref rightX, CheckStatus.Max);
			CheckMinMaxPoint(pointY, ref bottomY, CheckStatus.Max);

			GetRect = new Rect(leftX, topY, rightX, bottomY);
			RectWidth = rightX - leftX;
			RectHight = bottomY - topY;
		}

		/// <summary>
		/// 檢查狀態，找最大或最小
		/// </summary>
		public enum CheckStatus
		{
			Max,
			Min
		}

		public void CheckMinMaxPoint(int checkPoint, ref int comparePoint, CheckStatus checkstatus)
		{
			if (comparePoint == 0)
			{
				comparePoint = checkPoint;
			}
			else
			{
				if (checkstatus == CheckStatus.Min)
				{
					if (checkPoint < comparePoint)
					{
						comparePoint = checkPoint;
					}
				}
				else
				{
					if (checkPoint > comparePoint)
					{
						comparePoint = checkPoint;
					}
				}

			}
		}


	}
}

