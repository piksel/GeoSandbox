using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GeoSandbox
{
    public partial class GeoRect
    {
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            "Fill", typeof(Brush), typeof(GeoRect), new PropertyMetadata(default(Brush)));

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public event EventHandler<CornerMouseDownEventArgs>? CornerMouseDown; 

        public GeoRect()
        {
            InitializeComponent();
        }

        private void TopLeft_OnMouseDown(object sender, MouseButtonEventArgs e) => Border_OnMouseDown(Corner.TopLeft, e);
        private void TopRight_OnMouseDown(object sender, MouseButtonEventArgs e) => Border_OnMouseDown(Corner.TopRight, e);
        private void BottomLeft_OnMouseDown(object sender, MouseButtonEventArgs e) => Border_OnMouseDown(Corner.BottomLeft, e);
        private void BottomRight_OnMouseDown(object sender, MouseButtonEventArgs e) => Border_OnMouseDown(Corner.BottomRight, e);    
        private void Border_OnMouseDown(Corner corner, MouseButtonEventArgs e)
        {
            Debug.WriteLine($"Clicked => {corner}");
            CornerMouseDown?.Invoke(this, new CornerMouseDownEventArgs(corner, e.GetPosition(this)));
            e.Handled = true;
        }

        public void Flip()
        {
            if (Width > Height)
            {
                Width /= 2;
                Height *= 2;
            }
            else
            {
                Width *= 2;
                Height /= 2;
            }
        }
    }

    public class CornerMouseDownEventArgs : EventArgs
    {
        public CornerMouseDownEventArgs(Corner corner, Point mousePos)
        {
            Corner = corner;
            MousePos = mousePos;
        }

        public Corner Corner { get; }
        public Point MousePos { get; }
    }
}