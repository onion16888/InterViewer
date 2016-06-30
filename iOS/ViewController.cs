using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using CoreLocation;
using Foundation;
using MapKit;
using UIKit;
using Geolocator.Plugin;
using Geolocator.Plugin.Abstractions;

namespace InterViewer.iOS
{
	public partial class ViewController : UIViewController
	{
		public Document Doc { get; set; }

		private CLLocationCoordinate2D CenterLocation { get; set; }

		private Boolean CanvasEnable { get; set; } = false;

		private Boolean CollectionViewIsOpen { get; set; } = false;

		private ClusteringManager AnnotationsClusteringManager { get; set; }

		private List<Document> DocumentList { get; set; }

		private TableSource source { get; set; }

		public ViewController(IntPtr handle) : base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			ViewInit();

			MapViewInit();

			CreatePlusButton();

		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			LoadDocument();

			CollectionViewInit();

			AddAnnotations();
		}

		public override void ViewDidLayoutSubviews()
		{
			CollectionView.Frame = new CGRect(
				new CGPoint(CollectionViewIsOpen ? View.Frame.Size.Width - 430 : View.Frame.Size.Width, CollectionView.Frame.Y),
				CollectionView.Frame.Size
			);
			View.LayoutIfNeeded();
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.		
		}

		public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
		{

			if (segue.Identifier == "moveToListViewSegue")
			{
				var view = (ListViewController)segue.DestinationViewController;
				view.Doc = Doc;
			}

			if (segue.Identifier == "moveToDetailViewSegue")
			{
				var view = (DetailViewController)segue.DestinationViewController;
				view.PDF_Type = "Edit";
				view.Doc = Doc;
			}

			base.PrepareForSegue(segue, sender);

		}

		public void ViewInit()
		{
			btnNote.Enabled = true;
			btnClock.Enabled = false;
			btnTag.Enabled = false;
			btnMicrophone.Enabled = false;
			btnCamera.Enabled = false;
			btnPencil.Enabled = true;


			btnNote.UserInteractionEnabled = true;
			btnNote.AddGestureRecognizer(new UITapGestureRecognizer(tap =>
			{

				if (!CollectionViewIsOpen)
				{
					btnNote.Selected = CollectionViewIsOpen = true;

					UIView.Animate(0.5,
					() =>
					{
						CollectionView.Frame = new CGRect(
							new CGPoint(View.Frame.Size.Width - 430, CollectionView.Frame.Y),
							CollectionView.Frame.Size
						);
					});
				}
				else
				{
					btnNote.Selected = CollectionViewIsOpen = false;

					UIView.Animate(0.5,
					() =>
					{
						CollectionView.Frame = new CGRect(
							new CGPoint(View.Frame.Size.Width, CollectionView.Frame.Y),
							CollectionView.Frame.Size
						);
					});
				}
				View.LayoutIfNeeded();
			})
			{
				NumberOfTapsRequired = 1
			});


			btnPencil.UserInteractionEnabled = true;
			btnPencil.AddGestureRecognizer(new UITapGestureRecognizer(tap =>
			{
				if (!CanvasEnable)
				{
					MapCanvasView canvasView = new MapCanvasView
					{
						MapView = map,
						Frame = UIScreen.MainScreen.Bounds,
						Tag = 7
					};

					map.AddSubview(canvasView);

					map.BringSubviewToFront(View.ViewWithTag(1));

					btnPencil.Selected = CanvasEnable = true;
				}
				else
				{
					MapCanvasView canvasView = View.ViewWithTag(7) as MapCanvasView;

					//canvasView.GetLineList().ForEach(Line =>
					//{

					//	List<CLLocationCoordinate2D> CoordinateList = new List<CLLocationCoordinate2D>();

					//	Line.ForEach(Point =>
					//	{
					//		CoordinateList.Add(map.ConvertPoint(Point, map));
					//	});

					//	map.AddOverlay(MKPolyline.FromCoordinates(CoordinateList.ToArray()));

					//	View.SetNeedsDisplay();
					//});

					canvasView.RemoveFromSuperview();

					btnPencil.Selected = CanvasEnable = false;
				}
			})
			{
				NumberOfTapsRequired = 1
			});
		}

		public void LoadDocument()
		{
			InterViewerService DocumentManager = new InterViewerService(new IOService());

			DocumentList = DocumentManager.GetDocumentsOrderBy(CenterLocation.Latitude, CenterLocation.Longitude);

			if (source != null)
			{
				source.UpdateData(DocumentList);

				CollectionView.ReloadData();
			}
		}

		public void CollectionViewInit()
		{
			source = new TableSource(DocumentList);

			CollectionView.Source = source;

			CollectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout
			{
				SectionInset = new UIEdgeInsets(10, 10, 10, 10),
				ItemSize = new CGSize(200, 200),
				ScrollDirection = UICollectionViewScrollDirection.Vertical
			}, true);

			source.Selected += ItemOnSelected;
		}

		public async void MapViewInit()
		{
			CLLocationManager manager = new CLLocationManager();
			manager.RequestWhenInUseAuthorization();

			IGeolocator locator = CrossGeolocator.Current;
			locator.DesiredAccuracy = 50;

			Position position = await locator.GetPositionAsync(timeoutMilliseconds: 10000);

			Console.WriteLine("Position Status: {0}", position.Timestamp);
			Console.WriteLine("Position Latitude: {0}", position.Latitude);
			Console.WriteLine("Position Longitude: {0}", position.Longitude);

			CLLocationCoordinate2D mapCenter = new CLLocationCoordinate2D(position.Latitude, position.Longitude);
			//CLLocationCoordinate2D mapCenter = new CLLocationCoordinate2D(22.617193, 120.3032346);

			CenterLocation = map.CenterCoordinate = mapCenter;

			map.Region = MKCoordinateRegion.FromDistance(mapCenter, 1000, 1000);

			//map.ShowsUserLocation = true;

			CustomMapViewDelegate customDelegate = new CustomMapViewDelegate();
			customDelegate.OnRegionChanged += MapViewOnRegionChanged;
			map.Delegate = customDelegate;

		}

		public void AddAnnotations()
		{
			List<IMKAnnotation> AnnotationsList = new List<IMKAnnotation>();

			for (Int32 i = 0; i < DocumentList.Count; i++)
			{
				Document Doc = DocumentList[i];

				//map.AddAnnotation(new CustomAnnotation()
				//{
				//	Location = new CLLocationCoordinate2D(Doc.Latitude, Doc.Longitude),
				//	Count = i
				//});

				AnnotationsList.Add(new CustomAnnotation()
				{
					Location = new CLLocationCoordinate2D(Doc.Latitude, Doc.Longitude),
					Count = 1
				});
			}

			//Random rnd = new Random();
			//for (int i = 1; i <= 500; i++)
			//{
			//	double lat = 22.6183757 + rnd.NextDouble();
			//	double lon = 120.3028114 + rnd.NextDouble();
			//	AnnotationsList.Add(new MKPointAnnotation()
			//	{
			//		Coordinate = new CLLocationCoordinate2D(lat, lon),
			//	});
			//}

			AnnotationsClusteringManager = new ClusteringManager(AnnotationsList);

			AnnotationsClusteringManager.DisplayAnnotations(AnnotationsList, map);

			//map.AddAnnotation(new CustomAnnotation()
			//{
			//	Location = new CLLocationCoordinate2D(22.6115547, 120.2912767),
			//	Count = 2
			//});

			//map.AddAnnotations(new MKPointAnnotation()
			//{
			//	Coordinate = new CLLocationCoordinate2D(22.6115547, 120.2912767)
			//});

		}

		public void CreatePlusButton()
		{
			UIImageView ImageView = new UIImageView();
			UIView DrawView = new UIView();
			UIView BufferView = new UIView();
			UIView triangle = new UIView();
			UIView RectangleView = new UIView();
			UITextField TextField = new UITextField();
			UIButton AddButton = new UIButton();

			DrawView.Tag = 1;
			//RectangoView.Tag = 2;
			//TextField.Tag = 3;
			//AddButton.Tag = 4;
			//BufferView.Tag = 5;
			//ImageView.Tag = 6;

			BufferView.AddSubview(ImageView);
			RectangleView.AddSubview(BufferView);
			RectangleView.AddSubview(TextField);
			RectangleView.AddSubview(AddButton);

			UIBezierPath path = new UIBezierPath();
			path.MoveTo(new CGPoint(0, 0));
			path.AddLineTo(new CGPoint(15, 30));
			path.AddLineTo(new CGPoint(30, 0));
			path.AddLineTo(new CGPoint(0, 0));

			CoreAnimation.CAShapeLayer msaklayer = new CoreAnimation.CAShapeLayer();
			msaklayer.Path = path.CGPath;

			triangle.Frame = new CGRect(50 - 15, 0, 30, 30);
			triangle.Layer.Mask = msaklayer;
			triangle.Transform = CGAffineTransform.MakeRotation((Single)Math.PI);
			triangle.BackgroundColor = UIColor.White;


			DrawView.Frame = new CGRect((View.Frame.Size.Width / 2) - 50, (View.Frame.Size.Height / 2) - 8, 100, 122);
			DrawView.AddSubview(triangle);
			DrawView.AddSubview(RectangleView);

			//DrawView.BackgroundColor = UIColor.Blue;
			//DrawView.Center = new CGPoint(View.Frame.Size.Width / 2, View.Frame.Size.Height / 2);

			DrawView.Layer.MasksToBounds = false;
			DrawView.Layer.ShadowOffset = new CGSize(0, 10);
			DrawView.Layer.ShadowRadius = 8;
			DrawView.Layer.ShadowOpacity = 0.4f;

			map.AddSubview(DrawView);

			BufferView.Frame = new CGRect(10, 10, 80, 80);
			BufferView.BackgroundColor = UIColor.LightGray;
			BufferView.Layer.CornerRadius = 8;

			ImageView.Frame = new CGRect(25, 25, 30, 30);
			ImageView.Image = UIImage.FromBundle("plus.png");

			RectangleView.Frame = new CGRect(0, 22, 100, 100);
			RectangleView.BackgroundColor = UIColor.White;
			RectangleView.Layer.CornerRadius = 3;

			TextField.Frame = new CGRect(100, 10, 0, 80);
			TextField.Layer.CornerRadius = 8;
			TextField.Font = UIFont.BoldSystemFontOfSize(35f);
			TextField.BackgroundColor = UIColor.Green;
			TextField.ReturnKeyType = UIReturnKeyType.Done;
			TextField.Text = "TextField";

			//TextField.Delegate = UITextFieldDelegate;
			//UIView.AnimationsEnabled = false;

			TextField.EditingDidBegin += (object sender, EventArgs e) =>
			{
				Console.WriteLine("EditingDidBegin");

				UIView.Animate(0.5, () =>
				{
					CGPoint temp = DrawView.Frame.Location;

					DrawView.Frame = new CGRect(
						new CGPoint(temp.X, temp.Y - 100),
						DrawView.Frame.Size
					);

					View.LayoutIfNeeded();
				});
			};

			TextField.EditingDidEnd += (object sender, EventArgs e) =>
			{
				Console.WriteLine("EditingDidEnd");

				UIView.Animate(0.5, () =>
				{
					CGPoint temp = DrawView.Frame.Location;

					DrawView.Frame = new CGRect(
						new CGPoint(temp.X, temp.Y + 100),
						DrawView.Frame.Size
					);

					View.LayoutIfNeeded();
				});
			};

			TextField.ShouldReturn += (textField) =>
			{
				Console.WriteLine("ShouldReturn");

				textField.ResignFirstResponder();
				return true;
			};

			AddButton.Frame = new CGRect(400, 10, 0, 80);
			AddButton.Layer.CornerRadius = 8;
			AddButton.SetTitle("Add", UIControlState.Normal);
			AddButton.Font = UIFont.BoldSystemFontOfSize(35f);
			AddButton.SetTitleColor(UIColor.White, UIControlState.Normal);
			AddButton.BackgroundColor = UIColor.Red;


			BufferView.UserInteractionEnabled = true;
			BufferView.AddGestureRecognizer(new UITapGestureRecognizer(tap =>
			{

				Console.WriteLine("Tap");

				InvokeOnMainThread(() =>
				{
					if (DrawView.Frame.Size.Width == 100)
					{
						Console.WriteLine("Open");

						TextField.Text = "";

						UIView.Animate(0.25,
						() =>
						{
							BufferView.Transform = CGAffineTransform.MakeRotation((Single)Math.PI / 2);
						},
						() =>
						{
							BufferView.Transform = CGAffineTransform.MakeRotation(0);
							ImageView.Image = UIImage.FromBundle("minus.png");
						});

						UIView.Animate(0.5,
						() =>
						{
							DrawView.Frame = new CGRect(DrawView.Frame.Location, new CGSize(400, 122));
							RectangleView.Frame = new CGRect(RectangleView.Frame.Location, new CGSize(400, 100));
							TextField.Frame = new CGRect(TextField.Frame.Location, new CGSize(290, 80));
						}, null);

						UIView.Animate(0.25, 0.5, UIViewAnimationOptions.CurveLinear,
						() =>
						{
							DrawView.Frame = new CGRect(DrawView.Frame.Location, new CGSize(490, 122));
							RectangleView.Frame = new CGRect(RectangleView.Frame.Location, new CGSize(490, 100));
							AddButton.Frame = new CGRect(AddButton.Frame.Location, new CGSize(80, 80));
						},
						() =>
						{

						});
					}
					else
					{
						Console.WriteLine("Close");

						UIView.Animate(0.25,
						() =>
						{
							BufferView.Transform = CGAffineTransform.MakeRotation((Single)Math.PI / -2);
						},
						() =>
						{
							BufferView.Transform = CGAffineTransform.MakeRotation(0);
							ImageView.Image = UIImage.FromBundle("plus.png");
						});

						UIView.Animate(0.25,
						() =>
						{
							DrawView.Frame = new CGRect(DrawView.Frame.Location, new CGSize(400, 122));
							RectangleView.Frame = new CGRect(RectangleView.Frame.Location, new CGSize(400, 100));
							AddButton.Frame = new CGRect(AddButton.Frame.Location, new CGSize(0, 80));
						}, null);

						UIView.Animate(0.5, 0.25, UIViewAnimationOptions.CurveLinear,
						() =>
						{
							DrawView.Frame = new CGRect(DrawView.Frame.Location, new CGSize(100, 122));
							RectangleView.Frame = new CGRect(RectangleView.Frame.Location, new CGSize(100, 100));
							TextField.Frame = new CGRect(TextField.Frame.Location, new CGSize(0, 80));
						},
						() =>
						{

						});
					}
					View.LayoutIfNeeded();
				});
			})
			{
				NumberOfTapsRequired = 1
			});


			AddButton.UserInteractionEnabled = true;
			AddButton.AddGestureRecognizer(new UITapGestureRecognizer(tap =>
			{
				if (TextField.IsFirstResponder)
				{
					TextField.ResignFirstResponder();
				}

				Console.WriteLine(String.Format("TextField:{0}", TextField.Text));
				Console.WriteLine(String.Format("{0}, {1}", CenterLocation.Latitude, CenterLocation.Longitude));

				Doc = new Document()
				{
					Latitude = CenterLocation.Latitude,
					Longitude = CenterLocation.Longitude,
					Title = TextField.Text
				};

				PerformSegue("moveToListViewSegue", this);

				ImageView.Image = UIImage.FromBundle("plus.png");
				DrawView.Frame = new CGRect(DrawView.Frame.Location, new CGSize(100, 122));
				RectangleView.Frame = new CGRect(RectangleView.Frame.Location, new CGSize(100, 100));
				TextField.Frame = new CGRect(TextField.Frame.Location, new CGSize(0, 80));
				AddButton.Frame = new CGRect(AddButton.Frame.Location, new CGSize(0, 80));

			})
			{
				NumberOfTapsRequired = 1
			});
		}

		private void MapViewOnRegionChanged(Object sender, MKMapViewChangeEventArgs e)
		{

			CenterLocation = map.Region.Center;

			//var latitude = CenterLocation.Latitude;
			//var longitude = CenterLocation.Longitude;

			//Console.WriteLine(String.Format("{0}, {1}", latitude, longitude));


			Double scale = map.Bounds.Size.Width / map.VisibleMapRect.Size.Width;
			List<IMKAnnotation> annotationsToDisplay = AnnotationsClusteringManager.ClusteredAnnotationsWithinMapRect(map.VisibleMapRect, scale);
			AnnotationsClusteringManager.DisplayAnnotations(annotationsToDisplay, map);

			DocumentList = DocumentList.OrderBy(doc => doc.GetDistance(CenterLocation.Latitude, CenterLocation.Longitude)).ToList();

			source.UpdateData(DocumentList);

			CollectionView.ReloadData();

		}

		private void ItemOnSelected(Object sender, TableSource.SelectedEventArgs e)
		{
			
			Console.WriteLine(e.Selected);

			Doc = e.Selected;

			PerformSegue("moveToDetailViewSegue", this);

		}

		public class CustomMapViewDelegate : MKMapViewDelegate
		{

			public event EventHandler<MKMapViewChangeEventArgs> OnRegionChanged;

			public override void RegionChanged(MKMapView mapView, Boolean animated)
			{
				if (OnRegionChanged != null)
				{
					OnRegionChanged(mapView, new MKMapViewChangeEventArgs(animated));
				}
			}

			String pId = "PinAnnotation";
			String cId = "CustomAnnotation";

			public override MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
			{
				MKAnnotationView anView;

				Int32 annotationType =
					Convert.ToInt32(annotation is MKUserLocation) * 1 +
					Convert.ToInt32(annotation is CustomAnnotation) * 2;

				switch (annotationType)
				{
					case 1:
						{
							return null;
						}
					case 2:
						{
							CustomAnnotation customAnnotation = annotation as CustomAnnotation;

							anView = new MKAnnotationView(annotation, cId);

							UILabel DocCountLabel = new UILabel();

							DocCountLabel.Frame = new CGRect(-25, -25, 50, 50);
							DocCountLabel.BackgroundColor = UIColor.Red;
							DocCountLabel.TextColor = UIColor.White;
							DocCountLabel.TextAlignment = UITextAlignment.Center;
							DocCountLabel.Font = UIFont.BoldSystemFontOfSize(25f);
							if (null == customAnnotation.Annotations)
							{
								DocCountLabel.Text = customAnnotation.Count.ToString();
							}
							else 
							{
								DocCountLabel.Text = customAnnotation.Annotations.Count.ToString();
							}
							DocCountLabel.Layer.MasksToBounds = true;
							DocCountLabel.Layer.CornerRadius = 8;
							//DocCountLabel.Alpha = 0.5f;

							anView.AddSubview(DocCountLabel);

							anView.CanShowCallout = true;

							break;
						}
					default:
						{
							anView = (MKPinAnnotationView)mapView.DequeueReusableAnnotation(pId);

							if (anView == null)
							{
								anView = new MKPinAnnotationView(annotation, pId);
							}

							((MKPinAnnotationView)anView).PinColor = MKPinAnnotationColor.Red;
							anView.CanShowCallout = true;
							break;
						}
				}

				return anView;
			}

			public override MKOverlayRenderer OverlayRenderer(MKMapView mapView, IMKOverlay overlay)
			{
				MKPolylineRenderer polylineRenderer = new MKPolylineRenderer(overlay as MKPolyline);

				polylineRenderer.StrokeColor = UIColor.Black;
				polylineRenderer.FillColor = UIColor.Clear;
				polylineRenderer.LineWidth = 1.4f;

				return polylineRenderer;
			}

		}

		public class TableSource : UICollectionViewSource
		{
			const String CollectionViewCellIdentifier = "CollectionViewCell";

			public List<Document> Source { get; set; }

			public TableSource(List<Document> list)
			{
				Source = new List<Document>();
				Source.AddRange(list);
			}

			public void UpdateData(List<Document> list)
			{
				Source.Clear();
				Source.AddRange(list);
			}

			public override nint GetItemsCount(UICollectionView collectionView, nint section)
			{
				return Source.Count;
			}

			public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
			{
				var cell = collectionView.DequeueReusableCell(CollectionViewCellIdentifier, indexPath) as CollectionViewCell;

				var data = Source[indexPath.Row];

				cell.UpdateCellData(data);

				return cell;
			}

			public event EventHandler<SelectedEventArgs> Selected;

			public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
			{
				var data = Source[indexPath.Row];
				collectionView.DeselectItem(indexPath, true);

				// Raise Event
				EventHandler<SelectedEventArgs> handle = Selected;

				if (null != handle)
				{
					var args = new SelectedEventArgs { Selected = data };
					handle(this, args);

				}
			}

			public class SelectedEventArgs : EventArgs
			{
				public Document Selected { get; set; }
			}
		}

	}
}
