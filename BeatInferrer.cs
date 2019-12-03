using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    public class BeatTracker
    {
        public double Interval;
        public double NextPrediction;
        public List<BeatEvent> ProcessedItems;
        public double Rating;
        
        public BeatTracker(double interval, double nextPrediction, BeatEvent firstEvent)
        {
            Interval = interval;
            NextPrediction = nextPrediction;
            ProcessedItems = new List<BeatEvent>();
            ProcessedItems.Add(firstEvent);
            /* TODO: For now we use the number of notes in the event as the rating.
             * Later this will be weighted by dynamic velocity and instrument. */
            Rating = firstEvent.Notes.Count;
        }
    }

    public static class BeatInferrer
    {
        public static BeatTracker FindBeat(List<IntervalCluster> tempoHypotheses, List<BeatEvent> events)
        {
            return null;
        }
    }
}
