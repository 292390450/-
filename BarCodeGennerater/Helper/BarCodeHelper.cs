using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace BarCodeGennerater.Helper
{

    public class BarCodeHelper
    {
        private BarcodeFormat _currentFormat;

        public BarCodeHelper()
        {
            
        }

        public  async Task<Bitmap> CreateBarCode(string checkType,string checkNum)
        {
            var type = await ConfigHelper.GetValueSync("codetype");
            switch (type)
            {
                case "QR_CODE":
                    _currentFormat = BarcodeFormat.QR_CODE;
                    break;
                case "CODE_128":
                    _currentFormat = BarcodeFormat.CODE_128;
                    break;
                default:
                    _currentFormat = BarcodeFormat.CODE_128;
                    break;
            }

            var height = await ConfigHelper.GetValueSync("barcodeheight");
            var width = await ConfigHelper.GetValueSync("barcodewidth");
            EncodingOptions options = new QrCodeEncodingOptions()
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = int.Parse(width),
                Height = int.Parse(height)
            };
            BarcodeWriter writer=new BarcodeWriter();
            writer.Format = _currentFormat;
            writer.Options = options;
            return writer.Write(checkType + checkNum);
        }
    }

    public enum CodeType
    {
        
        CODE_128=0,
        QR_CODE=1
    }
}
