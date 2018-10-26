using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace PathTracker_Backend
{
    public class WebResourceManager {
        /// <summary>
        /// Dictionary of Level --> (AccumelatedXp, XpToLevel)
        /// </summary>
        public Dictionary<int, Tuple<long, long>> ExperienceDictionary = new Dictionary<int, Tuple<long, long>>();
        private void CalculateExperienceDictionary() {
            var assembly = Assembly.GetEntryAssembly();

            var rsStream = assembly.GetManifestResourceStream("PathTracker-Backend.Resources.ExperienceToLevel.txt");

            string fileContent = null;
            using (var reader = new StreamReader(rsStream)) {
                fileContent = reader.ReadToEnd();
            }

            if (fileContent == null) {
                throw new Exception("Could not load ExperienceToLevel.txt resource");
            }

            var fileLines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            foreach (string line in fileLines) {
                int level = -1;
                long accumXp = -1;
                long toLevel = -1;

                string[] lineContent = line.Split(';');

                if (int.TryParse(lineContent[0], out level) && long.TryParse(lineContent[1], out accumXp) && long.TryParse(lineContent[2], out toLevel)) {
                    ExperienceDictionary[level] = new Tuple<long, long>(accumXp, toLevel);
                }
                else {
                    throw new Exception("Error parsing ExperienceToLevel.txt resource into the appropiate dictionary");
                }
            }

        }
        
        public MapMods PossibleMapModsList;
        public List<string> PossibleMapModLines = new List<string>();
        public Dictionary<string, List<MapMod>> LineToMapModsDict = new Dictionary<string, List<MapMod>>();
        public void LoadMapMods() {
            var assembly = Assembly.GetEntryAssembly();

            var rsStream = assembly.GetManifestResourceStream("PathTracker-Backend.Resources.MapMods.txt");

            string fileContent = null;
            using (var reader = new StreamReader(rsStream)) {
                fileContent = reader.ReadToEnd();
            }

            
            if (fileContent == null) {
                throw new Exception("Could not load ExperienceToLevel.txt resource");
            }

            PossibleMapModsList = JsonConvert.DeserializeObject<MapMods>(fileContent);

            var modLines = PossibleMapModsList.MapModsList.SelectMany(x => x.ModLines, (x, y) => y.LineText).ToList();

            PossibleMapModLines = modLines.Distinct().ToList();

            foreach(var mod in PossibleMapModsList.MapModsList) {
                foreach(var line in mod.ModLines) {
                    if (LineToMapModsDict.ContainsKey(line.LineText)) {
                        LineToMapModsDict[line.LineText].Add(mod);
                    }
                    else {
                        List<MapMod> lineList = new List<MapMod>();
                        lineList.Add(mod);
                        LineToMapModsDict[line.LineText] = lineList;
                    }
                }
            }


            /*
            var fileLines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();


            MapMod mapMod = null;
            foreach(string line in fileLines) {
                if(line.Length > 0 && line.Substring(0, 1) == "#") {
                    continue;
                }
                else if(line == "") {
                    if(mapMod != null) {
                        PossibleMapMods.Add(mapMod);
                        mapMod = null;
                    }
                }
                else {
                    string[] splitLines = line.Split(';');

                    if(mapMod == null) {
                        mapMod = new MapMod();
                    }

                    if(splitLines.Length == 5) {
                        int iiq; int iir; int packsize;

                        if(!int.TryParse(splitLines[1].Trim('%'), out iiq)) {
                            iiq = 0;
                        }
                        if (!int.TryParse(splitLines[2].Trim('%'), out packsize)) {
                            packsize = 0;
                        }
                        if (!int.TryParse(splitLines[3].Trim('%'), out iir)) {
                            iir = 0;
                        }


                        mapMod.IIQ = iiq;
                        mapMod.PackSize = packsize;
                        mapMod.IIR = iir;
                        //mapMod.ModSource = splitLines[4];
                        //mapMod.ModLines.Add(splitLines[0]);
                    }
                    else {
                        //mapMod.ModLines.Add(line);
                    }

                }


            }

            if(mapMod != null) {
                if (!PossibleMapMods.Contains(mapMod)) {
                    PossibleMapMods.Add(mapMod);
                }
            }
            */
        }

        public Dictionary<string, string> CurrencyTagLookup = new Dictionary<string, string>();
        private void LoadCurrencyTagLookup() {
            var assembly = Assembly.GetEntryAssembly();

            var rsStream = assembly.GetManifestResourceStream("PathTracker-Backend.Resources.CurrencyTags.txt");

            string fileContent = null;
            using (var reader = new StreamReader(rsStream)) {
                fileContent = reader.ReadToEnd();
            }


            if (fileContent == null) {
                throw new Exception("Could not load CurrencyTags.txt resource");
            }

            var fileLines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            foreach(string line in fileLines) {
                string[] parts = line.Split(';');

                foreach(string s in parts[1].Split(',')) {
                    CurrencyTagLookup.Add(s.Trim(' '), parts[0]);
                }
            }
        }

        public HashSet<string> ExcludedCurrencies = new HashSet<string>();
        private void LoadExcludedCurrencies() {
            var assembly = Assembly.GetEntryAssembly();

            var rsStream = assembly.GetManifestResourceStream("PathTracker-Backend.Resources.CurrencyTags.txt");

            string fileContent = null;
            using (var reader = new StreamReader(rsStream)) {
                fileContent = reader.ReadToEnd();
            }


            if (fileContent == null) {
                throw new Exception("Could not load CurrencyTags.txt resource");
            }

            var fileLines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            foreach(string line in fileLines) {
                ExcludedCurrencies.Add(line);
            }
        }

        public ResourceManager() {
            CalculateExperienceDictionary();
            LoadMapMods();
            LoadCurrencyTagLookup();
            LoadExcludedCurrencies();
        }

        private static WebResourceManager Manager = new WebResourceManager();

        public static WebResourceManager Instance {
            get {
                return Manager;
            }
        }
    }
}
