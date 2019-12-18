using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MIDI_Drumkit_Parser
{
    public struct LabelSymbolPair
    {
        public string label;
        public string symbol;

        public LabelSymbolPair(string _label, string _symbol)
        {
            label = _label;
            symbol = _symbol;
        }
    }

    public static class AsciiTabRenderer
    {
        static Dictionary<byte, LabelSymbolPair> indexToName = new Dictionary<byte, LabelSymbolPair>
        {
            [38] = new LabelSymbolPair(" S", "o"),
            [48] = new LabelSymbolPair("T1", "o"),
            [45] = new LabelSymbolPair("T2", "o"),
            [43] = new LabelSymbolPair("FT", "o"),
            [46] = new LabelSymbolPair("HH", "o"),
            [49] = new LabelSymbolPair("LC", "x"),
            [51] = new LabelSymbolPair("RC", "x"),
            [36] = new LabelSymbolPair("BD", "o"),
            [44] = new LabelSymbolPair("HH", "x"),
            [42] = new LabelSymbolPair("HH", "x")
        };

        public static void RenderAsciiTab(BeatTracker tracker, List<BeatEvent> events)
        {
            const byte numDivisions = 4;
            int eventIndex = 0;
            Dictionary<string, string> tab = new Dictionary<string, string>();
            foreach (LabelSymbolPair pair in indexToName.Values)
            {
                tab[pair.label] = pair.label + "|";
            }

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
                    foreach(string index in tab.Keys.ToList())
                    {
                        tab[index] += "-";
                    }
                    double baseTime = baseEvent.Time + (j * interval);
                    if (eventIndex < events.Count && baseTime - (interval / 2) < events[eventIndex].Time && baseTime + (interval / 2) > events[eventIndex].Time)
                    {
                        foreach (NoteEvent noteEvent in events[eventIndex].Notes)
                        {
                            if (indexToName.Keys.Contains(noteEvent.Channel))
                            {
                                LabelSymbolPair pair = indexToName[noteEvent.Channel];
                                tab[pair.label] = tab[pair.label].Remove((i * numDivisions) + j + 3, 1).Insert((i * numDivisions) + j + 3, pair.symbol);
                            }
                        }
                        eventIndex++;
                    }
                }
            }

            using (StreamWriter writer = new StreamWriter("tab.txt"))
            {
                foreach(string str in tab.Values)
                {
                    writer.WriteLine(str);
                }
            }
        }
    }
}
