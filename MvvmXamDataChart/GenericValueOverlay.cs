using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Infragistics.Controls.Charts;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using MvvmXamDataChart.Extensions;
using TechCollections.Extensions;

namespace MvvmXamDataChart
{
    public class GenericValueOverlay : Series
    {
        #region [ Axis ]
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
        #endregion

        #region [ Share ]
        public static readonly DependencyProperty ShareProperty = DependencyProperty.Register("Share",
            typeof(double), typeof(GenericValueOverlay),
            new PropertyMetadata(default(double), (sender, args) =>
            {
                var series = sender as GenericValueOverlay;
                if (series != null)
                {
                    if (series.Axis is NumericYAxis)
                    {
                        var axis = series.Axis as NumericYAxis;
                        series.YValue = axis.ActualMaximumValue - (axis.ActualMaximumValue - axis.ActualMinimumValue) * series.Share;
                    }
                    series.RaisePropertyChanged("Share", args.OldValue, args.NewValue);
                }
            }));

        public double Share
        {
            get { return (double)this.GetValue(ShareProperty); }
            set { this.SetValue(ShareProperty, value); }
        } 
        #endregion

        public DataTemplate VerticalValueTemplate { get; set; }

        public DataTemplate HorizontalValueTemplate { get; set; }

        #region [ ValuesSource ]
        public static readonly DependencyProperty ValuesSourceProperty = DependencyProperty.Register(
            "ValuesSource",
            typeof(IEnumerable),
            typeof(GenericValueOverlay),
            new PropertyMetadata(ValuesSourceChanged));

        public IEnumerable ValuesSource
        {
            get { return (IEnumerable)this.GetValue(ValuesSourceProperty); }
            set { this.SetValue(ValuesSourceProperty, value); }
        }

        private static void ValuesSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var valueOverlay = sender as GenericValueOverlay;
            if (valueOverlay != null)
            {
                args.TryAddCollectionChangedHandler(valueOverlay.ValuesSourceCollectionChanged);
                args.TryAddPropertyChangedHandler(valueOverlay.ValuesSourcePropertyChanged);
            }
        }

        private void ValuesSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args != null)
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        this._labelPanel.Children.Clear();
                        break;
                    case NotifyCollectionChangedAction.Add:
                        args.NewItems.ForEach(item =>
                        {
                            if (item != null)
                            {
                                var uiElement = item as UIElement;
                                if (uiElement != null)
                                    this._labelPanel.Children.Add(uiElement);
                                else
                                {
                                    DataTemplate template;
                                    if (this._xAxes.Contains(this.Axis.GetType()) && this.VerticalValueTemplate != null)
                                        template = this.VerticalValueTemplate;
                                    else if (this._yAxes.Contains(this.Axis.GetType()) && this.HorizontalValueTemplate != null)
                                        template = this.HorizontalValueTemplate;
                                    else
                                        return;

                                    var dataType = template.DataType as Type;

                                    if (dataType == null || dataType == item.GetType() || item.GetType().IsSubclassOf(dataType)
                                        || item.GetType().GetInterfaces().Any(iface => iface == dataType))
                                    {
                                        uiElement = template.MakeObject(item);
                                        if (uiElement != null)
                                            this._labelPanel.Children.Add(uiElement);
                                    }
                                }
                            }
                        });
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        args.OldItems.ForEach(item =>
                        {
                            if (item != null)
                            {
                                var uiElement = item as UIElement;
                                if (uiElement != null)
                                    this._labelPanel.Children.Remove(uiElement);
                                else
                                {
                                    foreach (var child in this._labelPanel.Children.OfType<FrameworkElement>())
                                        if (child.IsPropertyValueEquals(FrameworkElement.DataContextProperty, item))
                                        {
                                            this._labelPanel.Children.Remove(child);
                                            break;
                                        }
                                }
                            }
                        });
                        break;
                }
            }
        }

        private void ValuesSourcePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            
        }
        #endregion

        public GenericValueOverlay()
        {
            this.DefaultStyleKey = typeof(GenericValueOverlay);
        }

        private Line _line;
        
        private readonly StackPanel _labelPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        public StackPanel LabelPanel
        {
            get { return this._labelPanel; }
        }

        private readonly List<Type> _xAxes = new List<Type>
        {
            typeof(CategoryXAxis),
            typeof(CategoryDateTimeXAxis),
            typeof(MvvmCategoryDateTimeXAxis)
        };

        private readonly List<Type> _yAxes = new List<Type>
        {
            typeof(NumericYAxis),
            typeof(MvvmNumericYAxis)
        };

        protected override void RenderSeriesOverride(bool animate)
        {
            base.RenderSeriesOverride(animate);

            if (this.Viewport.IsEmpty || this.RootCanvas == null || this.Axis == null)
                return;

            var scalerParams = new ScalerParams(this.SeriesViewer.WindowRect, this.Viewport, Axis.IsInverted);

            var value = this.GetValue();
            
            var position = this.Axis.GetScaledValue(value, scalerParams);

            if (this._line == null)
            {
                this.RootCanvas.Children.Clear();

                this._line = new Line
                {
                    Stroke = this.Brush,
                    StrokeThickness = this.Thickness,
                    Opacity = this.Opacity
                };
                this.RootCanvas.Children.Add(this._line);
                this.RootCanvas.Children.Add(this._labelPanel);
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

                Canvas.SetTop(this._labelPanel, 0);
                Canvas.SetLeft(this._labelPanel, position + this.Thickness / 2);
            }
            else if (this._yAxes.Contains(this.Axis.GetType()))
            {
                this._line.X1 = 0;
                this._line.X2 = this.RootCanvas.ActualWidth;
                this._line.Y1 = position;
                this._line.Y2 = position;

                Canvas.SetBottom(this._labelPanel, this.RootCanvas.ActualHeight - position + this.Thickness / 2);
                Canvas.SetLeft(this._labelPanel, 0);
            }
        }

        private double _yValue;
        public double YValue
        {
            get
            {
                return this._yValue;
            }
            set
            {
                if (Math.Abs(this._yValue - value) > 0.000000001)
                {
                    var oldValue = this._yValue;
                    this._yValue = value;
                    this.RaisePropertyChanged("YValue", oldValue, this._yValue);
                }
            }
        }

        private double GetValue()
        {
            var value = default(double);
            if (this.Axis is CategoryDateTimeXAxis)
            {
                var axis = this.Axis as CategoryDateTimeXAxis;
                value = axis.ActualMinimumValue.Ticks + (axis.ActualMaximumValue - axis.ActualMinimumValue).Ticks * this.Share;
            }
            if (this.Axis is NumericYAxis)
            {
                value = this.YValue;
            }
            return value;
        }

        protected override AxisRange GetRange(Axis axis)
        {
            var value = this.GetValue();
            return new AxisRange(value - 1, value + 1);
        }

        protected override void RenderThumbnail(Rect viewportRect, RenderSurface surface)
        {
            base.RenderThumbnail(viewportRect, surface);

            if (this.Axis == null || this.RootCanvas == null)
                return;

            var scalerParams = new ScalerParams(new Rect(0, 0, 1, 1), viewportRect, this.Axis.IsInverted);

            var value = this.GetValue();

            var x = this.Axis.GetScaledValue(value, scalerParams);

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

                case "Share":
                case "YValue":
                    {
                        if (this.Axis != null)
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
