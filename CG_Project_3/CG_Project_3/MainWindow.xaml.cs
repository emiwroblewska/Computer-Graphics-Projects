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
        private bool capsuleDrawing = false;
        private bool circlePointEdit = false;
        private bool movingPoint = false;

        private int Index1 = 0;
        private int Index2 = 0;
        private int polygonEditMode = 0; //0 - polygon vertex, 1 - polygon center, 2 - polygon edge
        private int lineEditMode = 0;    //0 - line start, 1 - line center, 2 - line end
        private int capsuleEditMode = 0; //0 - capsule origin A, 1 - capsule center, 2 - capsule origin B, 3 - capsule radius
        private Point firstPosition = new Point(-1, -1);
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
                EditionMethods.UpdateShapes(shapes, AntiAliasing.IsChecked == true);
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
        }

        private void Draw_Mode_Checked(object sender, RoutedEventArgs e)
        {
            mode = Modes.DrawingMode;
            shapeComboBox.IsEnabled = true;
            UncheckAll();
            RedrawImage();
        }

        private void Edit_Mode_Checked(object sender, RoutedEventArgs e)
        {
            mode = Modes.EditMode;
            shapeComboBox.IsEnabled = false;
            UncheckAll();
            RedrawImage();
        }

        private void Thick_Mode_Checked(object sender, RoutedEventArgs e)
        {
            mode = Modes.ThickMode;
            shapeComboBox.IsEnabled = false;
            UncheckAll();
            RedrawImage();
        }

        private void Color_Mode_Checked(object sender, RoutedEventArgs e)
        {
            mode = Modes.ColorMode;
            shapeComboBox.IsEnabled = false;
            UncheckAll();
            RedrawImage();
        }

        private void Delete_Mode_Checked(object sender, RoutedEventArgs e)
        {
            mode = Modes.DeleteMode;
            shapeComboBox.IsEnabled = false;
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
            EditionMethods.UpdateShapes(shapes, AntiAliasing.IsChecked == true);
            RedrawImage();
        }

        private void UncheckAll()
        {
            firstClick = false;
            circlePointEdit = false;
            lineEditMode = 0;
            polygonEditMode = 0;
            capsuleEditMode = 0;
            capsuleDrawing = false;
            polygonDrawing = false;
            editPoints.Clear();
        }

        // ---------------------- Drawing Mechanisms -----------------------
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (mode)
            {
                case Modes.DrawingMode:
                    if (shapeComboBox.SelectedIndex == 0) DrawLine(e);
                    if (shapeComboBox.SelectedIndex == 1) DrawPolygon(e);
                    if (shapeComboBox.SelectedIndex == 2) DrawCircle(e);
                    if (shapeComboBox.SelectedIndex == 3) DrawCapsule(e);
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
                        s.Radius = (int)EditionMethods.Distance(s.Origin, x, y);
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
                            EditionMethods.UpdateLine(s, AntiAliasing.IsChecked == true);
                            break;
                        case 1:
                            int xDiff = x - (int)lastPosition.X;
                            int yDiff = y - (int)lastPosition.Y;
                            s.Start = new PixelPoint(s.Start.X + xDiff, s.Start.Y + yDiff, s.shapeColor);
                            s.End = new PixelPoint(s.End.X + xDiff, s.End.Y + yDiff, s.shapeColor);
                            EditionMethods.UpdateLine(s, AntiAliasing.IsChecked == true);
                            lastPosition.X = x;
                            lastPosition.Y = y;
                            break;
                        case 2:
                            s.End = new PixelPoint(x, y, s.shapeColor);
                            EditionMethods.UpdateLine(s, AntiAliasing.IsChecked == true);
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
                            EditionMethods.UpdatePolygon(s, AntiAliasing.IsChecked == true);
                            break;
                        case 1: //Polygon Center
                            int xDiff = x - (int)lastPosition.X;
                            int yDiff = y - (int)lastPosition.Y;
                            for (int i = 0; i < s.Vertices.Count; i++)
                            {
                                s.Vertices[i].X += xDiff;
                                s.Vertices[i].Y += yDiff;
                            }
                            EditionMethods.UpdatePolygon(s, AntiAliasing.IsChecked == true);
                            lastPosition.X = x;
                            lastPosition.Y = y;
                            break;
                        case 2: //Polygon Edge
                            int dx = x - (int)lastPosition.X;
                            int dy = y - (int)lastPosition.Y;
                            s.Vertices[Index1] = new PixelPoint(s.Vertices[Index1].X + dx, s.Vertices[Index1].Y + dy, s.shapeColor);
                            s.Vertices[Index2] = new PixelPoint(s.Vertices[Index2].X + dx, s.Vertices[Index2].Y + dy, s.shapeColor);
                            EditionMethods.UpdatePolygon(s, AntiAliasing.IsChecked == true);
                            lastPosition.X = x;
                            lastPosition.Y = y;
                            break;
                    }
                }
                if (selectedShape is Capsule)
                {
                    var s = selectedShape as Capsule;
                    switch (capsuleEditMode)
                    {
                        case 0: //Capsule Origin A
                            s.OriginA = new PixelPoint(x, y, s.shapeColor);
                            s.AllPixels = DrawingAlgorithms.Capsule(s.OriginA.X, s.OriginA.Y, s.OriginB.X, s.OriginB.Y, s.Radius, s.shapeColor);
                            break;
                        case 1: //Capsule Center
                            int dx = x - (int)lastPosition.X;
                            int dy = y - (int)lastPosition.Y;
                            s.OriginA = new PixelPoint(s.OriginA.X + dx, s.OriginA.Y + dy, s.shapeColor);
                            s.OriginB = new PixelPoint(s.OriginB.X + dx, s.OriginB.Y + dy, s.shapeColor);
                            s.AllPixels = DrawingAlgorithms.Capsule(s.OriginA.X, s.OriginA.Y, s.OriginB.X, s.OriginB.Y, s.Radius, s.shapeColor);
                            lastPosition.X = x;
                            lastPosition.Y = y;
                            break;
                        case 2: //Capsule Origin B
                            s.OriginB = new PixelPoint(x, y, s.shapeColor);
                            s.AllPixels = DrawingAlgorithms.Capsule(s.OriginA.X, s.OriginA.Y, s.OriginB.X, s.OriginB.Y, s.Radius, s.shapeColor);
                            break;
                        case 3: //Capsule Radius
                            if(EditionMethods.Distance(s.OriginA, x, y) <= s.Radius + 10) 
                                s.Radius = (int)EditionMethods.Distance(s.OriginA, x, y);
                            else s.Radius = (int)EditionMethods.Distance(s.OriginB, x, y);
                            s.AllPixels = DrawingAlgorithms.Capsule(s.OriginA.X, s.OriginA.Y, s.OriginB.X, s.OriginB.Y, s.Radius, s.shapeColor);
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
                    EditionMethods.FindEditPoints(shapes, editPoints, editColor);
                    EditionMethods.DrawEditPoints(bitmap, editPoints);
                }
                if (mode == Modes.DeleteMode || mode == Modes.ColorMode || mode == Modes.ThickMode)
                {
                    EditionMethods.FindDeletePoints(shapes, editPoints, editColor);
                    EditionMethods.DrawEditPoints(bitmap, editPoints);
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

        private void DrawCapsule(MouseButtonEventArgs e)
        {
            int x = (int)Math.Round(e.GetPosition(Image).X);
            int y = (int)Math.Round(e.GetPosition(Image).Y);
            if (firstClick && !capsuleDrawing)
            {
                lastPosition.X = x;
                lastPosition.Y = y;
                capsuleDrawing = true;
            }
            else if (firstClick && capsuleDrawing)
            {
                Colour c = new Colour(lineColor.R, lineColor.G, lineColor.B);
                int radius = (int)Math.Round(Math.Sqrt(Math.Pow(lastPosition.X - x, 2) + Math.Pow(lastPosition.Y - y, 2)));
                PixelPoint A = new PixelPoint((int)firstPosition.X, (int)firstPosition.Y, c);
                PixelPoint B = new PixelPoint((int)lastPosition.X, (int)lastPosition.Y, c);
                Capsule capsule = new Capsule(A, B, radius, c);
                capsule.AllPixels = DrawingAlgorithms.Capsule(A.X, A.Y, B.X, B.Y, radius, c);
                capsule.DrawPixels(bitmap);
                shapes.Add(capsule);
                firstClick = false;
                capsuleDrawing = false;
            }
            else
            {
                firstClick = true;
                firstPosition.X = x;
                firstPosition.Y = y;
            }
        }


        // -------------------- Editing Shapes --------------------
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
                    if (EditionMethods.isNearPoint(s.Start, x, y))
                    {
                        lineEditMode = 0;
                        selectedShape = s;
                        return true;
                    }
                    if (EditionMethods.isNearPoint(center, x, y))
                    {
                        lineEditMode = 1;
                        selectedShape = s;
                        return true;
                    }
                    if (EditionMethods.isNearPoint(s.End, x, y))
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
                    if (EditionMethods.isNearPoint(center, x, y))
                    {
                        polygonEditMode = 1;
                        selectedShape = s;
                        return true;
                    }
                    foreach(PixelPoint v in s.Vertices)
                    {
                        int idx = s.Vertices.IndexOf(v);
                        if (EditionMethods.isNearPoint(v, x, y))
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
                            if (EditionMethods.isNearPoint(tmp, x, y))
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
                    if (EditionMethods.isNearPoint(s.Origin, x, y))
                    {
                        circlePointEdit = false;
                        selectedShape = s;
                        return true;
                    }
                    if (EditionMethods.Distance(s.Origin, x, y) > s.Radius - 10 && EditionMethods.Distance(s.Origin, x, y) < s.Radius + 10)
                    {
                        circlePointEdit = true;
                        selectedShape = s;
                        return true;
                    }
                }
                else if (shape is Capsule)
                {
                    var s = shape as Capsule;
                    PixelPoint center = new PixelPoint((s.OriginA.X + s.OriginB.X) / 2, (s.OriginA.Y + s.OriginB.Y) / 2, s.shapeColor);
                    if (EditionMethods.isNearPoint(s.OriginA, x, y))
                    {
                        capsuleEditMode = 0;
                        selectedShape = s;
                        return true;
                    }
                    if (EditionMethods.isNearPoint(center, x, y))
                    {
                        capsuleEditMode = 1;
                        selectedShape = s;
                        return true;
                    }
                    if (EditionMethods.isNearPoint(s.OriginB, x, y))
                    {
                        capsuleEditMode = 2;
                        selectedShape = s;
                        return true;
                    }
                    if (EditionMethods.Distance(s.OriginA, x, y) > s.Radius - 10 && EditionMethods.Distance(s.OriginA, x, y) < s.Radius + 10 ||
                        EditionMethods.Distance(s.OriginB, x, y) > s.Radius - 10 && EditionMethods.Distance(s.OriginB, x, y) < s.Radius + 10)
                    {
                        capsuleEditMode = 3;
                        selectedShape = s;
                        return true;
                    }
                }
            }
            return false;
        }

        // ------------------ Edit Color/Thickness, Delete ----------------
        private void DeleteShape(MouseButtonEventArgs e)
        {
            int x = (int)Math.Round(e.GetPosition(Image).X);
            int y = (int)Math.Round(e.GetPosition(Image).Y);
            Shape toDelete = EditionMethods.GetClickedShape(shapes, x, y);
            if (toDelete != null)
            {
                shapes.Remove(toDelete);
                RedrawImage();
            }
        }

        private void ChangeColorThickness(MouseButtonEventArgs e, bool color)
        {
            int x = (int)Math.Round(e.GetPosition(Image).X);
            int y = (int)Math.Round(e.GetPosition(Image).Y);
            Shape thick = EditionMethods.GetClickedShape(shapes, x, y);
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
                if (thick is Line) EditionMethods.UpdateLine(thick as Line, AntiAliasing.IsChecked == true);
                else if (thick is Polygon) EditionMethods.UpdatePolygon(thick as Polygon, AntiAliasing.IsChecked == true);
                else if (thick is Circle)
                {
                    if (color == false) return;
                    var s = thick as Circle;
                    s.AllPixels = DrawingAlgorithms.MidpointCircle(s.Origin.X, s.Origin.Y, s.Radius, s.shapeColor);
                }
                else if (thick is Capsule)
                {
                    if (color == false) return;
                    var s = thick as Capsule;
                    s.AllPixels = DrawingAlgorithms.Capsule(s.OriginA.X, s.OriginA.Y, s.OriginB.X, s.OriginB.Y, s.Radius, s.shapeColor);
                }
                RedrawImage();
            }
        }



    }
}
