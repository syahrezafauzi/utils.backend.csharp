using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zauzi.csharp.net.utils.Logging
{
    public class LogUtils
    {
        public static LogUtils Default = new LogUtils(Folder.DEFAULT);
        private Folder folder;

        public LogUtils(Folder folder)
        {
            this.folder = folder;
        }

        public void Write(Exception ex)
        {
            Console.WriteLine(ex);
            Debug.WriteLine(ex);
        }

        public void Write(String message)
        {
            Console.WriteLine(message);
            Debug.WriteLine(message);
        }

        public class Folder
        {
            public static Folder DEFAULT => new Folder();
        }
    }
}
