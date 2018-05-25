using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Linq;

namespace PathTracker_Backend
{
    public class ResourceManager {
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

        public List<MapMod> PossibleMapMods = new List<MapMod>();
        private void LoadMapMods() {
            var assembly = Assembly.GetEntryAssembly();

            var rsStream = assembly.GetManifestResourceStream("PathTracker-Backend.Resources.MapMods.txt");

            string fileContent = null;
            using (var reader = new StreamReader(rsStream)) {
                fileContent = reader.ReadToEnd();
            }

            if (fileContent == null) {
                throw new Exception("Could not load ExperienceToLevel.txt resource");
            }

            var fileLines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            int k = 0;
        }

        private ResourceManager() {
            CalculateExperienceDictionary();
            LoadMapMods();
        }

        private static ResourceManager Manager = new ResourceManager();

        public static ResourceManager Instance {
            get {
                return Manager;
            }
        }
    }
}
