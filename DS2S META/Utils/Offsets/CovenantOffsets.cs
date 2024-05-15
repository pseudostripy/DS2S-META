﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS2S_META.Utils;

namespace DS2S_META.Utils.Offsets
{
    public class CovenantOffsets
    {
        public Dictionary<COV,CovOff> Offsets { get; set; } = new();

        public void AddOffsets(COV id, int discov, int rank, int progress)
        {
            Offsets.Add(id, new CovOff(discov, rank, progress));
        }

        public class CovOff
        {
            public int Discov;
            public int Rank;
            public int Progress;

            public CovOff(int discov, int rank, int progress) 
            {
                Discov = discov;
                Rank = rank;
                Progress = progress;
            }
        }
    }
}