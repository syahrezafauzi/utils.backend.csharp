using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zauzi.utils.csharp.Models
{
    public abstract class Response
    {
        public bool? success { get; set; } = false;
        public object data { get; set; }
        public string message { get; set; }
    }
}
