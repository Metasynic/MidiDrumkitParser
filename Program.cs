/* This project uses the .NET MIDI library, under the MIT license.
 * As per the MIT license, the license text is included below.
 * 
 * Copyright (c) 2006 Leslie Sanford
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software. 
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            MidiReader reader = new MidiReader();
            Console.WriteLine("Ready to record. Press any key to begin.");
            Console.ReadKey();

            reader.Start();
            Console.WriteLine("Recording started. Press any key to stop the recording.");
            Console.ReadKey();

            reader.Stop();
            Console.WriteLine("Recording finished. Processing...");
            List<BeatEvent> beatEvents = TempoInferrer.NotesToEvents(reader.FetchEventList());
            List<IntervalCluster> intervalClusters = TempoInferrer.EventsToClusters(beatEvents);
            intervalClusters = TempoInferrer.RateClusters(intervalClusters);
            BeatTracker finalBeat = BeatInferrer.FindBeat(intervalClusters, beatEvents);
            RhythmStructure rhythm = RhythmCreator.CreateRhythm(finalBeat, beatEvents);

            AsciiTabRenderer.RenderAsciiTab(rhythm);
            SonicPiEmitter.EmitSonicPi();

            Console.ReadKey();
        }
    }
}
