using System;
using System.Collections.Generic;
using System.Text;

namespace PathTracker_Backend
{
    public class ExperienceDeltaCalculator
    {
        public Character EnteredWithCharacter;
        public Character ExitedWithCharacter;
        
        public CharacterProgress CalculateDelta() {

            CharacterProgress characterProgress = new CharacterProgress();
            characterProgress.AscendencyClass = ExitedWithCharacter.AscendencyClass;
            characterProgress.League = ExitedWithCharacter.League;
            characterProgress.Name = characterProgress.Name;

            //Simple delta calculation for penalized xp
            characterProgress.ExperienceProgress = ExitedWithCharacter.Experience - EnteredWithCharacter.Experience; 



            return characterProgress;
        }
    }
}
