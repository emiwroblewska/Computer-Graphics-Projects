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

namespace CG_Project_4
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
        private Point lastPosition = new Point(-1, -1);
        private Color lineColor = Colors.Black;
        private Color fillColor = Colors.Black;
        private System.Drawing.Bitmap fillPattern = null;

        private readonly Colour editColor = new Colour(163, 163, 194);
        private Shape selectedShape = null;
        private Rectangle clipRectangle = null;
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
            ClipMode,
            FillMode,
            UnfillMode,
            FloodFillMode,
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

        private void Window_Size_Changed(object sender, SizeChangedEventArgs e) { }

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
                EditionMethods.UpdateShapes(shapes, AntiAliasing.IsChecked == true, clipRectangle);
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
            clipRectangle = null;
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
            clipRectangle = null;
            EditionMethods.UpdateShapes(shapes, AntiAliasing.IsChecked == true, clipRectangle);
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
        private void Clip_Mode_Checked(object sender, RoutedEventArgs e)
        {
            mode = Modes.ClipMode;
            shapeComboBox.IsEnabled = false;
            UncheckAll();
            RedrawImage();
        }
        private void Fill_Mode_Checked(object sender, RoutedEventArgs e)
        {
            mode = Modes.FillMode;
            shapeComboBox.IsEnabled = false;
            PatternCheckBox.IsEnabled = true;
            UncheckAll();
            RedrawImage();
        }
        private void Fill_Mode_Unchecked(object sender, RoutedEventArgs e)
        {
            PatternCheckBox.IsEnabled = false;
            PatternCheckBox.IsChecked = false;
            PatternButton.IsEnabled = false;
            fillPattern = null;
            PatternImage.Source = null;
        }
        private void Unfill_Mode_Checked(object sender, RoutedEventArgs e)
        {
            mode = Modes.UnfillMode;
            shapeComboBox.IsEnabled = false;
            UncheckAll();
            RedrawImage();
        }
        private void Flood_Mode_Checked(object sender, RoutedEventArgs e)
        {
            mode = Modes.FloodFillMode;
            shapeComboBox.IsEnabled = false;
            UncheckAll();
            RedrawImage();
        }

        private void Selected_Color_Changed(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (ColorPicker.SelectedColor.HasValue)
                lineColor = ColorPicker.SelectedColor.Value;
        }

        private void Anti_Aliasing_Changed(object sender, RoutedEventArgs e)
        {
            foreach (var shape in shapes)
                shape.AllPixels.Clear();
            EditionMethods.UpdateShapes(shapes, AntiAliasing.IsChecked == true, clipRectangle);
            RedrawImage();
        }

        //---------------- Filling/Clipping functionalities -----------------
        private void Pattern_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a pattern";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" + "JPEG (*.jpg,*.jpeg)|*.jpg;*.jpeg|" +
                "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                fillPattern = new System.Drawing.Bitmap(op.FileName);
                PatternImage.Source = new BitmapImage(new Uri(op.FileName));
            }
        }

        private void Fill_Pattern_Checked(object sender, RoutedEventArgs e)
        {
            PatternButton.IsEnabled = PatternCheckBox.IsChecked == true;
            FloodCheckBox.IsEnabled = PatternCheckBox.IsChecked == true;
            if (PatternCheckBox.IsChecked == false)
            {
                PatternImage.Source = null;
                FloodCheckBox.IsChecked = false;
            }
                
        }

        private void Selected_FillColor_Changed(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (FillColorPicker.SelectedColor.HasValue)
                fillColor = FillColorPicker.SelectedColor.Value;
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
            int x = (int)Math.Round(e.GetPosition(Image).X);
            int y = (int)Math.Round(e.GetPosition(Image).Y);
            switch (mode)
            {
                case Modes.DrawingMode:
                    if (shapeComboBox.SelectedIndex == 0) DrawLine(x, y);
                    if (shapeComboBox.SelectedIndex == 1) DrawPolygon(x, y);
                    if (shapeComboBox.SelectedIndex == 2) DrawRectangle(x, y);
                    if (shapeComboBox.SelectedIndex == 3) DrawCircle(x, y);
                    break;
                case Modes.EditMode:
                    if (FindSelectedPoint(x, y))
                    {
                        movingPoint = true;
                        lastPosition = e.GetPosition(Image);
                    }
                    break;
                case Modes.ThickMode:
                    ChangeColorThickness(x, y, false);
                    break;
                case Modes.ColorMode:
                    ChangeColorThickness(x, y, true);
                    break;
                case Modes.DeleteMode:
                    DeleteShape(x, y);
                    break;
                case Modes.ClipMode:
                    Clipping(x, y);
                    break;
                case Modes.FillMode:
                    if (PatternCheckBox.IsChecked == true && FloodCheckBox.IsChecked == true && fillPattern != null)
                        FloodFilling(x, y);
                    else
                        Filling(x, y);
                    break;
                case Modes.UnfillMode:
                    Unfilling(x, y);
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
                    switch (lineEditMode)
                    {
                        case 0:
                            s.Start = new PixelPoint(x, y, s.shapeColor);
                            EditionMethods.UpdateLine(s, AntiAliasing.IsChecked == true, clipRectangle);
                            break;
                        case 1:
                            int xDiff = x - (int)lastPosition.X;
                            int yDiff = y - (int)lastPosition.Y;
                            s.Start = new PixelPoint(s.Start.X + xDiff, s.Start.Y + yDiff, s.shapeColor);
                            s.End = new PixelPoint(s.End.X + xDiff, s.End.Y + yDiff, s.shapeColor);
                            EditionMethods.UpdateLine(s, AntiAliasing.IsChecked == true, clipRectangle);
                            lastPosition.X = x;
                            lastPosition.Y = y;
                            break;
                        case 2:
                            s.End = new PixelPoint(x, y, s.shapeColor);
                            EditionMethods.UpdateLine(s, AntiAliasing.IsChecked == true, clipRectangle);
                            break;
                    }
                }
                if (selectedShape is Polygon)
                {
                    var s = selectedShape as Polygon;
                    int xDiff = x - (int)lastPosition.X;
                    int yDiff = y - (int)lastPosition.Y;
                    if (s.AllPixels != null) s.AllPixels.Clear();
                    switch (polygonEditMode)
                    {
                        case 0: //Polygon Vertex
                            if (s is Rectangle)
                            {
                                int idxLeft = Index1 == 0 ? s.Vertices.Count - 1 : Index1 - 1;
                                int idxRight = Index1 == s.Vertices.Count - 1 ? 0 : Index1 + 1;
                                if (s.Vertices[idxLeft].X == s.Vertices[Index1].X)
                                {
                                    s.Vertices[idxLeft] = new PixelPoint(x, s.Vertices[idxLeft].Y, s.shapeColor);
                                    s.Vertices[idxRight] = new PixelPoint(s.Vertices[idxRight].X, y, s.shapeColor);
                                }
                                else if (s.Vertices[idxLeft].Y == s.Vertices[Index1].Y)
                                {
                                    s.Vertices[idxLeft] = new PixelPoint(s.Vertices[idxLeft].X, y, s.shapeColor);
                                    s.Vertices[idxRight] = new PixelPoint(x, s.Vertices[idxRight].Y, s.shapeColor);
                                }
                            }
                            s.Vertices[Index1] = new PixelPoint(x, y, s.shapeColor);
                            EditionMethods.UpdatePolygon(s, AntiAliasing.IsChecked == true, clipRectangle);
                            if (clipRectangle != null && s == clipRectangle)
                                EditionMethods.UpdateShapes(shapes, AntiAliasing.IsChecked == true, clipRectangle);
                            lastPosition.X = x;
                            lastPosition.Y = y;
                            break;
                        case 1: //Polygon Center
                            for (int i = 0; i < s.Vertices.Count; i++)
                            {
                                s.Vertices[i].X += xDiff;
                                s.Vertices[i].Y += yDiff;
                            }
                            EditionMethods.UpdatePolygon(s, AntiAliasing.IsChecked == true, clipRectangle);
                            if (clipRectangle != null && s == clipRectangle)
                                EditionMethods.UpdateShapes(shapes, AntiAliasing.IsChecked == true, clipRectangle);
                            lastPosition.X = x;
                            lastPosition.Y = y;
                            break;
                        case 2: //Polygon Edge
                            if (s is Rectangle)
                            {
                                if (s.Vertices[Index1].X == s.Vertices[Index2].X) yDiff = 0;
                                else if (s.Vertices[Index1].Y == s.Vertices[Index2].Y) xDiff = 0;
                            }
                            s.Vertices[Index1] = new PixelPoint(s.Vertices[Index1].X + xDiff, s.Vertices[Index1].Y + yDiff, s.shapeColor);
                            s.Vertices[Index2] = new PixelPoint(s.Vertices[Index2].X + xDiff, s.Vertices[Index2].Y + yDiff, s.shapeColor);
                            EditionMethods.UpdatePolygon(s, AntiAliasing.IsChecked == true, clipRectangle);
                            if (clipRectangle != null && s == clipRectangle)
                                EditionMethods.UpdateShapes(shapes, AntiAliasing.IsChecked == true, clipRectangle);
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
            if(FloodCheckBox != null && FloodCheckBox.IsChecked == false)
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
                else if (mode == Modes.DeleteMode || mode == Modes.ColorMode || mode == Modes.ThickMode)
                {
                    EditionMethods.FindDeletePoints(shapes, editPoints, editColor);
                    EditionMethods.DrawEditPoints(bitmap, editPoints);
                }
                else if (mode == Modes.FillMode || mode == Modes.UnfillMode)
                {
                    EditionMethods.FindDeletePoints(shapes.Where(s => s is Polygon).ToList(), editPoints, editColor);
                    EditionMethods.DrawEditPoints(bitmap, editPoints);
                }
            }
        }


        // --------------------- Drawing Shapes ------------------------
        private void DrawLine(int x, int y)
        {
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

        private void DrawCircle(int x, int y)
        {
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

        private void DrawPolygon(int x, int y)
        {
            if (firstClick && !polygonDrawing)
            {
                int t = Convert.ToInt32(ThicknessComboBox.Text.ToString());
                Colour c = new Colour(lineColor.R, lineColor.G, lineColor.B);
                currentPolygon.SetThickness(t);
                currentPolygon.shapeColor = c;
                currentPolygon.AddVertex(new PixelPoint((int)lastPosition.X, (int)lastPosition.Y, c));
                currentPolygon.AddVertex(new PixelPoint(x, y, c));
                if (AntiAliasing.IsChecked == true)
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
                    if (AntiAliasing.IsChecked == true)
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

        private void DrawRectangle(int x, int y)
        {
            if (firstClick && lastPosition.X != -1)
            {
                int t = Convert.ToInt32(ThicknessComboBox.Text.ToString());
                Colour c = new Colour(lineColor.R, lineColor.G, lineColor.B);
                PixelPoint start = new PixelPoint((int)lastPosition.X, (int)lastPosition.Y, c);
                PixelPoint end = new PixelPoint(x, y, c);
                Rectangle rec = new Rectangle(start, end, t, c)
                {
                    AllPixels = new List<PixelPoint>()
                };
                EditionMethods.UpdatePolygon(rec, AntiAliasing.IsChecked == true, clipRectangle);
                rec.DrawPixels(bitmap);
                shapes.Add(rec);
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

        // ------------------ Find Selected Point -----------------
        private bool FindSelectedPoint(int x, int y)
        {
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
                    foreach (PixelPoint v in s.Vertices)
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
            }
            return false;
        }

        //--------------- Delete/Change Color/Thickness --------------
        private void DeleteShape(int x, int y)
        {
            Shape toDelete = EditionMethods.GetClickedShape(shapes, x, y);
            if (toDelete != null)
            {
                shapes.Remove(toDelete);
                if (clipRectangle != null && toDelete == clipRectangle)
                    clipRectangle = null;
                EditionMethods.UpdateShapes(shapes, AntiAliasing.IsChecked == true, clipRectangle);
                RedrawImage();
            }
        }

        private void ChangeColorThickness(int x, int y, bool color)
        {
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
                if (thick is Line) EditionMethods.UpdateLine(thick as Line, AntiAliasing.IsChecked == true, clipRectangle);
                else if (thick is Polygon)
                {
                    thick.AllPixels.Clear();
                    EditionMethods.UpdatePolygon(thick as Polygon, AntiAliasing.IsChecked == true, clipRectangle);
                }
                else if (thick is Circle)
                {
                    if (color == false) return;
                    var s = thick as Circle;
                    s.AllPixels = DrawingAlgorithms.MidpointCircle(s.Origin.X, s.Origin.Y, s.Radius, s.shapeColor);
                }
                RedrawImage();
            }
        }

        //---------------- Filling/Clipping -----------------
        private void Clipping(int x, int y)
        {
            foreach (var shape in shapes)
            {
                if (shape is Rectangle)
                {
                    var tmp = shape as Rectangle;
                    if (x >= tmp.Left && x <= tmp.Right && y >= tmp.Bottom && y <= tmp.Top)
                    {
                        clipRectangle = tmp;
                    }
                }
            }
            EditionMethods.UpdateShapes(shapes, AntiAliasing.IsChecked == true, clipRectangle);
            RedrawImage();
        }

        private void Filling(int x, int y)
        {
            var shape = EditionMethods.GetClickedShape(shapes, x, y);
            if(shape is Polygon)
            {
                var poly = shape as Polygon;
                if (clipRectangle != null && poly == clipRectangle) //don't fill clipping Rectangle
                    return;
                if (PatternCheckBox.IsChecked == true)
                {
                    poly.FillPattern = fillPattern;
                    poly.FillColor = null;
                }
                else
                {
                    poly.FillColor = new Colour(fillColor);
                    poly.FillPattern = null;
                }
                poly.AllPixels.Clear();
                EditionMethods.UpdatePolygon(poly, AntiAliasing.IsChecked == true, clipRectangle);
                RedrawImage();
            }
        }

        private void Unfilling(int x, int y)
        {
            var shape = EditionMethods.GetClickedShape(shapes, x, y);
            if (shape is Polygon)
            {
                var poly = shape as Polygon;
                poly.AllPixels.Clear();
                poly.FillColor = null;
                poly.FillPattern = null;
                EditionMethods.UpdatePolygon(poly, AntiAliasing.IsChecked == true, clipRectangle);
                RedrawImage();
            }
        }

        private void FloodFilling(int x, int y)
        {
            FillingAlgorithms.FloodFill(bitmap, x, y, fillPattern);
        }


        //------End of class -----
    }
}
