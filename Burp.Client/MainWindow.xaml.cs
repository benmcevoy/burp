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
using System.Net;
using System.Configuration;
using Burp.Model;
using Burp.Services;

namespace Burp.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            _vm = new MainViewModel(this.Dispatcher);

            this.DataContext = _vm;

            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);

            _vm.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_vm_PropertyChanged);
        }

        void _vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Messages")
            {
                MessageScroller.ScrollToEnd();
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _vm.Dispose();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift))
            {
                e.Handled = true;

                ((TextBox)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
                _vm.SendCommand.Execute(null);
            }
        }
    }
}
