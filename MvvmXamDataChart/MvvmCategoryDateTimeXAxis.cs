using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Infragistics.Controls.Charts;

namespace MvvmXamDataChart
{
    public class MvvmCategoryDateTimeXAxis : CategoryDateTimeXAxis
    {
        public MvvmCategoryDateTimeXAxis()
        {
            this.RangeChanged += OnRangeChanged;
        }

        public static readonly DependencyProperty ActualMaxProperty = DependencyProperty.Register(
            "ActualMax",
            typeof (object),
            typeof(MvvmCategoryDateTimeXAxis));

        public object ActualMax
        {
            get { return this.GetValue(ActualMaxProperty); }
            set { throw new NotImplementedException(); }
        }

        public static readonly DependencyProperty ActualMinProperty = DependencyProperty.Register(
            "ActualMin",
            typeof(object),
            typeof(MvvmCategoryDateTimeXAxis));

        public object ActualMin
        {
            get { return this.GetValue(ActualMinProperty); }
            set { throw new NotImplementedException(); }
        }

        private void OnRangeChanged(object sender, AxisRangeChangedEventArgs axisRangeChangedEventArgs)
        {
            this.SetValue(ActualMaxProperty, this.ActualMaximumValue);
            this.SetValue(ActualMinProperty, this.ActualMinimumValue);
        }
    }
}
