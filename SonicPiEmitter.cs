using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    public static class SonicPiEmitter
    {
        static Dictionary<string, string> sampleNames = new Dictionary<string, string>
        {
            [" S"] = ":drum_snare_hard",
            ["T1"] = ":drum_tom_hi_hard",
            ["T2"] = ":drum_tom_mid_hard",
            ["FT"] = ":drum_tom_lo_hard",
            ["HHc"] = ":drum_cymbal_closed",
            ["HHo"] = ":drum_cymbal_open",
            ["LC"] = ":drum_cymbal_hard",
            ["RC"] = ":drum_cymbal_hard",
            ["BD"] = ":drum_bass_hard"
        };

        public static void EmitSonicPi()
        {
            using (StreamReader reader = new StreamReader("tab.txt"))
            using (StreamWriter writer = new StreamWriter("sonicpi.txt"))
            {
                Dictionary<string, string> lines = new Dictionary<string, string>();
                double interval = Convert.ToInt32(reader.ReadLine());
                double sq_length = interval / 4;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    lines.Add(line.Substring(0, 2), line.Substring(3, line.Length - 3));
                }

                writer.WriteLine("loop do");
                for(int i = 0; i < lines[" S"].Length; i++)
                {
                    foreach(string drum in lines.Keys)
                    {
                        if (lines[drum][i] != '-')
                        {
                            if (drum.Equals("HH"))
                            {
                                if (lines[drum][i] == 'x')
                                {
                                    writer.WriteLine("sample " + sampleNames["HHc"]);
                                }
                                else // 'o'
                                {
                                    writer.WriteLine("sample" + sampleNames["HHo"]);
                                }
                            }
                            else
                            {
                                writer.WriteLine("sample " + sampleNames[drum]);
                            }
                        }
                    }

                    writer.WriteLine("sleep " + (sq_length / 1000));
                }
                writer.WriteLine("end");
            }
        }
    }
}
