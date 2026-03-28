using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Follow
{
    public partial class DraggableNumericKnob : UserControl
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);
        private Point _lockedScreenPos;
        private bool _isDragging;

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(DraggableNumericKnob),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register("Step", typeof(double), typeof(DraggableNumericKnob),
                new PropertyMetadata(0.1));

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(double), typeof(DraggableNumericKnob),
                new PropertyMetadata(double.MinValue));

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(double), typeof(DraggableNumericKnob),
                new PropertyMetadata(double.MaxValue));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, Math.Clamp(value, MinValue, MaxValue));
        }

        public double Step
        {
            get => (double)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public double MinValue
        {
            get => (double)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public DraggableNumericKnob()
        {
            InitializeComponent();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _isDragging = true;
            Point controlPos = this.PointToScreen(new Point(ActualWidth / 2, ActualHeight / 2));
            _lockedScreenPos = controlPos;
            SetCursorPos((int)_lockedScreenPos.X, (int)_lockedScreenPos.Y);
            CaptureMouse();
            Cursor = Cursors.None;
            e.Handled = true;
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isDragging && IsMouseCaptured)
            {
                Point currentPos = e.GetPosition(this);
                Point centerPos = new Point(ActualWidth / 2, ActualHeight / 2);

                double deltaY = centerPos.Y - currentPos.Y;

                if (Math.Abs(deltaY) > 0.5)
                {
                    double speed = Math.Abs(deltaY);
                    double accel = speed < 3.0
                        ? 1.0
                        : 1.0 + Math.Pow(speed - 3.0, 1.5);
                    Value += Math.Sign(deltaY) * Step * accel;
                    SetCursorPos((int)_lockedScreenPos.X, (int)_lockedScreenPos.Y);
                }

                e.Handled = true;
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                StopDrag();
                e.Handled = true;
            }
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            StopDrag();
            base.OnLostMouseCapture(e);
        }

        private void StopDrag()
        {
            _isDragging = false;
            ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
            // Maus wieder sichtbar — sie bleibt an der gespeicherten Position (Mitte des Controls)
        }
    }
}
