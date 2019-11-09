using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using devDept.Eyeshot;
using devDept.Graphics;

namespace WpfApplication1.Models
{
    public class MyModel : Model
    {
        public static readonly DependencyProperty MyEntityListProperty = DependencyProperty.Register(
            "MyEntityList", typeof(MyEntityList), typeof(MyModel), new FrameworkPropertyMetadata(default(MyEntityList), OnMyEntityListChanged));

        private static void OnMyEntityListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var vp = (MyModel)d;
            var oldValue = (MyEntityList)e.OldValue;
            if (oldValue != null)
                oldValue.Model = null;
            var newValue = (MyEntityList)e.NewValue;
            if (newValue != null)
                newValue.Model = vp;
        }

        public MyEntityList MyEntityList
        {
            get { return (MyEntityList)GetValue(MyEntityListProperty); }
            set { SetValue(MyEntityListProperty, value); }
        }
    }
}
