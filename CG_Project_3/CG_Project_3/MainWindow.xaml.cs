using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace CG_Project_3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap bitmap;
        private Modes mode;
        private bool firstClick = false;
        private bool polygonDrawing = false;
        private bool circlePointEdit = false;
        private bool movingPoint = false;

        private int Index1 = 0;
        private int Index2 = 0;
        private int polygonEditMode = 0; //0 - polygon vertex, 1 - polygon center, 2 - polygon edge
        private int lineEditMode = 0;    //0 - line start, 1 - line center, 2 - line end
        private Point lastPosition = new Point(-1,-1);
        private System.Windows.Media.Color lineColor = Colors.Black;

        private Colour editColor = new Colour(163, 163, 194);
        private Shape selectedShape = null;
        private Polygon currentPolygon = new Polygon();
        private List<Shape> shapes = new List<Shape>();
        private List<PixelPoint> editPoints = new List<PixelPoint>();
        
        public enum Modes
        {
            DrawingMode,
            EditMode,
            ThickMode,
            ColorMode,
            DeleteMode,
        }

        public MainWindow()
        {
            InitializeComponent();
            bitmap = new WriteableBitmap(
                (int)System.Windows.SystemParameters.PrimaryScreenWidth - 120,
                (int)System.Windows.SystemParameters.PrimaryScreenHeight,
                96,
                96,
                PixelFormats.Bgra32,
                BitmapPalettes.Halftone256);
            Image.Source = bitmap;
            ThicknessComboBox.ItemsSource = Enumerable.Range(1, 30).Where(i => i % 2 != 0).ToArray();
            ThicknessComboBox.SelectedIndex = 0;
        }

        private void Window_Size_Changed(object sender, SizeChangedEventArgs e)
        {

        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "Vector data|*.vd;";
            if (op.ShowDialog() == true)
            {
                FileStream fs = new FileStream(op.FileName, FileMode.Open);
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    shapes = (List<Shape>)formatter.Deserialize(fs);
                }
                catch (SerializationException error)
                {
                    MessageBox.Show("Loading return SerializationException " + error.Message, "Error");
                }
                finally
                {
                    fs.Close();
                }
                UpdateShapes();
                RedrawImage();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Vector data|*.vd";
            saveFileDialog1.Title = "Save an image";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    if (shapes != null)
                        formatter.Serialize(fs, shapes);
                }
                catch (SerializationException error)
                {
                    MessageBox.Show("Saving return SerializationException " + error.Message, "Error");
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            bitmap = new WriteableBitmap(
                (int)System.Windows.SystemParameters.PrimaryScreenWidth - 120,
                (int)System.Windows.SystemParameters.PrimaryScreenHeight,
                96,
                96,
                PixelFormats.Bgra32,
                BitmapPalettes.Halftone256);
            Image.Source = bitmap;
            shapes.Clear();
            editPoints.Clear();
            selectedShape = null;
            currentPolygon = new Polygon();
            UncheckAll();
        }

        private void Menu_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UncheckAll();
            
            if(!(ThicknessComboBox is null))
            {
                if(shapeComboBox.SelectedIndex == 2)
                {
                    ThicknessComboBox.IsEnabled = false;
                    ThickLabel.Foreground = new SolidColorBrush(Colors.DimGray);
                }
                else
                {
                    ThicknessComboBox.IsEnabled = true;
                    ThickLabel.Foreground = new SolidColorBrush(Colors.Black);
                }
            }
        }

        private void Draw_Mode_Checked(object sender, RoutedEventArgs e)
        {
            mode = Modes.DrawingMode;
            shapeComboBox.IsEnabled = true;
            if(shapeComboBox.SelectedIndex == 2)
            {
                ThicknessComboBox.IsEnabled = false;
                ThickLabel.Foreground = new SolidColorBrush(Colors.DimGray);
            }
            else
            {
                ThicknessComboBox.IsEnabled = true;
                ThickLabel.Foreground = new SolidColorBrush(Colors.Black);
            }
            UncheckAll();
            RedrawImage();
        }

        private void Edit_Mode_Checked(object sender, RoutedEventArgs e)
        {
            mode = Modes.EditMode;
            shapeComboBox.IsEnabled = false;
            ThicknessComboBox.IsEnabled = true;
            ThickLabel.Foreground = new SolidColorBrush(Colors.Black);
            UncheckAll();
            RedrawImage();
        }

        private void Thick_Mode_Checked(object sender, RoutedEventArgs e)
        {
            mode = Modes.ThickMode;
            shapeComboBox.IsEnabled = false;
            ThicknessComboBox.IsEnabled = true;
            ThickLabel.Foreground = new SolidColorBrush(Colors.Black);
            UncheckAll();
            RedrawImage();
        }

        private void Color_Mode_Checked(object sender, RoutedEventArgs e)
        {
            mode = Modes.ColorMode;
            shapeComboBox.IsEnabled = false;
            ThicknessComboBox.IsEnabled = true;
            ThickLabel.Foreground = new SolidColorBrush(Colors.Black);
            UncheckAll();
            RedrawImage();
        }

        private void Delete_Mode_Checked(object sender, RoutedEventArgs e)
        {
            mode = Modes.DeleteMode;
            shapeComboBox.IsEnabled = false;
            ThicknessComboBox.IsEnabled = true;
            ThickLabel.Foreground = new SolidColorBrush(Colors.Black);
            UncheckAll();
            RedrawImage();
        }

        private void Selected_Color_Changed(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (ColorPicker.SelectedColor.HasValue)
            {
                lineColor = ColorPicker.SelectedColor.Value;
            }
        }

        private void Anti_Aliasing_Changed(object sender, RoutedEventArgs e)
        {
            UpdateShapes();
            RedrawImage();
        }

        private void UncheckAll()
        {
            firstClick = false;
            circlePointEdit = false;
            lineEditMode = 0;
            polygonEditMode = 0;
            polygonDrawing = false;
            editPoints.Clear();
        }

        // ---------------------- Drawing Mechanisms ----------------------
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (mode)
            {
                case Modes.DrawingMode:
                    if (shapeComboBox.SelectedIndex == 0) DrawLine(e);
                    if (shapeComboBox.SelectedIndex == 1) DrawPolygon(e);
                    if (shapeComboBox.SelectedIndex == 2) DrawCircle(e);
                    break;
                case Modes.EditMode:
                    if (FindSelectedPoint(e))
                    {
                        movingPoint = true;
                        lastPosition = e.GetPosition(Image);
                    }
                    break;
                case Modes.ThickMode:
                    ChangeColorThickness(e, false);
                    break;
                case Modes.ColorMode:
                    ChangeColorThickness(e, true);
                    break;
                case Modes.DeleteMode:
                    DeleteShape(e);
                    break;
                default:
                    break;
            }
            //RedrawImage();
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            int x = (int)Math.Round(e.GetPosition(Image).X);
            int y = (int)Math.Round(e.GetPosition(Image).Y);
            if (mode == Modes.EditMode && movingPoint && selectedShape != null)
            {
                if (selectedShape is Circle)
                {
                    var s = selectedShape as Circle;
                    if (circlePointEdit)
                    {
                        s.Radius = (int)Distance(s.Origin, x, y);
                        s.AllPixels = DrawingAlgorithms.MidpointCircle(s.Origin.X, s.Origin.Y, s.Radius, s.shapeColor);
                    }
                    else
                    {
                        s.Origin = new PixelPoint(x, y, s.shapeColor);
                        s.AllPixels = DrawingAlgorithms.MidpointCircle(x, y, s.Radius, s.shapeColor);
                    }
                }
                if (selectedShape is Line)
                { 
                    var s = selectedShape as Line;
                    switch(lineEditMode)
                    {
                        case 0:
                            s.Start = new PixelPoint(x, y, s.shapeColor);
                            UpdateLine(s);
                            break;
                        case 1:
                            int xDiff = x - (int)lastPosition.X;
                            int yDiff = y - (int)lastPosition.Y;
                            s.Start = new PixelPoint(s.Start.X + xDiff, s.Start.Y + yDiff, s.shapeColor);
                            s.End = new PixelPoint(s.End.X + xDiff, s.End.Y + yDiff, s.shapeColor);
                            UpdateLine(s);
                            lastPosition.X = x;
                            lastPosition.Y = y;
                            break;
                        case 2:
                            s.End = new PixelPoint(x, y, s.shapeColor);
                            UpdateLine(s);
                            break;
                    }
                }
                if (selectedShape is Polygon)
                {
                    var s = selectedShape as Polygon;
                    switch(polygonEditMode)
                    {
                        case 0: //Polygon Vertex
                            s.Vertices[Index1] = new PixelPoint(x, y, s.shapeColor);
                            s.AllPixels.Clear();
                            UpdatePolygon(s);
                            break;
                        case 1: //Polygon Center
                            int xDiff = x - (int)lastPosition.X;
                            int yDiff = y - (int)lastPosition.Y;
                            for (int i = 0; i < s.Vertices.Count; i++)
                            {
                                s.Vertices[i].X += xDiff;
                                s.Vertices[i].Y += yDiff;
                            }
                            UpdatePolygon(s);
                            lastPosition.X = x;
                            lastPosition.Y = y;
                            break;
                        case 2: //Polygon Edge
                            int dx = x - (int)lastPosition.X;
                            int dy = y - (int)lastPosition.Y;
                            s.Vertices[Index1] = new PixelPoint(s.Vertices[Index1].X + dx, s.Vertices[Index1].Y + dy, s.shapeColor);
                            s.Vertices[Index2] = new PixelPoint(s.Vertices[Index2].X + dx, s.Vertices[Index2].Y + dy, s.shapeColor);
                            UpdatePolygon(s);
                            lastPosition.X = x;
                            lastPosition.Y = y;
                            break;
                    }
                }
                RedrawImage();
            }
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mode == Modes.EditMode && selectedShape != null)
            {
                movingPoint = false;
            }
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            if (mode == Modes.EditMode && selectedShape != null) movingPoint = false;
            if (shapeComboBox.SelectedIndex == 1) currentPolygon = new Polygon();
            UncheckAll();
            RedrawImage();
        }

        private void RedrawImage()
        {
            bitmap = new WriteableBitmap(
                (int)System.Windows.SystemParameters.PrimaryScreenWidth - 120,
                (int)System.Windows.SystemParameters.PrimaryScreenHeight,
                96,
                96,
                PixelFormats.Bgra32,
                BitmapPalettes.Halftone256);
            Image.Source = bitmap;
            editPoints.Clear();
            if (shapes.Count > 0)
            {
                foreach (Shape s in shapes)
                {
                    s.DrawPixels(bitmap);
                }
                if (mode == Modes.EditMode)
                {
                    FindEditPoints();
                    DrawEditPoints();
                }
                if (mode == Modes.DeleteMode || mode == Modes.ColorMode || mode == Modes.ThickMode)
                {
                    FindDeletePoints();
                    DrawEditPoints();
                }
            }
        }


        // --------------------- Drawing Shapes ------------------------
        private void DrawLine(MouseButtonEventArgs e)
        {
            int x = (int)Math.Round(e.GetPosition(Image).X);
            int y = (int)Math.Round(e.GetPosition(Image).Y);
            if (firstClick && lastPosition.X != -1)
            {
                int t = Convert.ToInt32(ThicknessComboBox.Text.ToString());
                Colour c = new Colour(lineColor.R, lineColor.G, lineColor.B);
                PixelPoint start = new PixelPoint((int)lastPosition.X, (int)lastPosition.Y, c);
                PixelPoint end = new PixelPoint(x, y, c);
                Line line = new Line(start, end, c, t);
                if (AntiAliasing.IsChecked == true)
                    line.AllPixels = DrawingAlgorithms.GuptaSproull(start.X, start.Y, end.X, end.Y, c, (double)t);
                else
                    line.AllPixels = DrawingAlgorithms.lineDDA(start.X, start.Y, end.X, end.Y, c, t);
                line.DrawPixels(bitmap);
                shapes.Add(line);
                firstClick = false;
                lastPosition.X = lastPosition.Y = -1;
            }
            else
            {
                firstClick = true;
                lastPosition.X = x;
                lastPosition.Y = y;
            }
        }

        private void DrawCircle(MouseButtonEventArgs e)
        {
            int x = (int)Math.Round(e.GetPosition(Image).X);
            int y = (int)Math.Round(e.GetPosition(Image).Y);
            if (firstClick && lastPosition.X != -1)
            {
                Colour c = new Colour(lineColor.R, lineColor.G, lineColor.B);
                int radius = (int)Math.Round(Math.Sqrt(Math.Pow(lastPosition.X - x, 2) + Math.Pow(lastPosition.Y - y, 2)));
                Circle circle = new Circle(new PixelPoint((int)lastPosition.X, (int)lastPosition.Y, c), radius, c);
                circle.AllPixels = DrawingAlgorithms.MidpointCircle((int)lastPosition.X, (int)lastPosition.Y, radius, c);
                circle.DrawPixels(bitmap);
                shapes.Add(circle);
                firstClick = false;
                lastPosition.X = lastPosition.Y = -1;
            }
            else
            {
                firstClick = true;
                lastPosition.X = x;
                lastPosition.Y = y;
            }
        }

        private void DrawPolygon(MouseButtonEventArgs e)
        {
            int x = (int)Math.Round(e.GetPosition(Image).X);
            int y = (int)Math.Round(e.GetPosition(Image).Y);
            if (firstClick && !polygonDrawing)
            {
                int t = Convert.ToInt32(ThicknessComboBox.Text.ToString());
                Colour c = new Colour(lineColor.R, lineColor.G, lineColor.B);
                currentPolygon.SetThickness(t);
                currentPolygon.shapeColor = c;
                currentPolygon.AddVertex(new PixelPoint((int)lastPosition.X, (int)lastPosition.Y, c));
                currentPolygon.AddVertex(new PixelPoint(x, y, c));
                if(AntiAliasing.IsChecked == true)
                    currentPolygon.AllPixels = DrawingAlgorithms.GuptaSproull((int)lastPosition.X, (int)lastPosition.Y, x, y, c, (double)t);
                else
                    currentPolygon.AllPixels = DrawingAlgorithms.lineDDA((int)lastPosition.X, (int)lastPosition.Y, x, y, c, t);
                currentPolygon.DrawPixels(bitmap);
                polygonDrawing = true;
                lastPosition.X = x;
                lastPosition.Y = y;
            }
            else if (firstClick && polygonDrawing)
            {
                PixelPoint start = currentPolygon.Vertices[0];
                if (x > start.X - 20 && x < start.X + 20 && y > start.Y - 20 && y < start.Y + 20)
                {
                    if (AntiAliasing.IsChecked == true)
                        currentPolygon.AllPixels = currentPolygon.AllPixels.Union(DrawingAlgorithms.GuptaSproull((int)lastPosition.X,
                        (int)lastPosition.Y, start.X, start.Y, start.MyColor, (double)currentPolygon.GetThickness())).ToList();
                    else
                        currentPolygon.AllPixels = currentPolygon.AllPixels.Union(DrawingAlgorithms.lineDDA((int)lastPosition.X,
                        (int)lastPosition.Y, start.X, start.Y, start.MyColor, currentPolygon.GetThickness())).ToList();
                    currentPolygon.DrawPixels(bitmap);
                    shapes.Add(currentPolygon);
                    firstClick = false;
                    polygonDrawing = false;
                    currentPolygon = new Polygon();
                    lastPosition.X = lastPosition.Y = -1;
                }
                else
                {
                    int t = Convert.ToInt32(ThicknessComboBox.Text.ToString());
                    Colour c = new Colour(lineColor.R, lineColor.G, lineColor.B);
                    currentPolygon.SetThickness(t);
                    currentPolygon.AddVertex(new PixelPoint(x, y, c));
                    if(AntiAliasing.IsChecked == true)
                        currentPolygon.AllPixels = currentPolygon.AllPixels.Union(DrawingAlgorithms.GuptaSproull((int)lastPosition.X, (int)lastPosition.Y, x, y, c, (double)t)).ToList();
                    else
                        currentPolygon.AllPixels = currentPolygon.AllPixels.Union(DrawingAlgorithms.lineDDA((int)lastPosition.X, (int)lastPosition.Y, x, y, c, t)).ToList();
                    currentPolygon.DrawPixels(bitmap);
                    lastPosition.X = x;
                    lastPosition.Y = y;
                }
            }
            else
            {
                firstClick = true;
                lastPosition.X = x;
                lastPosition.Y = y;
            }
        }


        // ----------------------- Moving Shapes -----------------------
        private void UpdatePolygon(Polygon s)
        {
            s.AllPixels.Clear();
            foreach (PixelPoint pp in s.Vertices)
            {
                if (s.Vertices.IndexOf(pp) == s.Vertices.Count - 1)
                {
                    if (AntiAliasing.IsChecked == true)
                        s.AllPixels = s.AllPixels.Union(DrawingAlgorithms.GuptaSproull(pp.X, pp.Y, s.Vertices[0].X, s.Vertices[0].Y, s.shapeColor, (double)s.GetThickness())).ToList();
                    else
                        s.AllPixels = s.AllPixels.Union(DrawingAlgorithms.lineDDA(pp.X, pp.Y, s.Vertices[0].X, s.Vertices[0].Y, s.shapeColor, s.GetThickness())).ToList();
                }
                else
                {
                    int idx = s.Vertices.IndexOf(pp);
                    if (AntiAliasing.IsChecked == true)
                        s.AllPixels = s.AllPixels.Union(DrawingAlgorithms.GuptaSproull(pp.X, pp.Y, s.Vertices.ElementAt(idx + 1).X,
                        s.Vertices.ElementAt(idx + 1).Y, s.shapeColor, (double)s.GetThickness())).ToList();
                    else
                        s.AllPixels = s.AllPixels.Union(DrawingAlgorithms.lineDDA(pp.X, pp.Y, s.Vertices.ElementAt(idx + 1).X,
                        s.Vertices.ElementAt(idx + 1).Y, s.shapeColor, s.GetThickness())).ToList();
                }
            }
        }

        private void UpdateLine(Line s)
        {
            if (AntiAliasing.IsChecked == true)
                s.AllPixels = DrawingAlgorithms.GuptaSproull(s.Start.X, s.Start.Y, s.End.X, s.End.Y, s.shapeColor, (double)s.GetThickness());
            else
                s.AllPixels = DrawingAlgorithms.lineDDA(s.Start.X, s.Start.Y, s.End.X, s.End.Y, s.shapeColor, s.GetThickness());
        }

        private void UpdateShapes()
        {
            foreach (Shape shape in shapes)
            {
                if (shape is Circle) continue;
                if (shape is Polygon) UpdatePolygon(shape as Polygon);
                if (shape is Line) UpdateLine(shape as Line);
            }
        }

        private void AddEditPoint(PixelPoint pixel, int chunk)
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

        private bool isNearPoint(PixelPoint p1, int x, int y)
        {
            if (x > p1.X - 20 && x < p1.X + 20 && y > p1.Y - 20 && y < p1.Y + 20)
                return true;
            return false;
        }

        private double Distance(PixelPoint p1, int x, int y)
        {
            return Math.Sqrt(Math.Pow(p1.X - x, 2) + Math.Pow(p1.Y - y, 2));
        }

        private bool FindSelectedPoint(MouseButtonEventArgs e)
        {
            int x = (int)Math.Round(e.GetPosition(Image).X);
            int y = (int)Math.Round(e.GetPosition(Image).Y);
            foreach (Shape shape in shapes)
            {
                if (shape is Line)
                {
                    var s = shape as Line;
                    PixelPoint center = new PixelPoint((s.Start.X + s.End.X) / 2, (s.Start.Y + s.End.Y) / 2, s.shapeColor);
                    if (isNearPoint(s.Start, x, y))
                    {
                        lineEditMode = 0;
                        selectedShape = s;
                        return true;
                    }
                    if (isNearPoint(center, x, y))
                    {
                        lineEditMode = 1;
                        selectedShape = s;
                        return true;
                    }
                    if (isNearPoint(s.End, x, y))
                    {
                        lineEditMode = 2;
                        selectedShape = s;
                        return true;
                    }
                }
                else if (shape is Polygon)
                {
                    var s = shape as Polygon;
                    var (cx, cy) = s.Center();
                    PixelPoint center = new PixelPoint(cx, cy, editColor);
                    if (isNearPoint(center, x, y))
                    {
                        polygonEditMode = 1;
                        selectedShape = s;
                        return true;
                    }
                    foreach(PixelPoint v in s.Vertices)
                    {
                        int idx = s.Vertices.IndexOf(v);
                        if (isNearPoint(v, x, y))
                        {
                            polygonEditMode = 0;
                            Index1 = s.Vertices.IndexOf(v);
                            selectedShape = s;
                            return true;
                        }
                        else
                        {
                            PixelPoint tmp;
                            if (idx == s.Vertices.Count - 1) tmp = new PixelPoint((v.X + s.Vertices[0].X) / 2, (v.Y + s.Vertices[0].Y) / 2, s.shapeColor);
                            else tmp = new PixelPoint((v.X + s.Vertices[idx + 1].X) / 2, (v.Y + s.Vertices[idx + 1].Y) / 2, s.shapeColor);
                            if (isNearPoint(tmp, x, y))
                            {
                                polygonEditMode = 2;
                                Index1 = idx;
                                Index2 = idx + 1 > s.Vertices.Count - 1 ? 0 : idx + 1;
                                selectedShape = s;
                                return true;
                            }
                        }
                    }
                }
                else if (shape is Circle)
                {
                    var s = shape as Circle;
                    if (isNearPoint(s.Origin, x, y))
                    {
                        circlePointEdit = false;
                        selectedShape = s;
                        return true;
                    }
                    if (Distance(s.Origin, x, y) > s.Radius - 10 && Distance(s.Origin, x, y) < s.Radius + 10)
                    {
                        circlePointEdit = true;
                        selectedShape = s;
                        return true;
                    }
                }
            }
            return false;
        }

        private void FindEditPoints()
        {
            foreach(Shape shape in shapes)
            {
                if (shape is Circle)
                {
                    var s = shape as Circle;
                    PixelPoint tmp = new PixelPoint(s.Origin.X, s.Origin.Y, editColor);
                    editPoints.Add(tmp);
                    AddEditPoint(tmp, 21);
                }
                else if (shape is Polygon)
                {
                    var s = shape as Polygon;
                    var (x, y) = s.Center();
                    PixelPoint pp = new PixelPoint(x, y, editColor);
                    editPoints.Add(pp);
                    AddEditPoint(pp, 21);
                    foreach (PixelPoint v in s.Vertices)
                    {
                        int idx = s.Vertices.IndexOf(v);
                        PixelPoint edgeCenter;
                        if (idx == s.Vertices.Count - 1) edgeCenter = new PixelPoint((v.X + s.Vertices[0].X) / 2, (v.Y + s.Vertices[0].Y) / 2, editColor);
                        else edgeCenter = new PixelPoint((v.X + s.Vertices[idx + 1].X) /2, (v.Y + s.Vertices[idx + 1].Y) / 2, editColor);
                        editPoints.Add(edgeCenter);
                        AddEditPoint(edgeCenter, 21);
                        PixelPoint vertex = new PixelPoint(v.X, v.Y, editColor);
                        editPoints.Add(vertex);
                        AddEditPoint(vertex, 21);
                    }
                }
                else if (shape is Line)
                {
                    var s = shape as Line;
                    PixelPoint p1 = new PixelPoint(s.Start.X, s.Start.Y, editColor);
                    PixelPoint p2 = new PixelPoint(s.End.X, s.End.Y, editColor);
                    PixelPoint center = new PixelPoint((s.Start.X + s.End.X) / 2, (s.Start.Y + s.End.Y) / 2, editColor);
                    editPoints.Add(p1);
                    AddEditPoint(p1, 21);
                    editPoints.Add(p2);
                    AddEditPoint(p2, 21);
                    editPoints.Add(center);
                    AddEditPoint(center, 21);
                }
            }
        }

        private void DrawEditPoints()
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
       

        // --------------------- Deleting Shapes ----------------------
        private void FindDeletePoints()
        {
            foreach (Shape shape in shapes)
            {
                if (shape is Circle)
                {
                    var s = shape as Circle;
                    PixelPoint tmp = new PixelPoint(s.Origin.X, s.Origin.Y, editColor);
                    editPoints.Add(tmp);
                    AddEditPoint(tmp, 21);
                }
                else if (shape is Polygon)
                {
                    var s = shape as Polygon;
                    var (x, y) = s.Center();
                    PixelPoint pp = new PixelPoint(x, y, editColor);
                    editPoints.Add(pp);
                    AddEditPoint(pp, 21);
                }
                else if (shape is Line)
                {
                    var s = shape as Line;
                    int x = (s.Start.X + s.End.X) / 2;
                    int y = (s.Start.Y + s.End.Y) / 2;
                    PixelPoint p1 = new PixelPoint(x, y, editColor);
                    editPoints.Add(p1);
                    AddEditPoint(p1, 21);
                }
            }
        }

        private Shape GetClickedShape(MouseButtonEventArgs e)
        {
            int x = (int)Math.Round(e.GetPosition(Image).X);
            int y = (int)Math.Round(e.GetPosition(Image).Y);
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
            }
            return null;
        }

        private void DeleteShape(MouseButtonEventArgs e)
        {
            Shape toDelete = GetClickedShape(e);
            if (toDelete != null)
            {
                shapes.Remove(toDelete);
                RedrawImage();
            }
        }

        private void ChangeColorThickness(MouseButtonEventArgs e, bool color)
        {
            Shape thick = GetClickedShape(e);
            if (thick != null)
            {
                if (color == true)
                {
                    Colour c = new Colour(lineColor.R, lineColor.G, lineColor.B);
                    thick.shapeColor = c;
                }
                else
                {
                    int t = Convert.ToInt32(ThicknessComboBox.Text.ToString());
                    thick.SetThickness(t);
                }
                if (thick is Line) UpdateLine(thick as Line);
                if (thick is Polygon) UpdatePolygon(thick as Polygon);
                if (thick is Circle)
                {
                    if (color == false) return;
                    var s = thick as Circle;
                    s.AllPixels = DrawingAlgorithms.MidpointCircle(s.Origin.X, s.Origin.Y, s.Radius, s.shapeColor);
                }
                RedrawImage();
            }
        }

    }
}
