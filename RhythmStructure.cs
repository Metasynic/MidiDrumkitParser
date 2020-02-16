using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    /* Types of Drum. */
    public enum Drum
    {
        Snare, TomHigh, TomMid, TomLow, HatOpen, HatClosed, HatClosing, CrashLeft, CrashRight, Kick
    }

    /* A class for a rhythm, making up a list of the drums used in each semiquaver,
     * and the interval between beats in milliseconds. */
    public class RhythmStructure
    {
        public double beatInterval;
        public List<HashSet<Drum>> drums;

        public RhythmStructure(double _beatInterval)
        {
            beatInterval = _beatInterval;
            drums = new List<HashSet<Drum>>();
        }

        // NOTE: This copy may need to go deeper as it copies hash set references.
        public RhythmStructure(RhythmStructure rhythm)
        {
            beatInterval = rhythm.beatInterval;
            drums = new List<HashSet<Drum>>(rhythm.drums);
        }

        /* Functions to get the set of Drums played on a certain semiquaver,
         * add drums to a semiquaver, and copy a subsection of the rhythm. */
        public HashSet<Drum> GetAtIndex(int beatIndex, int semiQIndex)
        {
            return drums[beatIndex * 4 + semiQIndex];
        }

        public HashSet<Drum> GetAtIndex(int index)
        {
            return drums[index];
        }

        public void AddDrums(HashSet<Drum> drumsIn)
        {
            drums.Add(drumsIn);
        }

        public void RemoveDrumAt (int index, Drum drum)
        {
            if (drums[index].Contains(drum))
            {
                drums[index].Remove(drum);
            }
        }

        public RhythmStructure CopySub(int startIndex, int length, double interval)
        {
            RhythmStructure copy = new RhythmStructure(interval);

            for (int i = startIndex; i < startIndex + length; i++)
            {
                copy.AddDrums(GetAtIndex(i));
            }

            return copy;
        }

        // Check all of the input rhythm against the subsection of this rhythm given by the parameters.
        public bool CheckMatch(RhythmStructure otherRhythm, int startIndex)
        {
            bool match = true;

            for (int i = startIndex; i < startIndex + otherRhythm.drums.Count; i++)
            {
                int otherRhythmIndex = i - startIndex;
                if (!drums[i].SetEquals(otherRhythm.drums[otherRhythmIndex]))
                {
                    match = false;
                }
            }

            return match;
        }
    }
}