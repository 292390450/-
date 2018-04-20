using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Xps;
using GalaSoft.MvvmLight.Ioc;

namespace BarCodeGennerater.Common
{
    public class Printer
    {
        public LocalPrintServer PrintServer { get; private set; }
        public PrintQueueCollection QueueCollection { get;private set; }
        public PrintQueue CurrentQueue { get; private set; }
        public PrintTicket Ticket { get; private set; }
        public Printer()
        {
            PrintServer=new LocalPrintServer();
            //打印机队列集合意思就是不同打印机的队列集合
            QueueCollection = PrintServer.GetPrintQueues();
            CurrentQueue = PrintServer.DefaultPrintQueue;
            Ticket = CurrentQueue.DefaultPrintTicket;
        }

        public  XpsDocumentWriter GetDocumentWriter()
        {
           return PrintQueue.CreateXpsDocumentWriter(CurrentQueue);
        }

        public void SetQueue(PrintQueue queue)
        {
            CurrentQueue = queue;
            Ticket = CurrentQueue.DefaultPrintTicket;
        }


        public static Printer GetPrinter()
        {
           return  SimpleIoc.Default.GetInstance<Printer>();
        }
    }
}
