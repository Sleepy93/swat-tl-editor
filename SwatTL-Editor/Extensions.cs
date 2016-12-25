using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwatTL_Editor
{
    public class Extensions
    {
        public delegate void ProcessFile(Stream input);

        public string title;
        public ProcessFile handler;

        public Extensions(string title, ProcessFile handler)
        {
            this.title = title;
            this.handler = handler;
        }
    }
}