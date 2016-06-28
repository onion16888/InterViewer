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
		public int RectWidth { get; set;}
		public int RectHight { get; set; }
		public void FindBestRect(int pointX, int pointY)
		{
			

			if (leftX == 0)
			{
				leftX = pointX;
			}
			else
			{
				if (pointX < leftX)
				{
					leftX = pointX;
				}
			}

			if (topY == 0)
			{
				topY = pointY;
			}
			else
			{
				if (pointY < topY)
				{
					topY = pointY;
				}
			}


			if (rightX == 0)
			{
				rightX = pointX;
			}
			else
			{

				if (pointX > rightX)
				{
					rightX = pointX;
				}
			}

			if (bottomY == 0)
			{
				bottomY = pointY;
			}
			else
			{
				if (pointY > bottomY)
				{
					bottomY = pointY;
				}
			}

			GetRect = new Rect(leftX, topY, rightX, bottomY);
			RectWidth = rightX - leftX;
			RectHight = bottomY - topY;
		}




		
	}
}

