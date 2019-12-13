using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    /* The TempoInferrer class is the main class for the algorithm. */
    public static class TempoInferrer
    {
        /* This bool defines whether we'll print our list of events and/or clusters after finding them. */
        static bool debugPrintEvents = false;
        static bool debugPrintClusters = false;
        static bool debugPrintRatedClusters = true;

        const double clusterWidth = 70;
        const double eventWidth = 70;

        /* NotesToEvents takes a list of notes, and outputs a list of BeatEvents.
         * We group together the notes to within 70ms of each other.
         * Any note not falling in 70ms of an existing event is made into a new one. */
        public static List<BeatEvent> NotesToEvents(List<NoteEvent> inputNoteEvents)
        {
            List<BeatEvent> events = new List<BeatEvent>();

            foreach(NoteEvent note in inputNoteEvents)
            {
                double timestamp = note.Timestamp.TotalMilliseconds;
                bool eventFound = false;

                foreach(BeatEvent _event in events)
                {
                    if (!eventFound && Math.Abs(timestamp - _event.Time) < eventWidth)
                    {
                        eventFound = true;
                        _event.AddNote(note);
                    }
                }

                if (!eventFound)
                {
                    events.Add(new BeatEvent(note));
                }
            }

            /* Print the list of events if so desired. */
            if (debugPrintEvents)
            {
                foreach(BeatEvent _event in events)
                {
                    Console.WriteLine("Event at " + _event.Time + " with " + _event.Notes.Count + " notes.");
                }
            }

            return events;
        }

        /* EventsToClusters takes a list of beat events, finds all the intervals between them
         * that are less than 2 seconds long, and then clusters the intervals together into a list of
         * tempo hypotheses. */
        public static List<IntervalCluster> EventsToClusters(List<BeatEvent> beatEvents)
        {
            List<IntervalCluster> clusters = new List<IntervalCluster>();

            for (int i = 0; i < beatEvents.Count; i++)
            {
                for (int j = i + 1; j < beatEvents.Count; j++)
                {
                    EventInterval interval = new EventInterval(beatEvents[i], beatEvents[j]);
                    if (interval.Length < 2000)
                    {
                        bool clusterFound = false;
                        int clusterIndex = 0;
                        double clusterDistance = double.PositiveInfinity;

                        for (int c = 0; c < clusters.Count; c++)
                        {
                            IntervalCluster cluster = clusters[c];
                            double difference = Math.Abs(cluster.MeanLength - interval.Length);
                            if (difference < clusterWidth && difference < clusterDistance)
                            {
                                clusterFound = true;
                                clusterIndex = c;
                                clusterDistance = difference;
                            }
                        }

                        if (clusterFound)
                        {
                            clusters[clusterIndex].AddInterval(interval);
                        }
                        else
                        {
                            clusters.Add(new IntervalCluster(interval));
                        }
                    }
                }
            }

            /* Now cluster the clusters just in case any of the averages have strayed close together. */
            List<IntervalCluster> newClusters = new List<IntervalCluster>();
            foreach(IntervalCluster cluster in clusters)
            {
                bool matchFound = false;
                foreach(IntervalCluster newCluster in newClusters)
                {
                    if (!matchFound && Math.Abs(cluster.MeanLength - newCluster.MeanLength) < clusterWidth)
                    {
                        newCluster.MergeCluster(cluster);
                        matchFound = true;
                    }
                }

                if (!matchFound)
                {
                    newClusters.Add(cluster);
                }
            }

            /* If the cluster printing flag is set, then display a list of all the found clusters. */
            if (debugPrintClusters)
            {
                foreach(IntervalCluster cluster in newClusters)
                {
                    Console.WriteLine("Interval Cluster " + cluster.MeanLength + "ms, with " + cluster.Intervals.Count + " notes.");
                }
            }

            return newClusters;
        }

        /* When we rate one cluster with respect to another, we add the other cluster's size to it,
         * weighted by the following function. Small multiples are weighted more heavily than higher ones. */
        static int Weight(int i)
        {
            if (i >= 1 && i <= 4)
                return 6 - i;
            else if (i >= 5 && i <= 8)
                return 1;
            else
                return 0;
        }

        /* In order to obtain a ranking for our different hypotheses, we compare the clusters
         * against each other to see which of them are integer multiples within a reasonable margin.
         * Those with many multiples will have a higher score overall than ones without. */
        public static List<IntervalCluster> RateClusters (List<IntervalCluster> clusters)
        {
            foreach(IntervalCluster baseCluster in clusters)
            {
                foreach(IntervalCluster comparisonCluster in clusters)
                {
                    for(int i = 1; i < 9; i++)
                    {
                        if (Math.Abs(baseCluster.MeanLength - (i * comparisonCluster.MeanLength)) < clusterWidth)
                        {
                            baseCluster.Rating += Weight(i) * comparisonCluster.Intervals.Count;
                        }
                    }
                }
            }

            /* Order the clusters by their rating from highest to lowest and then print them if desired. */
            clusters = clusters.OrderByDescending(c => c.Rating).ToList();

            if (debugPrintRatedClusters)
            {
                foreach (IntervalCluster cluster in clusters)
                {
                    Console.WriteLine("Interval Cluster " + cluster.GetBPM() + " BPM, with " + cluster.Intervals.Count + " notes, score " + cluster.Rating + ".");
                }
            }

            return clusters;
        }
    }
}