﻿using System;
using System.Windows.Controls;
using System.Windows.Data;
using Infragistics.Controls.Charts;
using MvvmXamDataChart.Extensions;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using TechCollections;

namespace MvvmXamDataChart
{
    public class MvvmXamDataChart : XamDataChart
    {
        #region [ AxesTemplateSelector ]
        public static readonly DependencyProperty AxesTemplateSelectorProperty = DependencyProperty.Register(
            "AxesTemplateSelector",
            typeof(DataTemplateSelector),
            typeof(MvvmXamDataChart),
            new PropertyMetadata(AxesTemplateSelectorChanged));

        public DataTemplateSelector AxesTemplateSelector
        {
            get { return (DataTemplateSelector)this.GetValue(AxesTemplateSelectorProperty); }
            set { this.SetValue(AxesTemplateSelectorProperty, value); }
        }

        private static void AxesTemplateSelectorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var chart = sender as MvvmXamDataChart;
            if (chart != null)
            {
                chart.AxesSourceCollectionChanged(chart,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                var newItems = new ArrayList();
                if (chart.AxesSource != null)
                    chart.AxesSource.ForEach(item => newItems.Add(item));
                chart.AxesSourceCollectionChanged(chart,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems));
            }
        }
        #endregion

        #region [ AxesSource ]
        public static readonly DependencyProperty AxesSourceProperty = DependencyProperty.Register(
            "AxesSource",
            typeof(IEnumerable),
            typeof(MvvmXamDataChart),
            new PropertyMetadata(AxesSourceChanged));

        public IEnumerable AxesSource
        {
            get { return (IEnumerable)this.GetValue(AxesSourceProperty); }
            set { this.SetValue(AxesSourceProperty, value); }
        }

        private static void AxesSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var chart = sender as MvvmXamDataChart;
            if (chart != null)
            {
                args.TryAddCollectionChangedHandler(chart.AxesSourceCollectionChanged);
                args.TryAddPropertyChangedHandler(chart.AxesSourcePropertyChanged);
            }
        }

        private void AxesSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args != null)
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        this.Axes.Clear();
                        break;
                    case NotifyCollectionChangedAction.Add:
                        foreach (var newItem in args.NewItems)
                        {
                            var axis = newItem as Axis;
                            if (axis != null)
                            {
                                this.Axes.Add(axis);
                                continue;
                            }

                            var found = false;

                            foreach (var series in this.Series.OfType<HorizontalAnchoredCategorySeries>())
                            {
                                if (series.XAxis != null && series.XAxis.IsPropertyValueEquals(FrameworkElement.DataContextProperty, newItem))
                                {
                                    this.Axes.Add(series.XAxis);
                                    found = true;
                                    break;
                                }

                                if (series.YAxis != null && series.YAxis.IsPropertyValueEquals(FrameworkElement.DataContextProperty, newItem))
                                {
                                    this.Axes.Add(series.YAxis);
                                    found = true;
                                    break;
                                }
                            }
                            if (found)
                                continue;

                            if (this.AxesTemplateSelector != null)
                            {
                                var dataTemplate = this.AxesTemplateSelector.SelectTemplate(newItem, this);
                                if (dataTemplate != null)
                                    axis = dataTemplate.LoadContent() as Axis;
                                if (axis != null)
                                    axis.DataContext = newItem;
                            }
                            else
                                axis = this.GetDataTemplate(newItem) as Axis;

                            if (axis != null)
                                this.Axes.Add(axis);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var oldItem in args.OldItems)
                            for (var i = this.Axes.Count - 1; i >= 0; i--)
                            {
                                var axis = this.Axes[i];
                                if (axis != null && axis.IsPropertyValueEquals(FrameworkElement.DataContextProperty, oldItem))
                                    this.Axes.RemoveAt(i);
                            }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        this.Axes.Move(args.OldStartingIndex, args.NewStartingIndex);
                        break;
                }
            }
        }

        private void AxesSourcePropertyChanged(object sender, PropertyChangedEventArgs args)
        {

        }
        #endregion

        #region [ SeriesTemplateSelector ]
        public static readonly DependencyProperty SeriesTemplateSelectorProperty = DependencyProperty.Register(
            "SeriesTemplateSelector",
            typeof(DataTemplateSelector),
            typeof(MvvmXamDataChart),
            new PropertyMetadata(SeriesTemplateSelectorChanged));

        public DataTemplateSelector SeriesTemplateSelector
        {
            get { return (DataTemplateSelector)this.GetValue(SeriesTemplateSelectorProperty); }
            set { this.SetValue(SeriesTemplateSelectorProperty, value); }
        }

        private static void SeriesTemplateSelectorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var chart = sender as MvvmXamDataChart;
            if (chart != null)
            {
                chart.SeriesSourceCollectionChanged(chart,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                var newItems = new ArrayList();
                if(chart.SeriesSource != null)
                    chart.SeriesSource.ForEach(item => newItems.Add(item));
                chart.SeriesSourceCollectionChanged(chart,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems));
            }
        }
        #endregion

        #region [ SeriesSource ]
        public static readonly DependencyProperty SeriesSourceProperty = DependencyProperty.Register(
            "SeriesSource",
            typeof(IEnumerable),
            typeof(MvvmXamDataChart),
            new PropertyMetadata(SeriesSourceChanged));

        public IEnumerable SeriesSource
        {
            get { return (IEnumerable)this.GetValue(SeriesSourceProperty); }
            set { this.SetValue(SeriesSourceProperty, value); }
        }

        private static void SeriesSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var chart = sender as MvvmXamDataChart;
            if (chart != null)
            {
                args.TryAddCollectionChangedHandler(chart.SeriesSourceCollectionChanged);
                args.TryAddPropertyChangedHandler(chart.SeriesSourcePropertyChanged);
            }
        }

        private void SeriesSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args != null)
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        this.Series.Clear();
                        break;
                    case NotifyCollectionChangedAction.Add:
                        foreach (var newItem in args.NewItems)
                        {
                            var series = newItem as Series;
                            if (series != null)
                            {
                                this.Series.Add(series);
                                continue;
                            }

                            if (this.SeriesTemplateSelector != null)
                            {
                                var dataTemplate = this.SeriesTemplateSelector.SelectTemplate(newItem, this);
                                if (dataTemplate != null)
                                    series = dataTemplate.LoadContent() as Series;
                                if (series != null)
                                    series.DataContext = newItem;
                            }
                            else
                                series = this.GetDataTemplate(newItem) as Series;

                            if (series != null)
                                this.Series.Add(series);
                        }

                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var oldItem in args.OldItems)
                            for (var i = this.Series.Count - 1; i >= 0; i--)
                            {
                                var series = this.Series[i];
                                if (series != null && series.IsPropertyValueEquals(FrameworkElement.DataContextProperty, oldItem))
                                    this.Series.RemoveAt(i);
                            }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        this.Series.Move(args.OldStartingIndex, args.NewStartingIndex);
                        break;
                }
            }
        }

        private void SeriesSourcePropertyChanged(object sender, PropertyChangedEventArgs args)
        {

        }
        #endregion

        #region [ Series attached ]
        public static readonly DependencyProperty XAxisSourceProperty = DependencyProperty.RegisterAttached(
            "XAxisSource",
            typeof(object),
            typeof(MvvmXamDataChart),
            new PropertyMetadata(XAxisSourceChanged));

        public static void SetXAxisSource(UIElement element, object value)
        {
            element.SetValue(XAxisSourceProperty, value);
        }

        public static object GetXAxisSource(UIElement element)
        {
            return element.GetValue(XAxisSourceProperty);
        }

        private static void XAxisSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var categorySeries = sender as HorizontalAnchoredCategorySeries;
            if (categorySeries != null)
            {
                if (args.NewValue != null)
                {
                    var xAxis = args.NewValue as CategoryAxisBase;
                    if (xAxis != null)
                    {
                        categorySeries.XAxis = xAxis;
                        return;
                    }

                    var chart = categorySeries.FindParent<XamDataChart>(parent => true);
                    if (chart != null)
                        foreach (var axis in chart.Axes)
                        {
                            xAxis = axis as CategoryAxisBase;
                            if (xAxis != null && xAxis.IsPropertyValueEquals(FrameworkElement.DataContextProperty, args.NewValue))
                            {
                                categorySeries.XAxis = xAxis;
                                return;
                            }
                        }

                    xAxis = categorySeries.GetDataTemplate(args.NewValue) as CategoryAxisBase;
                    if (xAxis != null && xAxis.IsPropertyValueEquals(FrameworkElement.DataContextProperty, args.NewValue))
                        categorySeries.XAxis = xAxis;
                }
            }

            var scatterSeries = sender as ScatterBase;
            if (scatterSeries != null)
            {
                if (args.NewValue != null)
                {
                    var xAxis = args.NewValue as NumericXAxis;
                    if (xAxis != null)
                    {
                        scatterSeries.XAxis = xAxis;
                        return;
                    }

                    var chart = scatterSeries.FindParent<XamDataChart>(parent => true);
                    if (chart != null)
                        foreach (var axis in chart.Axes)
                        {
                            xAxis = axis as NumericXAxis;
                            if (xAxis != null && xAxis.IsPropertyValueEquals(FrameworkElement.DataContextProperty, args.NewValue))
                            {
                                scatterSeries.XAxis = xAxis;
                                return;
                            }
                        }

                    xAxis = scatterSeries.GetDataTemplate(args.NewValue) as NumericXAxis;
                    if (xAxis != null && xAxis.IsPropertyValueEquals(FrameworkElement.DataContextProperty, args.NewValue))
                        scatterSeries.XAxis = xAxis;
                }
            }
        }

        public static readonly DependencyProperty YAxisSourceProperty = DependencyProperty.RegisterAttached(
            "YAxisSource",
            typeof(object),
            typeof(MvvmXamDataChart),
            new PropertyMetadata(YAxisSourceChanged));

        public static void SetYAxisSource(UIElement element, object value)
        {
            element.SetValue(YAxisSourceProperty, value);
        }

        public static object GetYAxisSource(UIElement element)
        {
            return element.GetValue(YAxisSourceProperty);
        }

        private static void YAxisSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var categorySeries = sender as HorizontalAnchoredCategorySeries;
            if (categorySeries != null)
            {
                if (args.NewValue != null)
                {
                    var yAxis = args.NewValue as NumericYAxis;
                    if (yAxis != null)
                    {
                        categorySeries.YAxis = yAxis;
                        return;
                    }

                    var chart = categorySeries.FindParent<XamDataChart>(parent => true);
                    if (chart != null)
                        foreach (var axis in chart.Axes)
                        {
                            yAxis = axis as NumericYAxis;
                            if (yAxis != null && yAxis.IsPropertyValueEquals(FrameworkElement.DataContextProperty, args.NewValue))
                            {
                                categorySeries.YAxis = yAxis;
                                return;
                            }
                        }

                    yAxis = categorySeries.GetDataTemplate(args.NewValue) as NumericYAxis;
                    if (yAxis != null && yAxis.IsPropertyValueEquals(FrameworkElement.DataContextProperty, args.NewValue))
                        categorySeries.YAxis = yAxis;
                }
            }

            var scatterSeries = sender as ScatterBase;
            if (scatterSeries != null)
            {
                if (args.NewValue != null)
                {
                    var yAxis = args.NewValue as NumericYAxis;
                    if (yAxis != null)
                    {
                        scatterSeries.YAxis = yAxis;
                        return;
                    }

                    var chart = scatterSeries.FindParent<XamDataChart>(parent => true);
                    if (chart != null)
                        foreach (var axis in chart.Axes)
                        {
                            yAxis = axis as NumericYAxis;
                            if (yAxis != null && yAxis.IsPropertyValueEquals(FrameworkElement.DataContextProperty, args.NewValue))
                            {
                                scatterSeries.YAxis = yAxis;
                                return;
                            }
                        }

                    yAxis = scatterSeries.GetDataTemplate(args.NewValue) as NumericYAxis;
                    if (yAxis != null && yAxis.IsPropertyValueEquals(FrameworkElement.DataContextProperty, args.NewValue))
                        scatterSeries.YAxis = yAxis;
                }
            }
        }

        public static readonly DependencyProperty ValueOverlayAxisSourceProperty = DependencyProperty.RegisterAttached(
            "ValueOverlayAxisSource",
            typeof(object),
            typeof(MvvmXamDataChart),
            new PropertyMetadata(ValueOverlayAxisSourceChanged));

        public static void SetValueOverlayAxisSource(UIElement element, object value)
        {
            element.SetValue(ValueOverlayAxisSourceProperty, value);
        }

        public static object GetValueOverlayAxisSource(UIElement element)
        {
            return element.GetValue(ValueOverlayAxisSourceProperty);
        }

        private static void ValueOverlayAxisSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var valueOverlay = sender as GenericValueOverlay;
            if (valueOverlay != null && args.NewValue != null)
            {
                var overlayAxis = args.NewValue as Axis;
                if (overlayAxis != null)
                {
                    valueOverlay.Axis = overlayAxis;
                    return;
                }

                var chart = valueOverlay.FindParent<XamDataChart>(parent => true);
                if (chart != null)
                    foreach (var axis in chart.Axes)
                        if (axis != null && axis.IsPropertyValueEquals(FrameworkElement.DataContextProperty, args.NewValue))
                        {
                            valueOverlay.Axis = axis;
                            return;
                        }

                overlayAxis = valueOverlay.GetDataTemplate(args.NewValue) as Axis;
                if (overlayAxis != null && overlayAxis.IsPropertyValueEquals(FrameworkElement.DataContextProperty, args.NewValue))
                    valueOverlay.Axis = overlayAxis;
            }
        }
        #endregion
    }
}
