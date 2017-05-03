using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataUtils.InfocardMap;
using DataUtils.Imports;
using System.IO;
using System.Reflection;

namespace DataUtils
{
    class Program
    {
        public const string fl_path = @"H:\git\game-repository\";
        public const string infocards_path = @"SERVICE\infocards.txt";
        public const string systems_path = @"DATA\UNIVERSE\universe.ini";
        public const string ships_path = @"DATA\SHIPS\shiparch.ini";
        public const string factions_path = @"DATA\initialworld.ini";


        static void Main(string[] args)
        {
            List<Infocard> infocards = new List<Infocard>();
            Dictionary<string, string> systems = new Dictionary<string, string>();
            Dictionary<string, string> ships = new Dictionary<string, string>();
            Dictionary<string, string> factions = new Dictionary<string, string>();

            var dt1 = DateTime.Now;
            Console.WriteLine("Please wait while infocards are being loaded.");
            infocards = LoadInfocards();
            var dt2 = DateTime.Now;
            TimeSpan span = dt2 - dt1;
            Console.WriteLine(string.Format("Loaded infocards in {0}s.", span.TotalSeconds));

            if (infocards.Count > 0)
            {
                systems = LoadSystems(infocards);
                ships = LoadShips(infocards);
                factions = LoadFactions(infocards);
            }

            //Make SQL
            MakeSQL(systems, ships, factions);
      
            Console.ReadLine();
        }

        static List<Infocard> LoadInfocards()
        {
            var infocards = new List<Infocard>();
            try
            {
                infocards = InfocardMap.Load(fl_path + infocards_path);
                return infocards;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load infocards file. Check if the file has not received bogus data such as git diff information.");
                return new List<Infocard>();
            }
        }

        static Dictionary<string, string> LoadSystems(List<Infocard> infocards)
        {
            var dico = new Dictionary<string, string>();
            var data = new FLDataFile(fl_path + systems_path, true);
            foreach (var item in data.sections.Where(c => c.sectionName.ToUpper() == "SYSTEM"))
            {
                var nickname = item.GetSetting("nickname").values.First().ToString().ToUpper();
                var name_ids = int.Parse(item.GetSetting("strid_name").values.First().ToString());
                var infocard = infocards.SingleOrDefault(c => c.id == name_ids);

                //don't process multiverse entries
                if (nickname.Contains("SECTOR"))
                    continue;

                dico.Add(nickname, infocard.content);
            }

            return dico;
        }

        static Dictionary<string, string> LoadShips(List<Infocard> infocards)
        {
            var dico = new Dictionary<string, string>();
            var data = new FLDataFile(fl_path + ships_path, true);
            foreach (var item in data.sections.Where(c => c.sectionName.ToUpper() == "SHIP"))
            {
                var nickname = item.GetSetting("nickname").values.First().ToString().ToUpper();
                var name_ids = int.Parse(item.GetSetting("ids_name").values.First().ToString());
                var infocard = infocards.SingleOrDefault(c => c.id == name_ids);
                dico.Add(nickname, infocard.content);
            }

            return dico;
        }

        static Dictionary<string, string> LoadFactions(List<Infocard> infocards)
        {
            var dico = new Dictionary<string, string>();
            var data = new FLDataFile(fl_path + factions_path, true);
            foreach (var item in data.sections.Where(c => c.sectionName.ToUpper() == "GROUP"))
            {
                var nickname = item.GetSetting("nickname").values.First().ToString().ToUpper();
                var name_ids = int.Parse(item.GetSetting("ids_name").values.First().ToString());
                var infocard = infocards.SingleOrDefault(c => c.id == name_ids);
                dico.Add(nickname, infocard.content);
            }

            return dico;
        }

        static void MakeSQL(Dictionary<string, string> systems, Dictionary<string, string> ships, Dictionary<string, string> factions)
        {
            using (StreamWriter sw = new StreamWriter((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\output.txt"), false))
            {
                sw.WriteLine("DELETE FROM factions;");
                foreach (var item in factions)
                {
                    sw.WriteLine(string.Format("INSERT INTO factions (nickname, name) VALUES ('{0}', '{1}');", item.Key, item.Value.Replace("'", "\\'")));
                }
                sw.WriteLine("DELETE FROM ships;");
                foreach (var item in ships)
                {
                    sw.WriteLine(string.Format("INSERT INTO ships (nickname, name) VALUES ('{0}', '{1}');", item.Key, item.Value.Replace("'", "\\'")));
                }
                sw.WriteLine("DELETE FROM systems;");
                foreach (var item in systems)
                {
                    sw.WriteLine(string.Format("INSERT INTO systems (nickname, name) VALUES ('{0}', '{1}');", item.Key, item.Value.Replace("'", "\\'")));
                }
            }

            Console.WriteLine("Created SQL statement");
        }
    }
}
