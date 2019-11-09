using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class TangentsWindow : Window
    {
        public double TangentRadius = 10;

        private bool _lineTangents = true;

        public bool LineTangents
        {
            get { return _lineTangents; }
            set { _lineTangents = value; }
        } 
        
        public bool CircleTangents { get; set; }
        
        public bool TrimTangents { get; set; }
        
        public bool FlipTangents { get; set; }
        public TangentsWindow()
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
            linesRadioButton.IsChecked = true;
            linesRadioButton.Click += LinesRadioButton_Click;
            circlesRadioButton.Click += circlesRadioButton_Click;
            trimCheckBox.Checked += trimCheckBox_CheckedChanged;
            trimCheckBox.Unchecked += trimCheckBox_CheckedChanged;
            flipCheckBox.Checked += flipCheckBox_CheckedChanged;
            flipCheckBox.Unchecked += flipCheckBox_CheckedChanged;
            radiusTextBox.TextChanged += radiusTextBox_TextChanged;
        }

        private void circlesRadioButton_Click(object sender, RoutedEventArgs e)
        {
            CircleTangents = circlesRadioButton.IsChecked.Value;
            LineTangents = linesRadioButton.IsChecked.Value;
            radiusTextBox.IsEnabled = circlesRadioButton.IsChecked.Value;
            radiusLabel.IsEnabled = circlesRadioButton.IsChecked.Value;   
            optionsGroupBox.IsEnabled=circlesRadioButton.IsChecked.Value;
        }

        private void LinesRadioButton_Click(object sender, RoutedEventArgs e)
        {
            CircleTangents = circlesRadioButton.IsChecked.Value;
            LineTangents = linesRadioButton.IsChecked.Value;
            radiusTextBox.IsEnabled = circlesRadioButton.IsChecked.Value;
            radiusLabel.IsEnabled = circlesRadioButton.IsChecked.Value; 
            optionsGroupBox.IsEnabled=circlesRadioButton.IsChecked.Value;
        }

        

        private void radiusTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            double val;
            if (Double.TryParse(radiusTextBox.Text, out val))
            {
                TangentRadius = val;
            }
        }

        private void flipCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            FlipTangents = flipCheckBox.IsChecked.Value;
        }

        private void trimCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            TrimTangents = trimCheckBox.IsChecked.Value;
        }

       

        private void selectButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
            
        }
    }
}
