using System;
using System.Collections.Generic;
using System.Text;

namespace BaseUtils
{
    public interface IAdditionalKeyParser
    {
        Tuple<string, int, string> Parse(string s);      // return replace key string, or null if not recognised.  int is parse length, Any errors signal in second string or null
    }
}
