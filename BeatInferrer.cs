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
        
        public BeatTracker(double interval, BeatEvent firstEvent)
        {
            Interval = interval;
            NextPrediction = firstEvent.Time + interval;
            ProcessedItems = new List<BeatEvent>();
            ProcessedItems.Add(firstEvent);
            /* TODO: For now we use the number of notes in the event as the rating.
             * Later this will be weighted by dynamic velocity and instrument. */
            Rating = firstEvent.Notes.Count;
        }

        public BeatTracker(BeatTracker tracker)
        {
            Interval = tracker.Interval;
            NextPrediction = tracker.NextPrediction;
            ProcessedItems = new List<BeatEvent>(tracker.ProcessedItems);
            Rating = tracker.Rating;
        }

        public void TakeBestTracker(BeatTracker tracker)
        {
            if (tracker.Rating > Rating)
            {
                Interval = tracker.Interval;
                NextPrediction = tracker.NextPrediction;
                ProcessedItems = new List<BeatEvent>(tracker.ProcessedItems);
                Rating = tracker.Rating;
            }
        }

        public void PrintTracker()
        {
            Console.WriteLine("Interval: " + Interval + "ms, or " + GetBPM() + " BPM");
            Console.WriteLine("Beat Events processed: " + ProcessedItems.Count);
            Console.WriteLine("Rating: " + Rating);
        }

        public double GetBPM()
        {
            return (1 / Interval) * 60 * 1000;
        }
    }

    public static class BeatInferrer
    {
        static bool SimilarTrackers(BeatTracker first, BeatTracker second)
        {
            return (Math.Abs(first.Interval - second.Interval) < 10) && (Math.Abs(first.NextPrediction - second.NextPrediction) < 20);
        }

        // TODO: The outerWindow may be better as a coefficient of the current hypothetical beat interval.
        const double innerWindow = 40;
        const double outerWindowFactor = 0.3;
        const double initialPeriod = 5000;
        const double maximumInterval = 2000;
        const double correctionFactor = 0.3;

        public static BeatTracker FindBeat(List<IntervalCluster> tempoHypotheses, List<BeatEvent> events)
        {
            List<BeatTracker> trackers = new List<BeatTracker>();
            foreach(IntervalCluster cluster in tempoHypotheses)
            {
                foreach(BeatEvent startEvent in events.Where(e => e.Time < initialPeriod).ToList())
                {
                    trackers.Add(new BeatTracker(cluster.MeanLength, startEvent));
                }
            }

            foreach(BeatEvent _event in events)
            {
                List<BeatTracker> newTrackers = new List<BeatTracker>();

                for(int i = trackers.Count - 1; i >= 0; i--)
                {
                    BeatTracker tracker = trackers[i];
                    if (_event.Time - tracker.ProcessedItems[tracker.ProcessedItems.Count - 1].Time > maximumInterval)
                    {
                        trackers.RemoveAt(i);
                    }
                    else
                    {
                        while (tracker.NextPrediction + (outerWindowFactor * tracker.Interval) < _event.Time)
                        {
                            tracker.NextPrediction += tracker.Interval;
                        }
                        if (_event.Time > tracker.NextPrediction - (outerWindowFactor * tracker.Interval) 
                            && (_event.Time < tracker.NextPrediction + (outerWindowFactor * tracker.Interval)))
                        {
                            if (Math.Abs(_event.Time - tracker.NextPrediction) > innerWindow)
                            {
                                newTrackers.Add(new BeatTracker(tracker));
                            }

                            double error = _event.Time - tracker.NextPrediction;
                            tracker.Interval += error / correctionFactor;
                            tracker.NextPrediction = _event.Time + tracker.Interval;
                            tracker.ProcessedItems.Add(_event);

                            //TODO: Switch Count out for salience
                            tracker.Rating += (1 - (Math.Abs(error) / (2 * tracker.NextPrediction))) * _event.Notes.Count;
                        }
                    }
                }

                // Add new trackers to the list
                foreach(BeatTracker tracker in newTrackers)
                {
                    trackers.Add(tracker);
                }

                // Remove duplicate trackers
                List<BeatTracker> nextTrackers = new List<BeatTracker>();
                foreach (BeatTracker tracker in trackers)
                {
                    bool matchFound = false;
                    foreach (BeatTracker nextTracker in nextTrackers)
                    {
                        if (!matchFound && SimilarTrackers(tracker, nextTracker))
                        {
                            nextTracker.TakeBestTracker(tracker);
                            matchFound = true;
                        }
                    }

                    if (!matchFound)
                    {
                        nextTrackers.Add(tracker);
                    }
                }

                trackers = nextTrackers;
            }

            return trackers.OrderByDescending(t => t.Rating).ToList()[0];
        }
    }
}