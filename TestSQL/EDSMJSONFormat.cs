using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCore.EDSM
{

    public class EDSMDumpSystem
    {
        public bool Deserialize(JsonReader rdr)
        {
            while (rdr.Read() && rdr.TokenType == JsonToken.PropertyName)
            {
                string field = rdr.Value as string;
                switch (field)
                {
                    case "name":
                        name = rdr.ReadAsString();
                        break;
                    case "id":
                        id = rdr.ReadAsInt32() ?? 0;
                        break;
                    case "date":
                        date = rdr.ReadAsDateTime() ?? DateTime.MinValue;
                        break;
                    case "coords":
                        {
                            if (rdr.TokenType != JsonToken.StartObject)
                                rdr.Read();

                            while (rdr.Read() && rdr.TokenType == JsonToken.PropertyName)
                            {
                                field = rdr.Value as string;
                                double? v = rdr.ReadAsDouble();
                                if (v == null)
                                    return false;
                                int vi = (int)(v * 128.0);

                                switch (field)
                                {
                                    case "x":
                                        x = vi;
                                        break;
                                    case "y":
                                        y = vi;
                                        break;
                                    case "z":
                                        z = vi;
                                        break;
                                }
                            }

                            break;
                        }
                    default: 
                        rdr.Read();
                        JToken.Load(rdr);
                        break;
                }
            }

            return true;
        }

        public string name;
        public long id = -1;
        public DateTime date;
        public int x = int.MinValue;
        public int y = int.MinValue;
        public int z = int.MinValue;
    }
}
