using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Backupper
{
    public class FromToArrow : Shape
    {
        #region == Angle ==

        public double? Angle { get => GetValue(AngleProperty) as double?; set => SetValue(AngleProperty, value); }
        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double?), typeof(FromToArrow), new PropertyMetadata(180d, new PropertyChangedCallback(UpdateGeometry)));

        #endregion

        #region == SubAngle ==

        public double? SubAngle { get => GetValue(SubAngleProperty) as double?; set => SetValue(SubAngleProperty, value); }
        public static readonly DependencyProperty SubAngleProperty = DependencyProperty.Register("SubAngle", typeof(double?), typeof(FromToArrow), new PropertyMetadata(45d, new PropertyChangedCallback(UpdateGeometry)));

        #endregion

        #region == ArrowheadLength ==

        public double? ArrowheadLength { get => GetValue(ArrowheadLengthProperty) as double?; set => SetValue(ArrowheadLengthProperty, value); }
        public static readonly DependencyProperty ArrowheadLengthProperty = DependencyProperty.Register("ArrowheadLength", typeof(double?), typeof(FromToArrow), new PropertyMetadata(0d, new PropertyChangedCallback(UpdateGeometry)));

        #endregion

        #region == IsRotating ==

        public bool? IsRotating { get => GetValue(IsRotatingProperty) as bool?; set => SetValue(IsRotatingProperty, value); }
        public static readonly DependencyProperty IsRotatingProperty = DependencyProperty.Register("IsRotating", typeof(bool?), typeof(FromToArrow));

        #endregion

        protected static void UpdateGeometry(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as FromToArrow)?.InvalidateVisual();

        protected override Geometry DefiningGeometry => GetFromToArrowGeometry();

        private Geometry GetFromToArrowGeometry()
        {
            double angle = (Angle ?? 180) * Math.PI / 180;
            double subAngle = (SubAngle ?? 45) * Math.PI / 180;
            double arrowheadLength = (ArrowheadLength ?? 0);
            double halfStrokeThickness = StrokeThickness / 2;
            double halfHeight = Height / 2;
            double padding = arrowheadLength * Math.Sin(subAngle) + halfStrokeThickness;
            Size size = new Size(Width - halfStrokeThickness - padding, halfHeight - halfStrokeThickness - padding);
            Point point1 = new Point(Width - size.Width * Math.Sin(angle) - halfStrokeThickness, halfHeight - size.Height * Math.Cos(angle));
            Point point2 = new Point(point1.X + arrowheadLength * Math.Cos(angle + subAngle), point1.Y - arrowheadLength * Math.Sin(angle + subAngle));
            Point point3 = new Point(point1.X + arrowheadLength * Math.Cos(angle - subAngle), point1.Y - arrowheadLength * Math.Sin(angle - subAngle));

            StreamGeometry streamGeometry = new StreamGeometry();
            using (StreamGeometryContext streamGeometryContext = streamGeometry.Open())
            {
                streamGeometryContext.BeginFigure(new Point(Width - halfStrokeThickness, halfStrokeThickness + padding), false, false);
                streamGeometryContext.ArcTo(point1, size, 0, false, SweepDirection.Counterclockwise, true, true);
                streamGeometryContext.LineTo(point2, true, true);
                streamGeometryContext.BeginFigure(point1, false, false);
                streamGeometryContext.LineTo(point3, true, true);
            }

            return streamGeometry;
        }
    }
}
