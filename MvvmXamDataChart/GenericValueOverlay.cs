using Infragistics.Controls.Charts;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MvvmXamDataChart
{
    public class GenericValueOverlay : Series
    {
        public static readonly DependencyProperty AxisProperty = DependencyProperty.Register("Axis",
            typeof(Axis), typeof(GenericValueOverlay),
            new PropertyMetadata(null, (sender, args) =>
            {
                var series = sender as GenericValueOverlay;
                if (series != null)
                    series.RaisePropertyChanged("Axis", args.OldValue, args.NewValue);
            }));

        public Axis Axis
        {
            get { return (Axis)this.GetValue(AxisProperty); }
            set { this.SetValue(AxisProperty, value); }
        }

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position",
            typeof(object), typeof(GenericValueOverlay),
            new PropertyMetadata(null, (sender, args) =>
            {
                var series = sender as GenericValueOverlay;
                if (series != null)
                    series.RaisePropertyChanged("Position", args.OldValue, args.NewValue);
            }));

        public object Position
        {
            get { return this.GetValue(PositionProperty); }
            set { this.SetValue(PositionProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
            typeof(object), typeof(GenericValueOverlay),
            new PropertyMetadata(null, (sender, args) =>
            {
                var series = sender as GenericValueOverlay;
                if (series != null)
                    series.RaisePropertyChanged("Value", args.OldValue, args.NewValue);
            }));

        public object Value
        {
            get { return this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        public GenericValueOverlay()
        {
            this.DefaultStyleKey = typeof(GenericValueOverlay);
        }

        private Line _line;
        private FrameworkElement _label;

        private readonly List<Type> _xAxes = new List<Type>
        {
            typeof(CategoryXAxis),
            typeof(CategoryDateTimeXAxis)
        };

        private readonly List<Type> _yAxes = new List<Type>
        {
            typeof(NumericYAxis)
        };

        protected override void RenderSeriesOverride(bool animate)
        {
            base.RenderSeriesOverride(animate);

            if (this.Viewport.IsEmpty || this.RootCanvas == null || this.Axis == null)
                return;

            var scalerParams = new ScalerParams(this.SeriesViewer.WindowRect, this.Viewport, Axis.IsInverted);

            double position;
            if (this.Position is DateTime)
                position = this.Axis.GetScaledValue(((DateTime)this.Position).Ticks, scalerParams);
            else
                try
                {
                    position = this.Axis.GetScaledValue((double)Convert.ChangeType(this.Position, typeof(double)), scalerParams);
                }
                catch (Exception exc)
                {
                    //ErrorLog.Write(exc);
                    return;
                }

            if (this._line == null || this._label == null)
            {
                this.RootCanvas.Children.Clear();

                this._line = new Line
                {
                    Stroke = this.Brush,
                    StrokeThickness = this.Thickness,
                    Opacity = this.Opacity
                };
                this.RootCanvas.Children.Add(this._line);

                var labelStack = new StackPanel
                {
                    Orientation = Orientation.Vertical
                };

                this._label = labelStack;

                var val = new TextBlock
                {
                    Margin = new Thickness(5, 5, 5, 2),
                    Background =
                        new SolidColorBrush(Color.FromArgb((byte)(255 * this.Opacity), 255, 255, 255)),
                    FontWeight = FontWeights.Bold
                };

                var valBinding = new Binding("Value") { Source = this };
                val.SetBinding(TextBlock.TextProperty, valBinding);

                var pos = new TextBlock
                {
                    Margin = new Thickness(5, 5, 5, 2),
                    Background = new SolidColorBrush(Color.FromArgb((byte)(255 * this.Opacity), 255, 255, 255)),
                    Visibility = Visibility.Collapsed
                };

                var posBinding = new Binding("Position") { Source = this };
                pos.SetBinding(TextBlock.TextProperty, posBinding);

                labelStack.Children.Add(val);
                labelStack.Children.Add(pos);

                this.RootCanvas.Children.Add(this._label);
            }
            else
            {
                this._line.Stroke = this.Brush;
                this._line.StrokeThickness = this.Thickness;
                this._line.Opacity = this.Opacity;
            }

            if (this._xAxes.Contains(this.Axis.GetType()))
            {
                this._line.X1 = position;
                this._line.X2 = position;
                this._line.Y1 = 0;
                this._line.Y2 = this.RootCanvas.ActualHeight;

                Canvas.SetTop(this._label, 0);
                Canvas.SetLeft(this._label, position);
            }
            else if (this._yAxes.Contains(this.Axis.GetType()))
            {
                this._line.X1 = 0;
                this._line.X2 = this.RootCanvas.ActualWidth;
                this._line.Y1 = position;
                this._line.Y2 = position;

                Canvas.SetBottom(this._label, position);
                Canvas.SetLeft(this._label, 0);
            }
        }

        protected override void RenderThumbnail(Rect viewportRect, RenderSurface surface)
        {
            base.RenderThumbnail(viewportRect, surface);

            if (this.Axis == null || this.RootCanvas == null)
                return;

            var scaleParams = new ScalerParams(new Rect(0, 0, 1, 1), viewportRect, this.Axis.IsInverted);

            double x;
            if (this.Position is DateTime)
                x = this.Axis.GetScaledValue(((DateTime)this.Position).Ticks, scaleParams);
            else
                try
                {
                    x = this.Axis.GetScaledValue((double)Convert.ChangeType(this.Position, typeof(double)), scaleParams);
                }
                catch (Exception exc)
                {
                    //ErrorLog.Write(exc);
                    return;
                }

            var lineElement = new Line
            {
                X1 = x,
                X2 = x,
                Y1 = 0,
                Y2 = this.RootCanvas.ActualHeight,
                Stroke = this.Brush,
                StrokeThickness = this.Thickness,
                Opacity = this.Opacity
            };

            surface.Surface.Children.Add(lineElement);
        }

        protected override void ViewportRectChangedOverride(Rect oldViewportRect, Rect newViewportRect)
        {
            base.ViewportRectChangedOverride(oldViewportRect, newViewportRect);
            this.RenderSeries(false);
        }

        protected override void WindowRectChangedOverride(Rect oldWindowRect, Rect newWindowRect)
        {
            base.WindowRectChangedOverride(oldWindowRect, newWindowRect);
            this.RenderSeries(false);
        }

        protected override void PropertyUpdatedOverride(object sender, string propertyName, object oldValue, object newValue)
        {
            base.PropertyUpdatedOverride(sender, propertyName, oldValue, newValue);
            switch (propertyName)
            {
                case "Axis":
                    {
                        if (oldValue != null)
                        {
                            var axis = oldValue as Axis;
                            if (axis != null)
                                axis.DeregisterSeries(this);
                        }

                        if (newValue != null)
                        {
                            var axis = newValue as Axis;
                            if (axis != null)
                                axis.RegisterSeries(this);
                        }

                        if ((this.Axis != null && !Axis.UpdateRange()) ||
                            (newValue == null && oldValue != null))
                        {
                            this.RenderSeries(false);
                            this.NotifyThumbnailDataChanged();
                        }
                    }
                    break;

                case "Position":
                    {
                        if (this.Position != null && this.Axis != null)
                        {
                            this.RenderSeries(false);
                            this.NotifyThumbnailDataChanged();
                        }
                    }
                    break;
            }
        }
    }
}
