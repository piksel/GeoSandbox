using System;
using System.Windows;
using System.Windows.Controls;

namespace GeoSandbox
{
    public class GeoGrid
    {
        private FrameworkElement? _target;
        private Point _mouseStart;
        private Corner _corner;
        private Size _originalSize;
        private Point _originalPos;
        public Interaction Interaction { get; private set; }
        public int GridSize { get; }
        public int SnapMargin { get; }

        public int MinDim { get; set; }

        public event EventHandler? GridlinesUpdated;

        public GeoGrid(int gridSize, int snapMargin)
        {
            GridSize = gridSize;
            SnapMargin = snapMargin;
            MinDim = gridSize;
        }

        public FrameworkElement[]? Elements { get; set; }

        public void EndInteraction()
        {
            if (_target is { } target)
            {
                if (Interaction == Interaction.Move)
                {
                    SnapPosition(target);
                }

                if (Interaction == Interaction.Resize)
                {
                    SnapSize(target);
                }
            }
            
            UpdateGridLines();
            
            Interaction = Interaction.None;
            _target = null;
        }

        public void UpdateGridLines()
        {
            if (Elements is null) return;
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var maxX = 0.0;
            var maxY = 0.0;
            for (var i = 0; i < Elements.Length; i++)
            {
                var r = Elements[i];

                var left = Canvas.GetLeft(r);
                var top = Canvas.GetTop(r);
                var right = left + r.Width;
                var bot = top + r.Height;

                minX = Math.Min(left, minX);
                minY = Math.Min(top, minY);
                
                maxX = Math.Max(right, maxX);
                maxY = Math.Max(bot, maxY);
            }
            
            var width = maxX - minX;
            var height = maxY - minY;
            var square = Math.Max(width, height);
            
            SquareOutline = new Rect(minX, minY, square, square);
            LimitOutline = new Rect(minX, minY, width, height);
            
            GridlinesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public Rect LimitOutline { get; private set; }

        public Rect SquareOutline { get; private set; }

        public void BeginResize(FrameworkElement target, Corner corner, Point mousePos)
        {
            Interaction = Interaction.Resize;
            _target = target;
            _mouseStart = mousePos;
            _corner = corner;
            _originalSize = new Size(target.Width, target.Height);
        }

        public bool UpdateInteraction(Point mousePos)
        {
            if(_target is not {} r) return false;
            if (Interaction == Interaction.Move)
            {
                var offset = mousePos - _mouseStart;
                var newRect = new Rect(_originalPos + offset, r.RenderSize);

                r.SetValue(Canvas.LeftProperty, newRect.X);
                r.SetValue(Canvas.TopProperty, newRect.Y);
            } 
            else if (Interaction == Interaction.Resize)
            {
                var offset = mousePos - _mouseStart;
                double newWidth, newHeight;
                if (r.Width > r.Height)
                {
                    var longOffset = _corner.HasFlag(Corner.Right) ? offset.X : 1 - offset.X;
                    var longDim = Math.Max(MinDim, _originalSize.Width + longOffset);
                    newWidth = longDim;
                    newHeight = longDim* 0.5;
                }
                else
                {
                    var longOffset = _corner.HasFlag(Corner.Bottom) ? offset.Y : 1 - offset.Y;
                    var longDim = Math.Max(MinDim, _originalSize.Height + longOffset);
                    newHeight = longDim;
                    newWidth = longDim* 0.5;
                }

                if (_corner.HasFlag(Corner.Left))
                {
                    Canvas.SetLeft(r, Canvas.GetLeft(r) - (newWidth - r.Width));
                }

                if(_corner.HasFlag(Corner.Top))
                {
                    Canvas.SetTop(r, Canvas.GetTop(r) - (newHeight - r.Height));
                }
                
                r.Width = newWidth;
                r.Height = newHeight;
            }
            else
            {
                // Skip updating if nothing changed
                return false;
            }
            
            UpdateGridLines();

            return true;
        }
        
        private Rect GetCanvasRect(FrameworkElement element)
            => new(GetCanvasPos(element), new Size(element.Width, element.Height));
        
        private Point GetCanvasPos(FrameworkElement element)
            => new(Canvas.GetLeft(element), Canvas.GetTop(element));

        public void BeginMove(FrameworkElement target, Point mousePos)
        {
            Interaction = Interaction.Move;
            _target = target;
            _originalPos = GetCanvasPos(target);
            _mouseStart = mousePos;
            
            MoveToTop(target);
            UpdateZIndexes();
        }
        
        private void UpdateZIndexes()
        {
            if (Elements is null) return;
            for (var i = 0; i < Elements.Length; i++)
            {
                Panel.SetZIndex(Elements[i], i);
            }
        }

        private void MoveToTop(FrameworkElement target)
        {
            if(Elements is null) return;
            Array.Sort(Elements, (ra, rb) => ra == target ? 1 : rb == target ? -1 : 0);
        }
        
        public void SnapSize(FrameworkElement target)
        {
            target.Width = Math.Floor(target.Width / GridSize) * GridSize;
            target.Height = Math.Floor(target.Height/ GridSize) * GridSize;
        }
        
        public void SnapPosition(FrameworkElement target)
        {
            var targetRect = GetCanvasRect(target);
            
            if (Elements != null)
            {
                foreach (var friend in Elements)
                {
                    if (friend == target) continue;

                    var friendRect = GetCanvasRect(friend);

                    if (Math.Abs(targetRect.Top - friendRect.Bottom) < SnapMargin)
                    {
                        targetRect.Y = friendRect.Bottom;
                    }
                    else if (Math.Abs(targetRect.Bottom - friendRect.Top) < SnapMargin)
                    {
                        targetRect.Y = (friendRect.Top - targetRect.Height);
                    }

                    if (Math.Abs(targetRect.Left - friendRect.Right) < SnapMargin)
                    {
                        targetRect.X = friendRect.Right;
                    }
                    else if (Math.Abs(targetRect.Right - friendRect.Left) < SnapMargin)
                    {
                        targetRect.X = (friendRect.Left - targetRect.Width);
                    }
                }
            }

            Canvas.SetTop(target, Math.Floor(targetRect.Y / GridSize) * GridSize);
            Canvas.SetLeft(target, Math.Floor(targetRect.X / GridSize) * GridSize);
        }

        public void SetCanvasRect(FrameworkElement element, Rect targetRect)
        {
            element.Width = targetRect.Width;
            element.Height = targetRect.Height;
            element.SetValue(Canvas.LeftProperty, targetRect.Left);
            element.SetValue(Canvas.TopProperty, targetRect.Top);
        }
    }
    
    
    [Flags]
    public enum Corner
    {
        None = 0,
        Top = 1 << 0,
        Left = 1 << 1,
        Bottom = 1 << 2,
        Right = 1 << 3,
        
        TopLeft = Top | Left,
        TopRight = Top | Right,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right
    }

    public enum Interaction
    {
        None = 0,
        Move = 1,
        Resize = 2
    }
}