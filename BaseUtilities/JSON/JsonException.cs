using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseUtils.JSON
{
    public class JsonException : InvalidOperationException
    {
        public string JSON { get; set; }
        public int Offset { get; set; }

        public JsonException(string json, int offset, string message) : base(message)
        {
            JSON = json;
            Offset = offset;
        }
    }
}
