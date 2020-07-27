using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zauzi.chsarp.net.utils.Models
{
    public abstract class Response
    {
        public bool? success { get; set; } = false;
        public string message { get; set; }
        object _data;
        public object data
        {
            get
            {
                return this._data;
            }
            set
            {
                this._data = value;
                try
                {
                    var datatemp = value.ToString();
                    this._data = datatemp;
                }
                catch (Exception e)
                {
                    //do nothing
                }
            }
        }
    }
}
