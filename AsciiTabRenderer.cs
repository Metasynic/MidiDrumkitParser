using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MIDI_Drumkit_Parser
{
    /* A LabelSymbolPair translates the label for a drum to the symbol for the ASCII tab. */
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
        /* This lookup table translates from Drums to LabelSymbolPairs. */
        static Dictionary<Drum, LabelSymbolPair> drumToName = new Dictionary<Drum, LabelSymbolPair>
        {
            [Drum.Snare] = new LabelSymbolPair(" S", "o"),
            [Drum.TomHigh] = new LabelSymbolPair("T1", "o"),
            [Drum.TomMid] = new LabelSymbolPair("T2", "o"),
            [Drum.TomLow] = new LabelSymbolPair("FT", "o"),
            [Drum.HatOpen] = new LabelSymbolPair("HH", "o"),
            [Drum.CrashLeft] = new LabelSymbolPair("LC", "x"),
            [Drum.CrashRight] = new LabelSymbolPair("RC", "x"),
            [Drum.Kick] = new LabelSymbolPair("BD", "o"),
            [Drum.HatClosing] = new LabelSymbolPair("HH", "x"),
            [Drum.HatClosed] = new LabelSymbolPair("HH", "x")
        };

        /* We render the ASCII tab based on the original rhythm. */
        public static void RenderAsciiTab(RhythmStructure rhythm)
        {
            Dictionary<string, string> tab = new Dictionary<string, string>();
            foreach (LabelSymbolPair pair in drumToName.Values)
            {
                tab[pair.label] = pair.label + "|";
            }

            /* Initialize each slot with the "-" symbol then replace it if we find any of the right drum during this interval. */
            for (int i = 0; i < rhythm.drums.Count; i++) 
            {
                HashSet<Drum> drums = rhythm.drums[i];
                foreach (string index in tab.Keys.ToList())
                {
                    tab[index] += "-";
                }

                foreach (Drum drum in drums)
                {
                    if (drumToName.Keys.Contains(drum))
                    {
                        LabelSymbolPair pair = drumToName[drum];
                        tab[pair.label] = tab[pair.label].Remove(i + 3, 1).Insert(i + 3, pair.symbol);
                    }
                }
            }

            /* Finally write out the tab to a text file. */
            using (StreamWriter writer = new StreamWriter("tab.txt"))
            {
                writer.WriteLine(Convert.ToInt32(rhythm.beatInterval));
                foreach (string str in tab.Values)
                {
                    writer.WriteLine(str);
                }
            }
        }
    }
}
