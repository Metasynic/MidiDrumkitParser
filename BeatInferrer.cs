using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    /* The BeatTracker class is a continuous record-keeper that travels along a rhythm,
     * tracking whether the notes in the rhythm line up its given tempo hypothesis. */
    public class BeatTracker
    {
        public double Interval;
        public double NextPrediction;
        public List<BeatEvent> ProcessedItems;
        public double Rating;
        public double OriginalScore;
        const bool debugPrintTrackers = true;
        
        /* There are two constructors, one of which is a copy constructor. */
        public BeatTracker(double interval, BeatEvent firstEvent, double originalScore)
        {
            Interval = interval;
            NextPrediction = firstEvent.Time + interval;
            ProcessedItems = new List<BeatEvent>();
            ProcessedItems.Add(firstEvent);
            /* NOTE: For now we use sum of velocities as the rating.
             * This could also use drum-specific information. */
            Rating = firstEvent.Notes.Sum(n => n.Velocity);
            OriginalScore = originalScore;
        }
        
        public BeatTracker(BeatTracker tracker)
        {
            Interval = tracker.Interval;
            NextPrediction = tracker.NextPrediction;
            ProcessedItems = new List<BeatEvent>(tracker.ProcessedItems);
            Rating = tracker.Rating;
            OriginalScore = tracker.OriginalScore;
        }

        /* Effectively merges two trackers together based on which has the best rating. */
        public void TakeBestTracker(BeatTracker tracker)
        {
            if (tracker.Rating > Rating)
            {
                Interval = tracker.Interval;
                NextPrediction = tracker.NextPrediction;
                ProcessedItems = new List<BeatEvent>(tracker.ProcessedItems);
                Rating = tracker.Rating;
                OriginalScore = tracker.OriginalScore;
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

    /* BeatInferrer runs a set of BeatTrackers against the rhythm to find the best beat alignment. */
    public static class BeatInferrer
    {
        /* Fuzzy comparison of trackers, since they can be non-identical but functionally the same. */
        static bool SimilarTrackers(BeatTracker first, BeatTracker second)
        {
            return (Math.Abs(first.Interval - second.Interval) < 10) && (Math.Abs(first.NextPrediction - second.NextPrediction) < 20);
        }

        const double innerWindow = 40;
        const double outerWindowFactor = 0.3;
        const double initialPeriod = 5000;
        const double maximumInterval = 3000;
        const double correctionFactor = 0.2;

        public static BeatTracker FindBeat(List<IntervalCluster> tempoHypotheses, List<BeatEvent> events)
        {
            /* First, create all the trackers. */
            List<BeatTracker> trackers = new List<BeatTracker>();
            for(int i = 0; i < tempoHypotheses.Count; i++)
            {
                IntervalCluster cluster = tempoHypotheses[i];
                foreach(BeatEvent startEvent in events.Where(e => e.Time < initialPeriod).ToList())
                {
                    trackers.Add(new BeatTracker(cluster.MeanLength, startEvent, cluster.Rating));
                }
            }

            /* Iterate through every event in the rhythm, processing each tracker. */
            foreach(BeatEvent _event in events)
            {
                List<BeatTracker> newTrackers = new List<BeatTracker>();

                for(int i = trackers.Count - 1; i >= 0; i--)
                {
                    /* If any tracker has gone too long without detecting a beat candidate, drop it. */
                    BeatTracker tracker = trackers[i];
                    if (_event.Time - tracker.ProcessedItems[tracker.ProcessedItems.Count - 1].Time > maximumInterval)
                    {
                        trackers.RemoveAt(i);
                    }
                    else
                    {
                        /* Catch the trackers up with the current event. */
                        while (tracker.NextPrediction + (outerWindowFactor * tracker.Interval) < _event.Time)
                        {
                            tracker.NextPrediction += tracker.Interval;
                        }

                        /* Check whether the event is a feasible beat time. */
                        if (_event.Time > tracker.NextPrediction - (outerWindowFactor * tracker.Interval) 
                            && (_event.Time < tracker.NextPrediction + (outerWindowFactor * tracker.Interval)))
                        {
                            /* If it's too close to be sure, create another tracker. */
                            if (Math.Abs(_event.Time - tracker.NextPrediction) > innerWindow)
                            {
                                newTrackers.Add(new BeatTracker(tracker));
                            }

                            /* Update the tracker to prepare for the next loop iteration. */
                            double error = _event.Time - tracker.NextPrediction;
                            tracker.Interval += error * correctionFactor;
                            tracker.NextPrediction = _event.Time + tracker.Interval;
                            tracker.ProcessedItems.Add(_event);

                            // NOTE: It might be useful to have drum specific stuff as well as velocity
                            tracker.Rating += (1 - (Math.Abs(error) / (2 * tracker.NextPrediction))) * _event.Notes.Sum(n => n.Velocity);
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

            /* Weight each tracker's rating by the original score, so as to avoid very short beat selections. */
            foreach(BeatTracker tracker in trackers)
            {
                tracker.Rating *= tracker.OriginalScore;
            }

            trackers = trackers.OrderByDescending(t => t.Rating).ToList();

            // TODO: There are a lot of trackers that appear to be duplicates. Investigate.
            for (int i = 0; i < Math.Min(trackers.Count, 10); i++)
            {
                trackers[i].PrintTracker();
            }

            return trackers[0];
        }
    }
}