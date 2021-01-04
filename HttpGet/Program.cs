using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseUtils;
using BaseUtils.JSON;

// debug space for http code

namespace HttpGet
{
    class Program
    {
        static void Main(string[] args)
        {
            GitHubClass p = new GitHubClass("https://api.github.com/repos/EDDiscovery/EDDiscovery/");
            JArray r = p.GetAllReleases(100);

            //string releases = File.ReadAllText(@"c:\code\releases.txt");
            //JArray r = JArray.Parse(releases);


            foreach (JObject t in r)
            {
                var url = t["url"].Value.ToNullSafeString();
                var tag = t["tag_name"].Value.ToNullSafeString();
                var name = t["name"].Value.ToNullSafeString();
                var pdate = t["published_at"].DateTime(CultureInfo.InvariantCulture);

                Console.Write("\"" + tag +"\",\"" + name +"\"," + pdate);

                int downloadeddiscovery = 0;
                int downloadportable = 0;

                JArray assets = t["assets"] as JArray;
                foreach (JObject asset in assets)
                {
                    var aname = asset["name"].Value.ToNullSafeString();
                    var download = asset["download_count"].Int();

                    if (aname.Contains(".exe"))
                    {
                        downloadeddiscovery = download;
                    }
                    else
                        downloadportable = download;

                }
                Console.Write("," + downloadeddiscovery + ","  + downloadportable);

                Console.WriteLine("");
            }
        }
    }
}
