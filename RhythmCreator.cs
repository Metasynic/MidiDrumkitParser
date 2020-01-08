using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    public enum Drum
    {
        Snare, TomHigh, TomMid, TomLow, HatOpen, HatClosed, HatClosing, CrashLeft, CrashRight, Kick
    }

    public class RhythmStructure
    {
        public double beatInterval;
        public List<HashSet<Drum>> drums;
        internal int unitLength;

        public RhythmStructure(double _beatInterval)
        {
            beatInterval = _beatInterval;
            drums = new List<HashSet<Drum>>();
        }

        public HashSet<Drum> GetAtIndex(int beatIndex, int semiQIndex)
        {
            return drums[beatIndex * 4 + semiQIndex];
        }

        public HashSet<Drum> GetAtIndex(int index) {
            return drums[index];
        }

        public void AddDrums(HashSet<Drum> drumsIn)
        {
            drums.Add(drumsIn);
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

    public static class RhythmCreator
    {
        static Dictionary<byte, Drum> indexToDrum = new Dictionary<byte, Drum>
        {
            [38] = Drum.Snare,
            [48] = Drum.TomHigh,
            [45] = Drum.TomMid,
            [43] = Drum.TomLow,
            [46] = Drum.HatOpen,
            [44] = Drum.HatClosing,
            [42] = Drum.HatClosed,
            [49] = Drum.CrashLeft,
            [51] = Drum.CrashRight,
            [36] = Drum.Kick
        };

        public static RhythmStructure CreateRhythm(BeatTracker tracker, List<BeatEvent> events)
        {
            const byte numDivisions = 4;
            int eventIndex = 0;
            RhythmStructure rhythm = new RhythmStructure(tracker.Interval);

            for (int i = 0; i < tracker.ProcessedItems.Count; i++)
            {
                BeatEvent baseEvent = tracker.ProcessedItems[i];
                double interval;
                if (i != tracker.ProcessedItems.Count - 1)
                {
                    BeatEvent nextEvent = tracker.ProcessedItems[i + 1];
                    interval = (nextEvent.Time - baseEvent.Time) / 4;
                }
                else
                {
                    interval = (tracker.NextPrediction - baseEvent.Time) / 4;
                }

                for (int j = 0; j < numDivisions; j++)
                {
                    HashSet<Drum> drums = new HashSet<Drum>();

                    double baseTime = baseEvent.Time + (j * interval);
                    if (eventIndex < events.Count && baseTime - (interval / 2) < events[eventIndex].Time && baseTime + (interval / 2) > events[eventIndex].Time)
                    {
                        foreach (NoteEvent noteEvent in events[eventIndex].Notes)
                        {
                            if (indexToDrum.Keys.Contains(noteEvent.Channel))
                            {
                                Drum drum = indexToDrum[noteEvent.Channel];
                                drums.Add(drum);
                            }
                        }
                        eventIndex++;
                    }

                    rhythm.AddDrums(drums);
                }
            }

            return rhythm;
        }
    }
}
