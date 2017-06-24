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
        public string title;
		public Action conv;
        public Action<Stream> handler;
		public string exp;

        public Extensions(string title, Action<Stream> handler, Action conv, string exp)
        {
            this.title = title;
            this.handler = handler;
			this.conv = conv;
			this.exp = exp;
        }
    }
}