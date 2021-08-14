using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GeoSandbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly GeoGrid _grid;

        private const int SnapMargin = 10;
        private const int GridSize = 10;
        private const int MinDim = 100;

        public MainWindow()
        {
            InitializeComponent();

            _grid = new GeoGrid(GridSize, SnapMargin)
            {
                Elements = RectCanvas.Children.OfType<GeoRect>().Cast<FrameworkElement>().ToArray(),
                MinDim = MinDim,
            };
            _grid.GridlinesUpdated += (_, _) =>
            {
                _grid.SetCanvasRect(Square, _grid.SquareOutline);
                _grid.SetCanvasRect(Outline, _grid.LimitOutline);
            };
            
            var rand = new Random();

            RectCanvas.MouseMove += OnMouseMouse;
            RectCanvas.MouseUp += OnMouseUp;
            
            foreach (var r in _grid.Elements)
            {
                r.MouseDown += OnMouseDown;
                r.MouseUp += OnMouseUp;
                
                if (r is GeoRect gr)
                {
                    var r8 = r;
                    gr.CornerMouseDown += (geo, e) 
                        => _grid.BeginResize((FrameworkElement)geo, e.Corner, r8.TranslatePoint(e.MousePos, this));
                    if (rand.NextDouble() > 0.5)
                    {
                        gr.Flip();
                    }
                }
                
                var left = rand.NextDouble() * (Width - r.Width);
                var top = rand.NextDouble() * (Height - r.Height);

                r.SetValue(Canvas.LeftProperty, left);
                r.SetValue(Canvas.TopProperty, top);
                
                _grid.SnapPosition(r);
            }
            _grid.UpdateGridLines();
        }

        private void OnMouseMouse(object sender, MouseEventArgs e)
        {
            if (_grid.Interaction == Interaction.None) return;
            _grid.UpdateInteraction(e.GetPosition(RectCanvas));
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _grid.EndInteraction();
            e.Handled = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not GeoRect r) return;
            e.Handled = true;

            if (e.MiddleButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                r.Flip();
                return;
            }
            
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _grid.BeginMove(r, e.GetPosition(RectCanvas));
            }
        }
    }

}