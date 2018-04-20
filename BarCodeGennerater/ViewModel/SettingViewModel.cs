using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using BarCodeGennerater.Common;
using BarCodeGennerater.Helper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace BarCodeGennerater.ViewModel
{
    public class SettingViewModel:ViewModelBase
    {
        #region 变量

        private string _hospitalName;
        private string _barCodeHeight;
        private string _barCodeWidth;
        private string _leftMargin;
        private string _topMargin;
        private string _fontSize;
        private Visibility _winVisibility;
        private ObservableCollection<CodeType> _codeTypes;
        private ObservableCollection<string> _printers;
        private string _selectedPrinter;
        private CodeType _selectedType;
        private Printer _printerService;
        #endregion

        #region 属性

        public string HospitalName
        {
            get { return _hospitalName; }
            set
            {
                _hospitalName = value;
                RaisePropertyChanged();
            }
        }

        public string BarCodeHeight
        {
            get { return _barCodeHeight; }
            set
            {
                _barCodeHeight = value;
                RaisePropertyChanged();
            }
        }

        public string BarCodeWidth
        {
            get { return _barCodeWidth; }
            set
            {
                _barCodeWidth = value;
                RaisePropertyChanged();
            }
        }

        public string LeftMargin
        {
            get { return _leftMargin; }
            set
            {
                _leftMargin = value;
                RaisePropertyChanged();
            }
        }

        public string TopMargin
        {
            get { return _topMargin; }
            set
            {
                _topMargin = value;
                RaisePropertyChanged();
            }
        }

        public string FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                RaisePropertyChanged();
            }
        }
        public Visibility WinVisibility
        {
            get { return _winVisibility; }
            set
            {
                _winVisibility = value;
                if (value==Visibility.Visible)
                {
                    //刷新数据
                    InitDataAsync();
                }
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<CodeType> CodeTypes
        {
            get { return _codeTypes; }
            set
            {
                _codeTypes = value;
                RaisePropertyChanged();
            }
        }

        public CodeType SelectedType
        {
            get { return _selectedType; }
            set
            {
                _selectedType = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> Printers
        {
            get { return _printers; }
            set
            {
                _printers = value;
                RaisePropertyChanged();
            }
        }

        public string SelectedPrinter
        {
            get { return _selectedPrinter; }
            set
            {
                _selectedPrinter = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region 命令

        public RelayCommand<Window> SaveCommand { get; private set; }
        public RelayCommand<Window> CancelCommand { get; private set; }

        #endregion
        #region 构造

        public SettingViewModel()
        {
            InitCommand();
            
            //打印服务
           _printerService=Printer.GetPrinter();
            InitDataAsync();
        }

        #endregion

        #region 初始化方法
        /// <summary>
        /// 初始化命令
        /// </summary>
        private void InitCommand()
        {
            CancelCommand=new RelayCommand<Window>(((win) => { win.Visibility = Visibility.Collapsed; }));
            SaveCommand=new RelayCommand<Window>((async (win) =>
            {
                if (CheckInt(BarCodeHeight)&&CheckInt(BarCodeWidth)&&CheckInt(LeftMargin)&&CheckInt(TopMargin)&&CheckString(HospitalName)&&CheckInt(FontSize))
                {
                    //合法
                 await  ConfigHelper.SetVaiueAsync("hospitalname",HospitalName.Trim());
                 await   ConfigHelper.SetVaiueAsync("barcodeheight",BarCodeHeight.Trim());
                 await   ConfigHelper.SetVaiueAsync("barcodewidth",BarCodeWidth.Trim());
                 await   ConfigHelper.SetVaiueAsync("leftmargin",LeftMargin.Trim());
                 await  ConfigHelper.SetVaiueAsync("topmargin",TopMargin.Trim());
                    await ConfigHelper.SetVaiueAsync("fontsize", FontSize.Trim());
                    //选中项
                    await ConfigHelper.SetVaiueAsync("codetype", SelectedType.ToString());
                    //打印机
                    _printerService.SetQueue(_printerService.PrintServer.GetPrintQueue(SelectedPrinter));
                    MessageBox.Show("保存成功");
                    win.Visibility = Visibility.Collapsed;
                    return;
                }

                MessageBox.Show("请确保修改合法！");
            }));
        }
        /// <summary>
        /// 初始化界面数据
        /// </summary>
        private async void InitDataAsync()
        {
            try
            {
                //配置文件数据
                HospitalName = await ConfigHelper.GetValueSync("hospitalname");
                BarCodeHeight = await ConfigHelper.GetValueSync("barcodeheight");
                BarCodeWidth =await ConfigHelper.GetValueSync("barcodewidth");
                LeftMargin = await ConfigHelper.GetValueSync("leftmargin");
                TopMargin = await ConfigHelper.GetValueSync("topmargin");
                FontSize = await ConfigHelper.GetValueSync("fontsize");
                //
                if (CodeTypes==null)
                {
                    CodeTypes = new ObservableCollection<CodeType>
                    {
                        CodeType.CODE_128,
                        CodeType.QR_CODE
                    };
            
            
                }
                //条码打印格式
                var codeStr = await ConfigHelper.GetValueSync("codetype");
                var codeTy = (CodeType)Enum.Parse(typeof(CodeType), codeStr, true);
                SelectedType=CodeTypes.FirstOrDefault(x => x == codeTy);

                //打印机
                if (Printers==null)
                {
                    Printers=new ObservableCollection<string>();
                    foreach (var installedPrinter in _printerService.QueueCollection)
                    {
                        Printers.Add(installedPrinter.Name);
                    }

                    SelectedPrinter = _printerService.CurrentQueue.Name;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        #endregion

        #region 检查
        private bool CheckInt(string value)
        {
            int val;
            if (!int.TryParse(value, out val))
            {
                return false;
            }

            //一位到九位数之间的大小
            string patten = "[0-9]{1,3}";
            Regex regex = new Regex(patten);
            var res = regex.Match(value + "");
            if (res.Value == value + "")
            {
                return true;
            }

            return false;
        }
        private bool CheckInt(int value)
        {
            //一位到三位数之间的大小
            string patten = "[0-9]{1,3}";
            Regex regex=new Regex(patten);
           var res=regex.Match(value + "");
            if (res.Value==value+"")
            {
                return true;
            }

            return false;
        }

        private bool CheckString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return true;
        }
        #endregion
    }
}
