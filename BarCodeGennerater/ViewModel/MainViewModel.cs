using System;
using System.Drawing;
using System.Printing;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Xps;
using BarCodeGennerater.Common;
using BarCodeGennerater.Helper;
using BarCodeGennerater.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using FontFamily = System.Windows.Media.FontFamily;

namespace BarCodeGennerater.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region 变量

        private BarCodeHelper codeHelper;
        private string _checkNum;
        private string _name;
        private string _checkType=String.Empty;
        private Window _settingWindow;
        private Printer _printerService;
        private bool _isPopupOpend;
        private string _indexNum;
        private bool _isIncrease;
        private bool _isCheckNumEnable=true;
        #endregion

        #region 属性

        public string CheckNum
        {
            get { return _checkNum; }
            set
            {
                _checkNum = value;
                PreViewCommand.RaiseCanExecuteChanged();
                PrintCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                PreViewCommand.RaiseCanExecuteChanged();
                PrintCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged();
            }
        }

        public string CheckType
        {
            get { return _checkType; }
            set
            {
                _checkType = value;
                RaisePropertyChanged();
            }
        }

        public bool IsPopupOpen
        {
            get { return _isPopupOpend; }
            set
            {
                _isPopupOpend = value;
                RaisePropertyChanged();
            }
        }

        public string IndexNum
        {
            get { return _indexNum; }
            set
            {
                _indexNum = value;
                if (IsIncrease)//如果开启自动增加
                {
                    CheckNum = IndexNum;
                }
                RaisePropertyChanged();
            }
        }

        public bool IsIncrease
        {
            get { return _isIncrease; }
            set
            {
                _isIncrease = value;
                if (value)//开启自动增加
                {
                    //写入起始需要与检查号匹配
                    CheckNum = IndexNum;
                    IsCheckNumEnable = false;
                }
                else
                {
                    IsCheckNumEnable = true;
                }
                RaisePropertyChanged();
            }
        }

        public bool IsCheckNumEnable
        {
            get { return _isCheckNumEnable; }
            set
            {
                _isCheckNumEnable = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region 命令
        public RelayCommand PopupCommand { get; private set; }
        public RelayCommand PrintCommand { get; private set; }
        public RelayCommand SettingCommand { get; private set; }
        public RelayCommand PreViewCommand { get; private set; }
       
 
        #endregion
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            //初始化条码帮助类
            codeHelper=new BarCodeHelper();
           //注册打印服务
            SimpleIoc.Default.Register<Printer>();
            _printerService = Printer.GetPrinter();
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
            /// 初始化命令
            InitCommand();
        }

        #region  初始化方法

        private void InitCommand()
        {
            //预览命令
            PreViewCommand = new RelayCommand((async () =>
            {
              var  TestWindow=new TestWindow();
                TestWindow.Viewer.Document =await CreateFixedDocumentAsync();
                TestWindow.Show();
            }),(() =>
            {
                if (!string.IsNullOrWhiteSpace(Name)&&CheckInt(CheckNum))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }));
            //设置命令
            SettingCommand=new RelayCommand((() =>
            {
                //设置页面
                if (_settingWindow==null)
                {
                    _settingWindow = new SettingView();
                }

                _settingWindow.ShowDialog();
            }));
            //打印
            PrintCommand=new RelayCommand((async () =>
            {


                XpsDocumentWriter writer = _printerService.GetDocumentWriter();
                var doc=await CreateFixedDocumentAsync();
                writer.Write(doc,_printerService.Ticket);
                //打印结束，判断是否开启自动增加
                if (IsIncrease)
                {
                    CheckNum = (int.Parse(CheckNum) + 1)+"";
                }
            }), (() =>
            {
                if (!string.IsNullOrWhiteSpace(Name) && CheckInt(CheckNum))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }));
            //popup显示
            PopupCommand=new RelayCommand((() =>
            {
                IsPopupOpen = true;
            }));
        }

        #endregion

        #region 打印相关

        private async System.Threading.Tasks.Task<FixedDocument> CreateFixedDocumentAsync()
        {
            FixedDocument fixedDocument=new FixedDocument();
            var  pageContent=new PageContent();
            fixedDocument.Pages.Add(pageContent);
            var  page=new FixedPage();
            pageContent.Child = page;
            //创建内容

            var container=CreateContainer(int.Parse(await ConfigHelper.GetValueSync("leftmargin")), int.Parse(await ConfigHelper.GetValueSync("topmargin")));
            string hospitalName = await ConfigHelper.GetValueSync("hospitalname");
            container=await CreateCheckNumAndNameAsync(container, CheckNum, Name, hospitalName,CheckType);
            container =await CreateBarCodeAsync(container, CheckType, CheckNum);
            page.Children.Add(container);
            return fixedDocument;
        }
        /// <summary>
        /// 创建一个grid容器
        /// </summary>
        /// <returns></returns>
        private UIElement CreateContainer(double leftOffset,double topOffset)
        {
            var grid=new Grid();
            grid.RowDefinitions.Add(new RowDefinition());  //医院名
            grid.RowDefinitions.Add(new RowDefinition());  //号码
            grid.RowDefinitions.Add(new RowDefinition());  //姓名
            grid.RowDefinitions.Add(new RowDefinition());  //条码
            grid.RowDefinitions.Add(new RowDefinition());   //时间
            FixedPage.SetLeft(grid,leftOffset);
            FixedPage.SetTop(grid,topOffset);
            return grid;
        }
    
        /// <summary>
        /// 创建除条码部分
        /// </summary>
        /// <param name="container"></param>
        /// <param name="checkNum"></param>
        /// <param name="name"></param>
        /// <param name="hospitalName"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task<UIElement> CreateCheckNumAndNameAsync(UIElement container, string checkNum,string name, string hospitalName,string checktype)
        {
            var fontSize =int.Parse(await ConfigHelper.GetValueSync("fontsize"));
            //医院名
            var hospitalNameText = new TextBlock
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = fontSize,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            hospitalNameText.Inlines.Add(hospitalName);
            Grid.SetRow(hospitalNameText,0);
            //检查号
            var checkNumText=new TextBlock
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = fontSize,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            checkNumText.Inlines.Add("影像号：");
            if (!string.IsNullOrWhiteSpace(checktype))
            {
                checkNumText.Inlines.Add(checktype.Trim());
            }
            checkNumText.Inlines.Add(checkNum+"");
            Grid.SetRow(checkNumText, 1);
            //姓名
            var nameText = new TextBlock
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = fontSize,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            nameText.Inlines.Add("姓名：");
            nameText.Inlines.Add(name);
            Grid.SetRow(nameText,2);
            //日期
            var dateText = new TextBlock()
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = fontSize,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            dateText.Inlines.Add("打印时间：");
            dateText.Inlines.Add(DateTime.Now.ToShortDateString());
            dateText.Inlines.Add(" "+DateTime.Now.ToShortTimeString());
            Grid.SetRow(dateText,4);
            //加入
            (container as Grid).Children.Add(hospitalNameText);
            (container as Grid).Children.Add(checkNumText);
            (container as Grid).Children.Add(nameText);
            (container as Grid).Children.Add(dateText);
            return container;
        }
       /// <summary>
       /// 创建条码
       /// </summary>
       /// <param name="container">容器</param>
       /// <param name="checkType">检查类型</param>
       /// <param name="checkNum">检查号</param>
       /// <returns></returns>
        private async System.Threading.Tasks.Task<UIElement> CreateBarCodeAsync(UIElement container,string checkType,string checkNum)
        {
           Bitmap bitmap= await codeHelper.CreateBarCode(checkType, checkNum+"");
            System.Windows.Controls.Image codeImage =new System.Windows.Controls.Image();
            codeImage.Margin=new Thickness(0,5,0,5);
           codeImage.Source=ImageHelper.ChangeBitmapToImageSource(bitmap);
            Grid.SetRow(codeImage,3);
            (container as Grid).Children.Add(codeImage);
            return container;
        }

        #endregion

        #region 检查
        private bool CheckInt(string value)
        {
            int val;
            if (!int.TryParse(value,out val))
            {
                return false;
            }

            //一位到九位数之间的大小
            string patten = "[0-9]{1,9}";
            Regex regex = new Regex(patten);
            var res = regex.Match(value + "");
            if (res.Value == value + "")
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}