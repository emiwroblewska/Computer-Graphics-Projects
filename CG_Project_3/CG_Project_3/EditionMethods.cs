using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;

namespace CG_Project_3
{
    public static class EditionMethods
    {
        public static bool isNearPoint(PixelPoint p1, int x, int y)
        {
            if (x > p1.X - 20 && x < p1.X + 20 && y > p1.Y - 20 && y < p1.Y + 20)
                return true;
            return false;
        }

        public static double Distance(PixelPoint p1, int x, int y)
        {
            return Math.Sqrt(Math.Pow(p1.X - x, 2) + Math.Pow(p1.Y - y, 2));
        }


        // ------------------ Updating Shapes ----------------
        public static void UpdatePolygon(Polygon s, bool antiAliasing)
        {
            s.AllPixels.Clear();
            foreach (PixelPoint pp in s.Vertices)
            {
                if (s.Vertices.IndexOf(pp) == s.Vertices.Count - 1)
                {
                    if (antiAliasing == true)
                        s.AllPixels = s.AllPixels.Union(DrawingAlgorithms.GuptaSproull(pp.X, pp.Y, s.Vertices[0].X, s.Vertices[0].Y, s.shapeColor, (double)s.GetThickness())).ToList();
                    else
                        s.AllPixels = s.AllPixels.Union(DrawingAlgorithms.lineDDA(pp.X, pp.Y, s.Vertices[0].X, s.Vertices[0].Y, s.shapeColor, s.GetThickness())).ToList();
                }
                else
                {
                    int idx = s.Vertices.IndexOf(pp);
                    if (antiAliasing == true)
                        s.AllPixels = s.AllPixels.Union(DrawingAlgorithms.GuptaSproull(pp.X, pp.Y, s.Vertices.ElementAt(idx + 1).X,
                        s.Vertices.ElementAt(idx + 1).Y, s.shapeColor, (double)s.GetThickness())).ToList();
                    else
                        s.AllPixels = s.AllPixels.Union(DrawingAlgorithms.lineDDA(pp.X, pp.Y, s.Vertices.ElementAt(idx + 1).X,
                        s.Vertices.ElementAt(idx + 1).Y, s.shapeColor, s.GetThickness())).ToList();
                }
            }
        }

        public static void UpdateLine(Line s, bool antiAliasing)
        {
            if (antiAliasing == true)
                s.AllPixels = DrawingAlgorithms.GuptaSproull(s.Start.X, s.Start.Y, s.End.X, s.End.Y, s.shapeColor, (double)s.GetThickness());
            else
                s.AllPixels = DrawingAlgorithms.lineDDA(s.Start.X, s.Start.Y, s.End.X, s.End.Y, s.shapeColor, s.GetThickness());
        }

        public static void UpdateShapes(List<Shape> shapes, bool antiAliasing)
        {
            foreach (Shape shape in shapes)
            {
                if (shape is Polygon) UpdatePolygon(shape as Polygon, antiAliasing);
                if (shape is Line) UpdateLine(shape as Line, antiAliasing);
                if (shape is Circle)
                {
                    var s = shape as Circle;
                    s.AllPixels = DrawingAlgorithms.MidpointCircle(s.Origin.X, s.Origin.Y, s.Radius, s.shapeColor);
                }
                if (shape is Capsule)
                {
                    var s = shape as Capsule;
                    s.AllPixels = DrawingAlgorithms.Capsule(s.OriginA.X, s.OriginA.Y, s.OriginB.X, s.OriginB.Y, s.Radius, s.shapeColor);
                }
            }
        }

        // ---------------- Add edition points ----------------
        private static void AddEditPoint(List<PixelPoint> editPoints, PixelPoint pixel, int chunk)
        {
            int size = (chunk - 1) / 2;
            for (int i = -size; i <= size; i++)
            {
                for (int j = -size; j <= size; j++)
                {
                    if (Math.Sqrt(Math.Pow(i, 2) + Math.Pow(j, 2)) < (double)size)
                    {
                        editPoints.Add(new PixelPoint(pixel.X + i, pixel.Y + j, pixel.MyColor));
                    }
                }
            }
        }

        public static void FindEditPoints(List<Shape> shapes, List<PixelPoint> editPoints, Colour editColor)
        {
            foreach (Shape shape in shapes)
            {
                if (shape is Circle)
                {
                    var s = shape as Circle;
                    PixelPoint tmp = new PixelPoint(s.Origin.X, s.Origin.Y, editColor);
                    editPoints.Add(tmp);
                    AddEditPoint(editPoints, tmp, 21);
                }
                else if (shape is Polygon)
                {
                    var s = shape as Polygon;
                    var (x, y) = s.Center();
                    PixelPoint pp = new PixelPoint(x, y, editColor);
                    editPoints.Add(pp);
                    AddEditPoint(editPoints, pp, 21);
                    foreach (PixelPoint v in s.Vertices)
                    {
                        int idx = s.Vertices.IndexOf(v);
                        PixelPoint edgeCenter;
                        if (idx == s.Vertices.Count - 1) edgeCenter = new PixelPoint((v.X + s.Vertices[0].X) / 2, (v.Y + s.Vertices[0].Y) / 2, editColor);
                        else edgeCenter = new PixelPoint((v.X + s.Vertices[idx + 1].X) / 2, (v.Y + s.Vertices[idx + 1].Y) / 2, editColor);
                        editPoints.Add(edgeCenter);
                        AddEditPoint(editPoints, edgeCenter, 21);
                        PixelPoint vertex = new PixelPoint(v.X, v.Y, editColor);
                        editPoints.Add(vertex);
                        AddEditPoint(editPoints, vertex, 21);
                    }
                }
                else if (shape is Line)
                {
                    var s = shape as Line;
                    PixelPoint p1 = new PixelPoint(s.Start.X, s.Start.Y, editColor);
                    PixelPoint p2 = new PixelPoint(s.End.X, s.End.Y, editColor);
                    PixelPoint center = new PixelPoint((s.Start.X + s.End.X) / 2, (s.Start.Y + s.End.Y) / 2, editColor);
                    editPoints.Add(p1);
                    AddEditPoint(editPoints, p1, 21);
                    editPoints.Add(p2);
                    AddEditPoint(editPoints, p2, 21);
                    editPoints.Add(center);
                    AddEditPoint(editPoints, center, 21);
                }
                else if (shape is Capsule)
                {
                    var s = shape as Capsule;
                    PixelPoint p1 = new PixelPoint(s.OriginA.X, s.OriginA.Y, editColor);
                    PixelPoint p2 = new PixelPoint(s.OriginB.X, s.OriginB.Y, editColor);
                    PixelPoint center = new PixelPoint((s.OriginA.X + s.OriginB.X) / 2, (s.OriginA.Y + s.OriginB.Y) / 2, editColor);
                    editPoints.Add(p1);
                    AddEditPoint(editPoints, p1, 21);
                    editPoints.Add(p2);
                    AddEditPoint(editPoints, p2, 21);
                    editPoints.Add(center);
                    AddEditPoint(editPoints, center, 21);
                }
            }
        }

        // --------------------- Find Deletion Points ----------------------
        public static void FindDeletePoints(List<Shape> shapes, List<PixelPoint> editPoints, Colour editColor)
        {
            foreach (Shape shape in shapes)
            {
                if (shape is Circle)
                {
                    var s = shape as Circle;
                    PixelPoint tmp = new PixelPoint(s.Origin.X, s.Origin.Y, editColor);
                    editPoints.Add(tmp);
                    AddEditPoint(editPoints, tmp, 21);
                }
                else if (shape is Polygon)
                {
                    var s = shape as Polygon;
                    var (x, y) = s.Center();
                    PixelPoint pp = new PixelPoint(x, y, editColor);
                    editPoints.Add(pp);
                    AddEditPoint(editPoints, pp, 21);
                }
                else if (shape is Line)
                {
                    var s = shape as Line;
                    int x = (s.Start.X + s.End.X) / 2;
                    int y = (s.Start.Y + s.End.Y) / 2;
                    PixelPoint p1 = new PixelPoint(x, y, editColor);
                    editPoints.Add(p1);
                    AddEditPoint(editPoints, p1, 21);
                }
                else if (shape is Capsule)
                {
                    var s = shape as Capsule;
                    PixelPoint p1 = new PixelPoint((s.OriginA.X + s.OriginB.X) / 2, (s.OriginA.Y + s.OriginB.Y) / 2, editColor);
                    editPoints.Add(p1);
                    AddEditPoint(editPoints, p1, 21);
                }
            }
        }

        public static Shape GetClickedShape(List<Shape> shapes, int x, int y)
        {
            foreach (Shape shape in shapes)
            {
                if (shape is Circle)
                {
                    var s = shape as Circle;
                    if (isNearPoint(s.Origin, x, y)) return s;
                }
                else if (shape is Polygon)
                {
                    var s = shape as Polygon;
                    var (cx, cy) = s.Center();
                    if (isNearPoint(new PixelPoint(cx, cy, s.shapeColor), x, y)) return s;
                }
                else if (shape is Line)
                {
                    var s = shape as Line;
                    PixelPoint pp = new PixelPoint((s.Start.X + s.End.X) / 2, (s.Start.Y + s.End.Y) / 2, s.shapeColor);
                    if (isNearPoint(pp, x, y)) return s;
                }
                else if (shape is Capsule)
                {
                    var s = shape as Capsule;
                    PixelPoint pp = new PixelPoint((s.OriginA.X + s.OriginB.X) / 2, (s.OriginA.Y + s.OriginB.Y) / 2, s.shapeColor);
                    if (isNearPoint(pp, x, y)) return s;
                }
            }
            return null;
        }

        // --------------- Drawing Edit Points ----------------
        public static void DrawEditPoints(WriteableBitmap bitmap, List<PixelPoint> editPoints)
        {
            try
            {
                bitmap.Lock();
                foreach (PixelPoint pp in editPoints)
                {
                    int column = pp.X;
                    int row = pp.Y;
                    if (row >= 0 && column >= 0 && row < ((int)bitmap.PixelHeight - 1) && column < (int)bitmap.PixelWidth - 1)
                    {
                        unsafe
                        {
                            IntPtr pBackBuffer = bitmap.BackBuffer;
                            pBackBuffer += row * bitmap.BackBufferStride;
                            pBackBuffer += column * 4;

                            int color_data = 0;
                            color_data |= 255 << 24;           // A
                            color_data |= pp.MyColor.R << 16;  // R
                            color_data |= pp.MyColor.G << 8;   // G
                            color_data |= pp.MyColor.B << 0;   // B

                            *((int*)pBackBuffer) = color_data;
                        }
                        bitmap.AddDirtyRect(new Int32Rect(column, row, 1, 1));
                    }
                }
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        
    }
}
