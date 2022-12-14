using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AniTool
{
    class StatusUpdateDependancy : DependencyObject
    {
        public TextBlock MyProperty
        {
            get
            {
                return (TextBlock)GetValue(MyPropertyProperty);
            }
            set
            {
                SetValue(MyPropertyProperty, value);
            }
        }

        // Look up DependencyProperty in MSDN for details
        public static DependencyProperty MyPropertyProperty = DependencyProperty.Register("MyProperty", typeof(TextBlock), typeof(StatusUpdateDependancy), new PropertyMetadata(null));
    }
}
