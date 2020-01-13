using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    public static class HierarchicalRhythmInferrer
    {
        /* This function strips down a rhythm into its smallest repeating unit as a power of two semiquavers. */
        public static RhythmStructure FindRepeatingUnit(RhythmStructure rhythm)
        {
            int rhythmLength = rhythm.drums.Count;
            int maxRepeatingUnitLength = Convert.ToInt32(Math.Pow(2, Math.Floor(Math.Log(rhythmLength, 2))));
            bool repeatingUnitFound = false;
            int unitLength = 1;
            RhythmStructure rhythmUnit = new RhythmStructure(rhythm.beatInterval);

            /* Basically, start with a unit of length one and check it against the rest of the rhythm,
             * if it doesn't match keep doubling the length until it matches, or take the original rhythm. */
            while (!repeatingUnitFound && unitLength <= maxRepeatingUnitLength)
            {
                rhythmUnit = rhythm.CopySub(0, unitLength, rhythm.beatInterval);
                int repetitions = (rhythmLength / unitLength) - 1;
                repeatingUnitFound = true;

                for (int i = 0; i < repetitions; i++)
                {
                    if (!rhythm.CheckMatch(rhythmUnit, (i + 1) * unitLength))
                    {
                        repeatingUnitFound = false;
                    }
                }

                unitLength *= 2;
            }

            return rhythmUnit;
        }

        public static HierarchicalRhythm CreateHierarchicalRhythm(RhythmStructure rhythm)
        {
            // TODO: Implement
            return null;
        }
    }
}
