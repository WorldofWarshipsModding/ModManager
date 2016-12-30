using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace WoWS_Mod_Manager
{
    public sealed partial class TabHeader : UserControl
    {
        public TabHeader()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        
        
        public static readonly DependencyProperty LabellProperty = DependencyProperty.Register("Header", typeof(string), typeof(TabHeader), null);

        public string Header
        {
            get { return GetValue(LabellProperty) as string; }
            set { SetValue(LabellProperty, value); }
        }

        override public String ToString()
        {
            return "TabHeader";
        }
    }
}
