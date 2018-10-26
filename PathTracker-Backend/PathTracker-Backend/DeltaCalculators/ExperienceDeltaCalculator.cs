using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Reflection;

namespace PathTracker_Backend
{
    public class ExperienceDeltaCalculator
    {
        public Character EnteredWithCharacter;
        public Character ExitedWithCharacter;

        public List<Item> EnteredWithItems;
        public List<Item> ExitedWithItems;

        public CharacterProgress CalculateDelta() {

            CharacterProgress characterProgress = new CharacterProgress();
            characterProgress.AscendencyClass = ExitedWithCharacter.AscendencyClass;
            characterProgress.League = ExitedWithCharacter.League;
            characterProgress.Name = ExitedWithCharacter.Name;

            //Simple delta calculation for penalized xp
            characterProgress.ExperienceProgress = ExitedWithCharacter.Experience - EnteredWithCharacter.Experience;

            characterProgress.TotalExperienceNonPenalized = CalculateTotalExperience();
            characterProgress.EquippedItems = ExitedWithItems;
            characterProgress.LevelProgress = CalculateLevelProgress();

            return characterProgress;
        }

        private Dictionary<int, double> CalculateLevelProgress() {

            Dictionary<int, double> LevelProgress = new Dictionary<int, double>();

            var ExperienceDictionary = ResourceManager.Instance.ExperienceDictionary;

            int EnteredLevel = EnteredWithCharacter.Level;
            long EnteredXp = EnteredWithCharacter.Experience;

            int ExitedLevel = ExitedWithCharacter.Level;
            long ExitedXp = ExitedWithCharacter.Experience;

            //Iterates all gained levels in the Delta
            for(int lvl = EnteredLevel; lvl <= ExitedLevel; lvl++) {
                var XpReqsForLevel = ExperienceDictionary[lvl];
                
                //Initial level percentage progress as: (EnteredXp - AccumelatedXpToThatLevel) / XpNeededToLevel
                double EnteredProgress = (EnteredXp - XpReqsForLevel.Item1) / XpReqsForLevel.Item2;

                //Exited level percentage progress as: (Max(ExitedXp,AccumelatedXpToThatLevel+XpNeededToLevel) - AccumelatedXpToThatLevel) / XpNeededToLevel
                double ExitedProgress = (Math.Max(ExitedXp, XpReqsForLevel.Item1+XpReqsForLevel.Item2) - XpReqsForLevel.Item1) / XpReqsForLevel.Item2;

                LevelProgress[lvl] = ExitedProgress - EnteredProgress;
            }

            return LevelProgress;
        }

        private int CalculateTotalExperience() {

            List<Item> EnteredWithSocketedGems = SocketedGems(EnteredWithItems);
            List<Item> ExitedWithSocketedGems = SocketedGems(ExitedWithItems);

            foreach(Item enteredWith in EnteredWithSocketedGems) {
                foreach(Item exitedWith in ExitedWithSocketedGems) {

                    //Don't use these due to the quality giving more xp.
                    var noGoGems = new string[] {"Enlighten Support", "Empower Support", "Enhance Support"};

                    //Match sockets based on id
                    if(enteredWith.Id == exitedWith.Id && enteredWith.AdditionalProperties.Count > 0 && ExitedWithSocketedGems.Count > 0 && 
                        !(noGoGems.Contains(enteredWith.TypeLine)) && !(noGoGems.Contains(exitedWith.TypeLine))) {

                        string[] enteredWithXp = null;
                        string[] exitedWithXp = null;

                        //Find xp property and value for enteredWith socket
                        foreach(Property p in enteredWith.AdditionalProperties) {
                            if(p.Name=="Experience" && p.Values.Count > 0) {
                                foreach(var values in p.Values) {
                                    foreach(var value in values) {
                                        if (value.Split('/').ToList().Count == 2) {
                                            enteredWithXp = value.Split('/');
                                        }
                                    }
                                }
                            }
                        }

                        //Find xp property and value for exitedWith socket
                        foreach (Property p in exitedWith.AdditionalProperties) {
                            if (p.Name == "Experience" && p.Values.Count > 0) {
                                foreach (var values in p.Values) {
                                    foreach (var value in values) {
                                        if (value.Split('/').ToList().Count == 2) {
                                            exitedWithXp = value.Split('/');
                                        }
                                    }
                                }
                            }
                        }

                        if(enteredWithXp != null && exitedWithXp != null) {
                            //Check same level, and that gem has gotten experience, and that exited with gem is not ready to level up
                            if (enteredWithXp[1] == exitedWithXp[1] && int.Parse(exitedWithXp[0]) > int.Parse(enteredWithXp[0]) && exitedWithXp[0] != exitedWithXp[1]) {
                                int gemXp = int.Parse(exitedWithXp[0]) - int.Parse(enteredWithXp[0]);

                                //Gems gain 1/10 of player xp
                                return gemXp * 10;
                            }
                        }
                        

                    }

                }

            }


            return 0;
        }

        private List<Item> SocketedGems(List<Item> inventory) {
            List<Item> SocketedGems = new List<Item>();

            var items = inventory.Where(x => x.InventoryId != "MainInventory");

            foreach (Item item in items) {
                if (item.SocketedItems != null) {
                    if (item.SocketedItems.Count > 0) {
                        foreach (Item socket in item.SocketedItems) {
                            if (socket.AdditionalProperties != null) {
                                if (socket.AdditionalProperties.Count > 0) {
                                    foreach (Property prop in socket.AdditionalProperties) {
                                        if (prop.Name == "Experience") {
                                            SocketedGems.Add(socket);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return SocketedGems;
        }
    }
}
