using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;
using devDept.Eyeshot;
using devDept.Graphics;

namespace WpfApplication1
{
    public class ColorTableToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string retVal = string.Empty;
            if (value is ObservableCollection<Brush>)
            {
                switch (((ObservableCollection<Brush>)value).Count)
                {
                    case 9:
                        {
                            retVal = "Red to Blue 9";
                            break;
                        }
                    case 17:
                        {
                            retVal = "Red to Blue 17";
                            break;
                        }
                    case 33:
                        {
                            retVal = "Red to Blue 33";
                            break;
                        }
                }
            }
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }        

    public class ViewportEnumsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is backgroundStyleType)
            {
                return ((backgroundStyleType)value).ToString();
            }

            if (value is coordinateSystemPositionType)
            {
                return ((coordinateSystemPositionType)value).ToString();
            }

            if (value is originSymbolStyleType)
            {
                return ((originSymbolStyleType)value).ToString();
            }

            if (value is ToolBar.positionType)
            {
                return ((ToolBar.positionType) value).ToString();
            }

            if (value is displayType)
            {
                return ((displayType) value).ToString();
            }

            if (value is actionType)
            {
                return ((actionType)value).ToString();
            }

            if (value is colorThemeType)
            {
                return ((colorThemeType)value).ToString();
            }

            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {        
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }     
    }

    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is System.Windows.Media.Color)
            {
                return Helper.ConvertColor((System.Windows.Media.Color)value);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ColorToDrawingColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is System.Windows.Media.Color)
            {
                return Helper.ConvertDrawingColor((System.Windows.Media.Color)value);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public static class Helper
    {
        public static Color ConvertColor(Brush brush)
        {
            SolidColorBrush newBrush = (SolidColorBrush)brush;
            return newBrush.Color;
        }

        public static Brush ConvertColor(Color color)
        {
            return new SolidColorBrush(color);
        }

        public static System.Drawing.Color ConvertDrawingColor(Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Converts hex color string to <see cref="System.Drawing.Color"/>
        /// </summary>
        /// <param name="hexColor">Hex color like "#FF434752"</param>
        /// <returns>The <see cref="System.Drawing.Color"/></returns>
        public static System.Drawing.Color ConvertColor(string hexColor)
        {            
            return System.Drawing.ColorTranslator.FromHtml(hexColor);
        }

        public static Uri GetUriFromResource(string resourceFilename)
        {
            return new Uri(@"pack://application:,,,/Resources/" + resourceFilename);
        }
    }
}
