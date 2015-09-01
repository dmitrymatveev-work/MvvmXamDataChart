using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sample
{
    /// <summary>
    /// Interaction logic for Sample.xaml
    /// </summary>
    public partial class Sample : Window
    {
        public Sample()
        {
            InitializeComponent();
        }

        private readonly SampleViewModel _viewModel = new SampleViewModel();
        public SampleViewModel ViewModel
        {
            get { return this._viewModel; }
        }
    }
}
