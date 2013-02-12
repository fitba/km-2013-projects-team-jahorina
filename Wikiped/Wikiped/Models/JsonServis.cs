using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wikiped.Models
{
    public class JsonServis
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Result { get; set; }
    }
}