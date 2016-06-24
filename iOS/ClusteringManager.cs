//
//  ClusteringManager.cs
//
//  Created by Filip Bec on 06/01/14.
//  Translated to C# by Dnote Software (www.dnote.nl) on Jan 4th, 2015
//  Copyright (c) 2014 Infinum Ltd. All rights reserved.
//
using System;
using System.Collections.Generic;
using MapKit;
using CoreLocation;
using Foundation;

namespace InterViewer.iOS
{
    public class ClusteringManager
    {
        public delegate double RespondsToSelectorHandler(ClusteringManager sender);
        public event ClusteringManager.RespondsToSelectorHandler RespondsToSelector;

        private QuadTree _tree = null;

        public ClusteringManager() : base()
        {
        }

        public ClusteringManager(List<IMKAnnotation> annotations) : base()
        {
            AddAnnotations(annotations);
        }

        public void SetAnnotations(List<IMKAnnotation> annotations)
        {
            _tree = null;
            AddAnnotations(annotations);
        }

        public void AddAnnotations(List<IMKAnnotation> annotations)
        {
            if (_tree == null)
                _tree = new QuadTree();

            lock(this) 
            {
                foreach (IMKAnnotation annotation in annotations)
                {
                    _tree.InsertAnnotation(annotation);
                }
            }
        }

        public void RemoveAnnotations(List<IMKAnnotation> annotations)
        {
            if (_tree == null)
                return;

            lock (this)
            {
                foreach (IMKAnnotation annotation in annotations)
                {
                    _tree.RemoveAnnotation(annotation);
                }
            }
        }

        public List<IMKAnnotation> ClusteredAnnotationsWithinMapRect(MKMapRect rect, double zoomScale)
        {
            return ClusteredAnnotationsWithinMapRect(rect, zoomScale, null);
        }

        public List<IMKAnnotation> ClusteredAnnotationsWithinMapRect(MKMapRect rect, double zoomScale, Dictionary<IMKAnnotation, bool> filter)
        {
            double cellSize = CellSizeForZoomScale(zoomScale);
            if (RespondsToSelector != null)
            {
                cellSize *= RespondsToSelector(this);
            }
            double scaleFactor = zoomScale / cellSize;

            int minX = (int)Math.Floor(rect.MinX * scaleFactor);
            int maxX = (int)Math.Floor(rect.MaxX * scaleFactor);
            int minY = (int)Math.Floor(rect.MinY * scaleFactor);
            int maxY = (int)Math.Floor(rect.MaxY * scaleFactor);

            List<IMKAnnotation> clusteredAnnotations = new List<IMKAnnotation>();

            lock (this)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        MKMapRect mapRect = new MKMapRect(x / scaleFactor, y / scaleFactor, 1.0 / scaleFactor, 1.0 / scaleFactor);
                        BoundingBox mapBox = Utils.BoundingBoxForMapRect(mapRect);

                        double totalLatitude = 0;
                        double totalLongitude = 0;

                        List<IMKAnnotation> annotations = new List<IMKAnnotation>();

                        _tree.EnumerateAnnotationsInBox(mapBox, delegate(IMKAnnotation annotation)
                            {
                                if (filter == null || filter[annotation])
                                {
                                    totalLatitude += annotation.Coordinate.Latitude;
                                    totalLongitude += annotation.Coordinate.Longitude;
                                    annotations.Add(annotation);
                                }
                            });

                        int count = annotations.Count;
                        if (count == 1)
                            clusteredAnnotations.AddRange(annotations);
                        if (count > 1)
                        {
                            CLLocationCoordinate2D coordinate = new CLLocationCoordinate2D(totalLatitude/count, totalLongitude/count);
							CustomAnnotation cluster = new CustomAnnotation() 
							{ 
								Location = coordinate
							};
                            cluster.Annotations = annotations;
                            clusteredAnnotations.Add(cluster);
                        }
                            
                    }
                }
            }

            return clusteredAnnotations;
        }

        public List<IMKAnnotation> AllAnnotations()
        {
            List<IMKAnnotation> annotations = new List<IMKAnnotation>();

            lock (this)
            {
                _tree.EnumerateAnnotations(delegate(IMKAnnotation annotation)
                    {
                        annotations.Add(annotation);
                    });
            }

            return annotations;
        }

        public void DisplayAnnotations(List<IMKAnnotation> annotations, MKMapView mapView)
        {
            List<IMKAnnotation> before = new List<IMKAnnotation>();
            foreach (IMKAnnotation annotation in mapView.Annotations)
                before.Add(annotation);
            //MKUserLocation userLocation = mapView.UserLocation;
            //if (userLocation != null)
            //    before.Remove(userLocation);
            List<IMKAnnotation> after = new List<IMKAnnotation>(annotations);
            List<IMKAnnotation> toKeep = new List<IMKAnnotation>(before);
            toKeep = Utils.Intersect(toKeep, after);
            List<IMKAnnotation> toAdd = new List<IMKAnnotation>(after);
            toAdd.RemoveAll((IMKAnnotation obj) =>
                {
                    return toKeep.Contains(obj);
                });
            List<IMKAnnotation> toRemove = new List<IMKAnnotation>(before);
            toRemove.RemoveAll((IMKAnnotation obj) =>
                {
                    return after.Contains(obj);
                });

            NSOperationQueue.MainQueue.AddOperation(delegate ()
                {
                    mapView.AddAnnotations(toAdd.ToArray());
                    mapView.RemoveAnnotations(toRemove.ToArray());
                }
            );

        }

        public int ZoomScaleToZoomLevel(double scale)
        {
            double totalTilesAtMaxZoom = MKMapSize.World.Width / 256.0;
            int zoomLevelAtMaxZoom = (int)Math.Log(totalTilesAtMaxZoom, 2);
            int zoomLevel = (int)Math.Max(0, zoomLevelAtMaxZoom + Math.Floor(Math.Log(scale, 2) + 0.5));

            return zoomLevel;
        }

        public float CellSizeForZoomScale(double zoomScale)
        {
            int zoomLevel = ZoomScaleToZoomLevel(zoomScale);

            switch (zoomLevel) {
                case 13:
                case 14:
                case 15:
                    return 64;
                case 16:
                case 17:
                case 18:
                    return 32;
                case 19:
                    return 16;

                default:
                    return 88;
            }
        }

    }

	public struct BoundingBox
	{
		public double x0;
		public double y0;
		public double xf;
		public double yf;
	}

	public class Consts
	{
		public const int kNodeCapacity = 8;
	}

	public class QuadTree
	{
		private QuadTreeNode _rootNode;

		public QuadTree()
		{
			_rootNode = new QuadTreeNode(Utils.BoundingBoxForMapRect(new MKMapRect().World));
		}

		public bool InsertAnnotation(IMKAnnotation annotation)
		{
			return InsertAnnotation(annotation, _rootNode);
		}

		public bool RemoveAnnotation(IMKAnnotation annotation)
		{
			return RemoveAnnotation(annotation, _rootNode);
		}

		public bool RemoveAnnotation(IMKAnnotation annotation, QuadTreeNode fromNode)
		{
			if (!Utils.BoundingBoxContainsCoordinate(fromNode.BoundingBox, annotation.Coordinate))
			{
				return false;
			}

			if (fromNode.Annotations.Contains(annotation))
			{
				fromNode.Annotations.Remove(annotation);
				fromNode.Count--;
				return true;
			}

			if (RemoveAnnotation(annotation, fromNode.NorthEast)) return true;
			if (RemoveAnnotation(annotation, fromNode.NorthWest)) return true;
			if (RemoveAnnotation(annotation, fromNode.SouthEast)) return true;
			if (RemoveAnnotation(annotation, fromNode.SouthWest)) return true;

			return false;
		}

		public bool InsertAnnotation(IMKAnnotation annotation, QuadTreeNode toNode)
		{
			if (!Utils.BoundingBoxContainsCoordinate(toNode.BoundingBox, annotation.Coordinate))
			{
				return false;
			}

			if (toNode.Count < Consts.kNodeCapacity)
			{
				toNode.Annotations.Add(annotation);
				toNode.Count++;
				return true;
			}

			if (toNode.IsLeaf())
			{
				toNode.Subdivide();
			}

			if (InsertAnnotation(annotation, toNode.NorthEast)) return true;
			if (InsertAnnotation(annotation, toNode.NorthWest)) return true;
			if (InsertAnnotation(annotation, toNode.SouthEast)) return true;
			if (InsertAnnotation(annotation, toNode.SouthWest)) return true;

			return false;
		}

		public delegate void AnnotationEnumDelegate(IMKAnnotation annotation);

		public void EnumerateAnnotationsInBox(BoundingBox box, AnnotationEnumDelegate enumFunc)
		{
			EnumerateAnnotationsInBox(box, _rootNode, enumFunc);
		}

		public void EnumerateAnnotations(AnnotationEnumDelegate enumFunc)
		{
			EnumerateAnnotationsInBox(Utils.BoundingBoxForMapRect(new MKMapRect().World), _rootNode, enumFunc);
		}

		public void EnumerateAnnotationsInBox(BoundingBox box, QuadTreeNode withNode, AnnotationEnumDelegate enumFunc)
		{
			if (!Utils.BoundingBoxIntersectsBoundingBox(withNode.BoundingBox, box))
			{
				return;
			}

			List<IMKAnnotation> tempArray = new List<IMKAnnotation>(withNode.Annotations);

			foreach (IMKAnnotation annotation in tempArray)
			{
				if (Utils.BoundingBoxContainsCoordinate(box, annotation.Coordinate))
				{
					enumFunc(annotation);
				}
			}

			if (withNode.IsLeaf())
			{
				return;
			}

			EnumerateAnnotationsInBox(box, withNode.NorthEast, enumFunc);
			EnumerateAnnotationsInBox(box, withNode.NorthWest, enumFunc);
			EnumerateAnnotationsInBox(box, withNode.SouthEast, enumFunc);
			EnumerateAnnotationsInBox(box, withNode.SouthWest, enumFunc);
		}
	}

	public class QuadTreeNode
	{
		public BoundingBox BoundingBox;
		public List<IMKAnnotation> Annotations;
		public int Count;
		public QuadTreeNode NorthEast;
		public QuadTreeNode NorthWest;
		public QuadTreeNode SouthEast;
		public QuadTreeNode SouthWest;

		public QuadTreeNode()
		{
			Init();
		}


		public QuadTreeNode(BoundingBox box) : base()
		{
			Init();
			BoundingBox = box;
		}

		private void Init()
		{
			Count = 0;
			NorthEast = null;
			NorthWest = null;
			SouthEast = null;
			SouthWest = null;
			Annotations = new List<IMKAnnotation>(Consts.kNodeCapacity);
		}

		public bool IsLeaf()
		{
			return NorthEast != null ? false : true;
		}

		public void Subdivide()
		{
			NorthEast = new QuadTreeNode();
			NorthWest = new QuadTreeNode();
			SouthEast = new QuadTreeNode();
			SouthWest = new QuadTreeNode();

			BoundingBox box = BoundingBox;
			double xMid = (box.xf + box.x0) / 2.0;
			double yMid = (box.yf + box.y0) / 2.0;

			NorthEast.BoundingBox = Utils.BoundingBoxMake(xMid, box.y0, box.xf, yMid);
			NorthWest.BoundingBox = Utils.BoundingBoxMake(box.x0, box.y0, xMid, yMid);
			SouthEast.BoundingBox = Utils.BoundingBoxMake(xMid, yMid, box.xf, box.yf);
			SouthWest.BoundingBox = Utils.BoundingBoxMake(box.x0, yMid, xMid, box.yf);
		}
	}

	public class Utils
	{
		public static BoundingBox BoundingBoxMake(double x0, double y0, double xf, double yf)
		{
			BoundingBox box;
			box.x0 = x0;
			box.y0 = y0;
			box.xf = xf;
			box.yf = yf;
			return box;
		}

		public static BoundingBox BoundingBoxForMapRect(MKMapRect mapRect)
		{
			CLLocationCoordinate2D topLeft = MKMapPoint.ToCoordinate(mapRect.Origin);
			CLLocationCoordinate2D botRight = MKMapPoint.ToCoordinate(new MKMapPoint(mapRect.MaxX, mapRect.MaxY));

			double minLat = botRight.Latitude;
			double maxLat = topLeft.Latitude;
			double minLon = topLeft.Longitude;
			double maxLon = botRight.Longitude;

			return BoundingBoxMake(minLat, minLon, maxLat, maxLon);
		}

		public static MKMapRect MapRectForBoundingBox(BoundingBox boundingBox)
		{
			MKMapPoint topLeft = MKMapPoint.FromCoordinate(new CLLocationCoordinate2D(boundingBox.x0, boundingBox.y0));
			MKMapPoint botRight = MKMapPoint.FromCoordinate(new CLLocationCoordinate2D(boundingBox.xf, boundingBox.yf));

			return new MKMapRect(topLeft.X, botRight.Y, Math.Abs(botRight.X - topLeft.X), Math.Abs(botRight.Y - topLeft.Y));
		}

		public static bool BoundingBoxContainsCoordinate(BoundingBox box, CLLocationCoordinate2D coordinate)
		{
			bool containsX = box.x0 <= coordinate.Latitude && coordinate.Latitude <= box.xf;
			bool containsY = box.y0 <= coordinate.Longitude && coordinate.Longitude <= box.yf;
			return containsX && containsY;
		}

		public static bool BoundingBoxIntersectsBoundingBox(BoundingBox box1, BoundingBox box2)
		{
			return (box1.x0 <= box2.xf && box1.xf >= box2.x0 && box1.y0 <= box2.yf && box1.yf >= box2.y0);
		}

		public static List<T> Intersect<T>(List<T> list1, List<T> list2)
		{
			List<T> result = new List<T>();
			foreach (T item in list1)
			{
				if (list2.Contains(item))
					result.Add(item);
			}
			return result;
		}
	}
}

