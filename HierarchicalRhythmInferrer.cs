using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    public class HierarchicalRhythm
    {

    }

    public static class HierarchicalRhythmInferrer
    {
        public static RhythmStructure FindRepeatingUnit(RhythmStructure rhythm)
        {
            int rhythmLength = rhythm.drums.Count;
            int maxRepeatingUnitLength = Convert.ToInt32(Math.Pow(2, Math.Floor(Math.Log(rhythmLength, 2))));
            bool repeatingUnitFound = false;
            int unitLength = 1;
            RhythmStructure rhythmUnit = new RhythmStructure(rhythm.beatInterval);

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
