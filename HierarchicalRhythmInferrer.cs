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

        static List<Drum> drumList = new List<Drum> { Drum.Snare, Drum.TomHigh, Drum.TomMid, Drum.TomLow, 
            Drum.HatOpen, Drum.HatClosed, Drum.HatClosing, Drum.CrashLeft, Drum.CrashRight, Drum.Kick };

        public static HierarchicalRhythm CreateHierarchicalRhythm(RhythmStructure rhythm)
        {
            RhythmStructure rhythmCopy = new RhythmStructure(rhythm);
            int treeDepth = (int)Math.Log(rhythm.drums.Count, 2);
            HierarchicalRhythm hRhythm = new HierarchicalRhythm(rhythm.beatInterval, treeDepth);

            foreach (Drum drum in drumList)
            {
                /* Tree level 0 is the root, for drums that occur on every semiquaver of the rhythm,
                 * and the bottom level is the notes that occur only once across the rhythm. */
                for (int level = 0; level <= treeDepth; level++)
                {
                    int intervalBetweenNotes = (int)Math.Pow(2, level);

                    /* This index is the amount that the recurring note is "shifted" across the rhythm.
                     * For example, if we're at minim level, then at i=3 we have notes occuring every minim,
                     * starting on the semiquaver with index 3. */
                    for (int shift = 0; shift < intervalBetweenNotes; shift++)
                    {
                        /* If the particular drum we're looking at is found in the rhythm,
                         * at the location we're dealing with, start scanning along. */
                        if (rhythmCopy.GetAtIndex(shift).Contains(drum))
                        {
                            /* Finally, iterate across the entire rhythm according to the shift and interval. 
                             * TODO: This isn't very efficient, would be better to stop immediately if there's a mismatch. */
                            bool notesLineUp = true;

                            for (int i = shift; i < rhythmCopy.drums.Count; i += intervalBetweenNotes)
                            {
                                if (!rhythmCopy.GetAtIndex(i).Contains(drum))
                                {
                                    notesLineUp = false;
                                }
                            }

                            if (notesLineUp)
                            {
                                hRhythm.AddDrum(level, shift, drum);

                                for (int i = shift; i < rhythmCopy.drums.Count; i += intervalBetweenNotes)
                                {
                                    rhythmCopy.RemoveDrumAt(i, drum);
                                }
                            }
                        }
                    }
                }
            }

            return hRhythm;
        }
    }
}
