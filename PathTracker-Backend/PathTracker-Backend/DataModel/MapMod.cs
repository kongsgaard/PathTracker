﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracker_Backend
{
    public class MapMod
    {
        public List<string> ModLines;
        public int IIQ;
        public int IIR;
        public int PackSize;
        public string ModSource;

        public MapMod(List<string> lines, int iiq, int iir, int packsize, string modsource) {
            ModLines = lines;
            IIQ = iiq;
            IIR = iir;
            PackSize = packsize;
            ModSource = modsource;
        }

        public MapMod() {
            ModLines = new List<string>();
            IIQ = 0;
            IIR = 0;
            PackSize = 0;
            ModSource = "None";
        }

        public MapMod(MapMod mapMod) {
            ModLines = new List<string>(mapMod.ModLines);
            IIQ = mapMod.IIQ;
            IIQ = mapMod.IIR;
            PackSize = mapMod.PackSize;
            ModSource = mapMod.ModSource;
        }
    }
}