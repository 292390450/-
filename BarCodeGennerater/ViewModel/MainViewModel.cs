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
        #region ����

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

        #region ����

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
                if (IsIncrease)//��������Զ�����
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
                if (value)//�����Զ�����
                {
                    //д����ʼ��Ҫ�����ƥ��
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

        #region ����
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
            //��ʼ�����������
            codeHelper=new BarCodeHelper();
           //ע���ӡ����
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
            /// ��ʼ������
            InitCommand();
        }

        #region  ��ʼ������

        private void InitCommand()
        {
            //Ԥ������
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
            //��������
            SettingCommand=new RelayCommand((() =>
            {
                //����ҳ��
                if (_settingWindow==null)
                {
                    _settingWindow = new SettingView();
                }

                _settingWindow.ShowDialog();
            }));
            //��ӡ
            PrintCommand=new RelayCommand((async () =>
            {


                XpsDocumentWriter writer = _printerService.GetDocumentWriter();
                var doc=await CreateFixedDocumentAsync();
                writer.Write(doc,_printerService.Ticket);
                //��ӡ�������ж��Ƿ����Զ�����
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
            //popup��ʾ
            PopupCommand=new RelayCommand((() =>
            {
                IsPopupOpen = true;
            }));
        }

        #endregion

        #region ��ӡ���

        private async System.Threading.Tasks.Task<FixedDocument> CreateFixedDocumentAsync()
        {
            FixedDocument fixedDocument=new FixedDocument();
            var  pageContent=new PageContent();
            fixedDocument.Pages.Add(pageContent);
            var  page=new FixedPage();
            pageContent.Child = page;
            //��������

            var container=CreateContainer(int.Parse(await ConfigHelper.GetValueSync("leftmargin")), int.Parse(await ConfigHelper.GetValueSync("topmargin")));
            string hospitalName = await ConfigHelper.GetValueSync("hospitalname");
            container=await CreateCheckNumAndNameAsync(container, CheckNum, Name, hospitalName,CheckType);
            container =await CreateBarCodeAsync(container, CheckType, CheckNum);
            page.Children.Add(container);
            return fixedDocument;
        }
        /// <summary>
        /// ����һ��grid����
        /// </summary>
        /// <returns></returns>
        private UIElement CreateContainer(double leftOffset,double topOffset)
        {
            var grid=new Grid();
            grid.RowDefinitions.Add(new RowDefinition());  //ҽԺ��
            grid.RowDefinitions.Add(new RowDefinition());  //����
            grid.RowDefinitions.Add(new RowDefinition());  //����
            grid.RowDefinitions.Add(new RowDefinition());  //����
            grid.RowDefinitions.Add(new RowDefinition());   //ʱ��
            FixedPage.SetLeft(grid,leftOffset);
            FixedPage.SetTop(grid,topOffset);
            return grid;
        }
    
        /// <summary>
        /// ���������벿��
        /// </summary>
        /// <param name="container"></param>
        /// <param name="checkNum"></param>
        /// <param name="name"></param>
        /// <param name="hospitalName"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task<UIElement> CreateCheckNumAndNameAsync(UIElement container, string checkNum,string name, string hospitalName,string checktype)
        {
            var fontSize =int.Parse(await ConfigHelper.GetValueSync("fontsize"));
            //ҽԺ��
            var hospitalNameText = new TextBlock
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = fontSize,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            hospitalNameText.Inlines.Add(hospitalName);
            Grid.SetRow(hospitalNameText,0);
            //����
            var checkNumText=new TextBlock
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = fontSize,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            checkNumText.Inlines.Add("Ӱ��ţ�");
            if (!string.IsNullOrWhiteSpace(checktype))
            {
                checkNumText.Inlines.Add(checktype.Trim());
            }
            checkNumText.Inlines.Add(checkNum+"");
            Grid.SetRow(checkNumText, 1);
            //����
            var nameText = new TextBlock
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = fontSize,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            nameText.Inlines.Add("������");
            nameText.Inlines.Add(name);
            Grid.SetRow(nameText,2);
            //����
            var dateText = new TextBlock()
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = fontSize,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            dateText.Inlines.Add("��ӡʱ�䣺");
            dateText.Inlines.Add(DateTime.Now.ToShortDateString());
            dateText.Inlines.Add(" "+DateTime.Now.ToShortTimeString());
            Grid.SetRow(dateText,4);
            //����
            (container as Grid).Children.Add(hospitalNameText);
            (container as Grid).Children.Add(checkNumText);
            (container as Grid).Children.Add(nameText);
            (container as Grid).Children.Add(dateText);
            return container;
        }
       /// <summary>
       /// ��������
       /// </summary>
       /// <param name="container">����</param>
       /// <param name="checkType">�������</param>
       /// <param name="checkNum">����</param>
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

        #region ���
        private bool CheckInt(string value)
        {
            int val;
            if (!int.TryParse(value,out val))
            {
                return false;
            }

            //һλ����λ��֮��Ĵ�С
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