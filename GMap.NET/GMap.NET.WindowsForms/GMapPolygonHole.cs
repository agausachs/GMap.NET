using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMap.NET.WindowsForms
{
    public class GMapPolygonHole : GMapPolygon
    {
        public GMapPolygonHole(List<PointLatLng> outerPoints, string name) : base(outerPoints, name)
        {
            CalcBoundingBox();
        }

        public List<List<PointLatLng>> ListPointHoles = new List<List<PointLatLng>>();

        public int NumberOfHoles => ListPointHoles.Count;

        public double MinLatitude { get; private set; }
        public double MaxLatitude { get; private set; }
        public double MinLongitude { get; private set; }
        public double MaxLongitude { get; private set; }

        public List<List<GPoint>> ListPointsLocalHoles = new List<List<GPoint>>();

        public void CalcBoundingBox()
        {
            if (this.Points.Count == 0) return;
            MinLatitude = this.Points.Min(x => x.Lat);
            MaxLatitude = this.Points.Max(x => x.Lat);

            MinLongitude = this.Points.Min(x => x.Lng);
            MaxLongitude = this.Points.Max(x => x.Lng);
        }

        public void AddNormalHole(GMapPolygon holePoly)
        {
            this.ListPointHoles.Add(holePoly.Points);
        }

        public override void UpdateGraphicsPath()
        {
            if (_graphicsPath == null)
            {
                _graphicsPath = new GraphicsPath();
            }
            else
            {
                _graphicsPath.Reset();
            }
            //Add Main Polygon first:
            Point[] pnts = new Point[LocalPoints.Count];
            for (int i = 0; i < LocalPoints.Count; i++)
            {
                Point p2 = new Point((int)LocalPoints[i].X, (int)LocalPoints[i].Y);
                pnts[pnts.Length - 1 - i] = p2;
            }

            if (pnts.Length > 2)
            {
                _graphicsPath.AddPolygon(pnts);

                //Add All Holes:
                foreach (var holeLocalPoints in ListPointsLocalHoles)
                {
                    pnts = new Point[holeLocalPoints.Count];
                    for (int i = 0; i < holeLocalPoints.Count; i++)
                    {
                        Point p2 = new Point((int)holeLocalPoints[i].X, (int)holeLocalPoints[i].Y);
                        pnts[pnts.Length - 1 - i] = p2;
                    }
                    if (pnts.Length > 2)
                    {
                        _graphicsPath.AddPolygon(pnts);
                    }
                }
            }
            else if (pnts.Length > 0)
            {
                _graphicsPath.AddLines(pnts);
            }
        }

        public void AddNormalHole(List<PointLatLng> points)
        {
            this.ListPointHoles.Add(points);
        }

        public bool IsPointInsidePolygon(PointLatLng p, bool borderIsPartOfPolygon)
        {
            //Border belongs to Polygon!
            if (borderIsPartOfPolygon && Points.Contains(p))
            {
                return true;
            }

            //Check if point is within the Bounding Box
            if (p.Lng < MinLongitude || p.Lng > MaxLongitude || p.Lat < MinLatitude || p.Lng > MaxLatitude)
            {
                // Definitely not within the polygon!
                return false;
            }

            //Ray-cast algorithm is here onward
            int k, j = Points.Count - 1;
            bool oddNodes = false; //to check whether number of intersections is odd
            for (k = 0; k < Points.Count; k++)
            {
                //fetch adjacent points of the polygon
                PointLatLng polyK = Points[k];
                PointLatLng polyJ = Points[j];

                //check the intersections
                if (polyJ.Lat < p.Lat && polyK.Lat >= p.Lat || polyK.Lat < p.Lat && polyJ.Lat >= p.Lat)
                {
                    if (polyJ.Lng + (p.Lat - polyJ.Lat) / (polyK.Lat - polyJ.Lat) * (polyK.Lng - polyJ.Lng) < p.Lng)
                    {
                        oddNodes = !oddNodes;
                    }
                }

                j = k;
            }

            //Now Test all Holes:
            foreach (var hole in this.ListPointHoles)
            {
                j = hole.Count - 1;
                for (k = 0; k < hole.Count; k++)
                {
                    //fetch adjacent points of the polygon
                    PointLatLng polyK = hole[k];
                    PointLatLng polyJ = hole[j];

                    //check the intersections
                    if (polyJ.Lat < p.Lat && polyK.Lat >= p.Lat || polyK.Lat < p.Lat && polyJ.Lat >= p.Lat)
                    {
                        if (polyJ.Lng + (p.Lat - polyJ.Lat) / (polyK.Lat - polyJ.Lat) * (polyK.Lng - polyJ.Lng) < p.Lng)
                        {
                            oddNodes = !oddNodes;
                        }
                    }
                    j = k;
                }
            }

            return oddNodes;
        }
    }
}
