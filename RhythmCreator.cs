using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    public static class RhythmCreator
    {
        /* A lookup table of MIDI indices to Drums. */
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

        /* Create a rhythm from a tracker and list of events, by quantizing the events
         * according to the beat tracker given to the function. */
        public static RhythmStructure CreateRhythm(BeatTracker tracker, List<BeatEvent> events)
        {
            const byte numDivisions = 4;
            int eventIndex = 0;
            RhythmStructure rhythm = new RhythmStructure(tracker.Interval);

            /* Iterate over all the beats and semiquavers, and set the semiquaver interval. */
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

                    /* Determine the time at which one semiquaver event occurs and then quantizes each note event to the semiquaver. */
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
