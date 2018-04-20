using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BarCodeGennerater
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe ", AppDomain.CurrentDomain.BaseDirectory);
                string error = "\r\n" + ((Exception)e.ExceptionObject).Message + "\r\n 导致错误的对象名称:" +
                               ((Exception)e.ExceptionObject).Source + "\r\n 引发异常的方法:" +
                               ((Exception)e.ExceptionObject).TargetSite + "\r\n  帮助链接:" +
                               ((Exception)e.ExceptionObject).HelpLink + "\r\n 调用堆:" +
                               ((Exception)e.ExceptionObject).StackTrace;
                FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + @"log.txt", FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(error);
                sw.Flush();
                sw.Close();
                fs.Close();
            }
            catch (Exception exception)
            {

            }
        }

        public static void PrinterException(Exception e)
        {
            try
            {

                string error = "\r\n" + e.Message + "\r\n 导致错误的对象名称:" + e.Source +
                               "\r\n 引发异常的方法:" + e.TargetSite + "\r\n  帮助链接:" +
                               e.HelpLink + "\r\n 调用堆:" + e.StackTrace;
                FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + @"log.txt", FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(error);
                sw.Flush();
                sw.Close();
                fs.Close();
            }
            catch (Exception exception)
            {

            }
        }
        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;
                System.Diagnostics.Process.Start("explorer.exe ", AppDomain.CurrentDomain.BaseDirectory);
                string error = "\r\n" + e.Exception.Message + "\r\n 导致错误的对象名称:" + ((Exception)e.Exception).Source +
                               "\r\n 引发异常的方法:" + ((Exception)e.Exception).TargetSite + "\r\n  帮助链接:" +
                               ((Exception)e.Exception).HelpLink + "\r\n 调用堆:" + ((Exception)e.Exception).StackTrace;
                FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + @"log.txt", FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(error);
                sw.Flush();
                sw.Close();
                fs.Close();
            }
            catch (Exception exception)
            {

            }
        }
    }
}
