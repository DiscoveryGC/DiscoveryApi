using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DataUtils
{
    public static class InfocardMap
    {
        public struct Infocard
        {
            //The ID
            public int id { get; set; }
            //False is a NAME infocard, True is an INFOCARD type infocard
            public bool type { get; set; }
            //The content of that infocard
            public string content { get; set; }
        }

        public static List<Infocard> Load(string path)
        {
            var Infocards = new List<Infocard>();
            var file = File.OpenRead(path);
            using (var sr = new StreamReader(file))
            {
                while (sr.EndOfStream == false)
                {
                    var line = sr.ReadLine();
                    if (!string.IsNullOrEmpty(line.Replace(" ", String.Empty)))
                    {
                        var inf = new Infocard();
                        //Infocards file work as blocks of three lines separated by empty lines
                        inf.id = int.Parse(line);
                        line = sr.ReadLine();
                        inf.type = line == "NAME" ? false : true;
                        line = sr.ReadLine();
                        inf.content = line;
                        Infocards.Add(inf);                        
                    }
                }
            }

            return Infocards;
        }
    }
}
