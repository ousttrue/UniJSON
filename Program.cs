using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniJSONPRofiling
{
    class Program
    {
        static void Main(string[] args)
        {
            var json = File.ReadAllText(args[0], Encoding.UTF8);
            var parsed = UniJSON.JsonParser.Parse(json);

        }
    }
}
