using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BarCodeGennerater.Helper
{
    public class ConfigHelper
    {
        #region 变量

       

        #endregion

        #region 私有方法

        
        private static bool IsExist()
        {
           return File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Config/Config.xml");
        }

        #endregion

        #region 共有方法

        public static async Task<string> GetValueSync(string key)
        {
            return await Task.Run((() =>
            {
                if (IsExist())
                {
                    try
                    {
                        XDocument config = XDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "Config/Config.xml");
                        XElement root = config.Root;
                       
                        return root.Element(key).Value;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return String.Empty;
                    }
                }

                return String.Empty;
            }));
        }
        public static async  Task SetVaiueAsync(string key, string value)
        {
            await Task.Run((() =>
            {
                if (IsExist())
                {
                    try
                    {
                        XDocument config = XDocument.Load(AppDomain.CurrentDomain.BaseDirectory + "Config/Config.xml");
                        XElement root = config.Root;
                        root.Element(key).SetValue(value);
                        config.Save(AppDomain.CurrentDomain.BaseDirectory + "Config/Config.xml");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }));
        }

       
        #endregion
    }
}
