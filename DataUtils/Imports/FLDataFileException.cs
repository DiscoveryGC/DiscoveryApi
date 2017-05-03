using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataUtils.Imports
{
    class FLDataFileException : Exception
    {
        public FLDataFileException(string msg) : base(msg) { }
    }
}
