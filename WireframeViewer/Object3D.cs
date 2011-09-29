using System;
using System.Collections.Generic;
using System.Drawing;

namespace WireframeViewer
{
    // could use Point for this, but 1) that would be misleading since 
    // these values are indices, not x,y coordinates; and 2) we can add
    // to this struct (e.g., if we want each edge to have its own color)
    struct EndPoints
    {
        public int p1;
        public int p2;

        public EndPoints(int one, int two)
        {
            p1 = one;
            p2 = two;
        }
    }

    struct PointPair
    {
        public Point p1;
        public Point p2;

        public PointPair(Point one, Point two)
        {
            p1 = one;
            p2 = two;
        }
    }

    // for 3D coordinates
    struct Point3D
    {
        public double X;
        public double Y;
        public double Z;

        public Point3D(double eX, double wY, double Ze)
        {
            X = eX;
            Y = wY;
            Z = Ze;
        }
    }

    public class Object3D
    {
        List<Point3D> vertex;
        List<EndPoints> edge;
        // bounding box 
        double minX, minY, minZ, maxX, maxY, maxZ;
        // used for projection from 3D to 2D
        double eyeZ, screenZ;
        double zoomMin, zoomMax, zoomRange;

        public Object3D(double Xmin, double Xmax, double Ymin, double Ymax,
            double Zmin, double Zmax, double minZoom, double maxZoom)
        {
            vertex = new List<Point3D>();
            edge = new List<EndPoints>();
            SetBoundingBox(Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
            zoomMin = minZoom;
            zoomMax = maxZoom;
            // TO DO: make these values adjustable
            eyeZ = -20.0;
            screenZ = -10.0;
            // the range of Z values that will map to (maxZoom - minZoom)
            zoomRange = 10.0;
        }

        public void SetBoundingBox(double Xmin, double Xmax, double Ymin, double Ymax,
            double Zmin, double Zmax)
        {
            minX = Xmin;
            maxX = Xmax;
            minY = Ymin;
            maxY = Ymax;
            minZ = Zmin;
            maxZ = Zmax;
        }

        public void AddVertex(double x, double y, double z)
        {
            vertex.Add(new Point3D(x, y, z));
        }

        public void AddEdge(int v1, int v2)
        {
            edge.Add(new EndPoints(v1, v2));
        }

        public void Draw(Graphics g, Rectangle viewport, int rho, int phi, int theta, int zoom)
        {
            // TO DO: use matrix multiplication
            List<Point3D> newVertex = RotateX(rho);
            newVertex = RotateY(phi, newVertex);
            newVertex = RotateZ(theta, newVertex);
            List<PointPair> lines = Project2D(newVertex, viewport, zoom);
            // TO DO: user-defined color, or defined as part of object
            Pen p = Pens.Black;
            foreach (PointPair endPts in lines)
            {
                g.DrawLine(p, endPts.p1, endPts.p2);
            }
        }

        // TO DO: use matrix multiplication, combine these 3 similar methods into 1
        
        // clockwise rotation about the X axis
        private List<Point3D> RotateX(int degrees)
        {
            double radians = Math.PI * degrees / 180;
            double s = Math.Sin(radians);
            double c = Math.Cos(radians);
            List<Point3D> newVertex = new List<Point3D>();
            foreach (Point3D v in vertex)
            {
                double newZ = v.Z * c - v.Y * s;
                double newY = v.Z * s + v.Y * c;
                newVertex.Add(new Point3D(v.X, newY, newZ));
            }
            return newVertex;
        }

        // clockwise rotation about the Y axis
        private List<Point3D> RotateY(int degrees, List<Point3D> vertIn)
        {
            double radians = Math.PI * degrees / 180;
            double s = Math.Sin(radians);
            double c = Math.Cos(radians);
            List<Point3D> newVertex = new List<Point3D>();
            foreach (Point3D v in vertIn)
            {
                double newX = v.X * c - v.Z * s;
                double newZ = v.X * s + v.Z * c;
                newVertex.Add(new Point3D(newX, v.Y, newZ));
            }
            return newVertex;
        }

        // clockwise rotation about the Z axis
        private List<Point3D> RotateZ(int degrees, List<Point3D> vertIn)
        {
            double radians = Math.PI * degrees / 180;
            double s = Math.Sin(radians);
            double c = Math.Cos(radians);
            List<Point3D> newVertex = new List<Point3D>();
            foreach (Point3D v in vertIn)
            {
                double newX = v.X * c - v.Y * s;
                double newY = v.X * s + v.Y * c;
                newVertex.Add(new Point3D(newX, newY, v.Z));
            }
            return newVertex;
        }

        // project from 3D to 2D, return a list of paired screen coordinates,
        // the endpoints of the lines to draw
        private List<PointPair> Project2D(List<Point3D> newVertex, Rectangle viewport, int zoom)
        {
            List<PointPair> lineList = new List<PointPair>();
            List<Point> vertex2D = new List<Point>();
            double eyeDistance = MapZoomToEyeDistance(zoom);
            foreach (Point3D pt in newVertex)
            {
                // project the vertices to 2D
                double x = pt.X * screenZ / (eyeDistance - pt.Z);
                double y = pt.Y * screenZ / (eyeDistance - pt.Z);
                // map to screen coords
                int screen_x = ViewPortX(x, viewport);
                int screen_y = ViewPortY(y, viewport);
                vertex2D.Add(new Point(screen_x, screen_y));
            }
            // create the list of lines to draw
            foreach (EndPoints pt in edge)
            {
                lineList.Add(new PointPair(new Point(vertex2D[pt.p1].X, vertex2D[pt.p1].Y),
                    new Point(vertex2D[pt.p2].X, vertex2D[pt.p2].Y)));
            }
            return lineList;
        }

        // TO DO: efficiency (eliminate repeated caclulations, etc.)
        private int ViewPortX(double x, Rectangle viewport)
        {
            // preserve object aspect ratio, regardless of viewport aspect ratio
            int smallerDim = (viewport.Width > viewport.Height) ? viewport.Height : viewport.Width;
            return viewport.Left + (int)(((x - minX) / (maxX - minX))*(smallerDim));
        }

        private int ViewPortY(double y, Rectangle viewport)
        {
            // to preserve object aspect ratio
            int smallerDim = (viewport.Width > viewport.Height) ? viewport.Height : viewport.Width;
            // note: assumes viewport coords are smaller at top (as usual with computer screen coords)
            return viewport.Top + (int)(((maxY - y) / (maxY - minY))*(smallerDim));
        }

        // TO DO: move this out to the caller, have users pass 0.0 - 1.0 range for zoom
        private double MapZoomToEyeDistance(int zoom)
        {
            return (zoom - zoomMin)/(zoomMax-zoomMin)*zoomRange + eyeZ;
        }

        public void Load(string name)
        {
            // TO DO: load object from disk
            throw new Exception("not yet implemented");
        }

        public void Save(string name)
        {
            // TO DO: save object to disk
            throw new Exception("not yet implemented");
        }
    }
}
