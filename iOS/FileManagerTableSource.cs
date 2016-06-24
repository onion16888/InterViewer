using System;
using System.Linq;
using System.Collections.Generic;
using UIKit;
using Foundation;
using Debug = System.Diagnostics.Debug;
namespace InterViewer.iOS
{
	public class FileManagerTableSource : UITableViewSource
	{
		List<FileListAttributes> tableItems;
		string cellIdentifier = "FileManagerTableCell";
		public FileManagerTableSource(List<FileListAttributes> items)
		{
			tableItems = new List<FileListAttributes>();
			tableItems.AddRange(items);
		}
		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return tableItems.Count;
		}
		public override UITableViewCell GetCell(UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell(cellIdentifier) as FileManagerTableCell;
			if (cell == null)
				cell = new UITableViewCell(UITableViewCellStyle.Default, cellIdentifier) as FileManagerTableCell;
			cell.ReloadData(tableItems[indexPath.Row].Name, tableItems[indexPath.Row].IsFile);
			return cell;
		}
		public event EventHandler<SelectedEventArgs> ItemClick;
		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			var file = tableItems[indexPath.Row];
			tableView.DeselectRow(indexPath, true);

			// Raise Event
			EventHandler<SelectedEventArgs> handle = ItemClick;

			if (null != handle)
			{
				var args = new SelectedEventArgs { SelectedName = file };
				handle(this, args);
			}
		}
		public class SelectedEventArgs : EventArgs
		{
			public FileListAttributes SelectedName { get; set; }
		}
	}
}

