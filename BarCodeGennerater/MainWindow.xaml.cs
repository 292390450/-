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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BarCodeGennerater.Common;
using BarCodeGennerater.Helper;
using BarCodeGennerater.ViewModel;

namespace BarCodeGennerater
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
           Get();
            //ConfigHelper.SetVaiueAsync("hospitalname",gg+"hello");
            //gg = ConfigHelper.GetValueSync("hospitalname").Result;
            this.Closed += MainWindow_Closed;
            
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
           App.Current.Shutdown();
        }

        private async void Get()
        {
            string hh= await ConfigHelper.GetValueSync("hospitalname");
        }
    }
}
