using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    /* A BeatEvent is a group of one or more notes that may be said to have
     * occurred at the same abstract musical time, i.e. on a score they'd all
     * be aligned perfectly. These are our base object to be processed by the algorithm. */
    public struct BeatEvent
    {
        /* We keep track of the average time for this BeatEvent as well as the notes
         * that make it up. */
        public double MeanTimestamp;
        public List<NoteEvent> Notes;

        public BeatEvent(NoteEvent note)
        {
            MeanTimestamp = note.Timestamp.TotalMilliseconds;
            Notes = new List<NoteEvent>();
            Notes.Add(note);
        }
        
        /* We add a new note and update the central time for the event at the same time. */

        public void AddNote(NoteEvent note)
        {
            Notes.Add(note);
            MeanTimestamp += (note.Timestamp.TotalMilliseconds - MeanTimestamp) / Notes.Count;
        }
    }

    /* EventInterval is a unit of time between two BeatEvents. Since it may be similar to
     * other intervals between notes, we will group these together into clusters. */

    public struct EventInterval
    {
        /* Remember both of the events at each end of the interval, and its timespan. */
        public BeatEvent Event1;
        public BeatEvent Event2;
        public double Length;

        public EventInterval(BeatEvent event1, BeatEvent event2)
        {
            Event1 = event1;
            Event2 = event2;
            Length = Math.Abs(event1.MeanTimestamp - event2.MeanTimestamp);
        }
    }

    /* An IntervalCluster is a group of BeatIntervals that are so similar that they can be
     * considered as one single hypothesis for the tempo of the rhythm. */
    public class IntervalCluster
    {
        public List<EventInterval> Intervals;
        public double MeanLength;
        public int Rating;

        public IntervalCluster(EventInterval interval)
        {
            MeanLength = interval.Length;
            Intervals = new List<EventInterval>();
            Intervals.Add(interval);
            Rating = 0;
        }

        /* When we add a new interval to the cluster, we update the mean length at the same time. */
        public void AddInterval(EventInterval interval)
        {
            Intervals.Add(interval);
            MeanLength += (interval.Length - MeanLength) / Intervals.Count;
        }

        /* If we want to merge two clusters together, we add all the intervals from one into the other. */
        public void MergeCluster(IntervalCluster cluster)
        {
            foreach(EventInterval interval in cluster.Intervals)
            {
                AddInterval(interval);
            }
        }
    }

    /* The TempoInferrer class is the main class for the algorithm. */
    public static class TempoInferrer
    {
        /* This bool defines whether we'll print our list of events and/or clusters after finding them. */
        static bool debugPrintEvents = false;
        static bool debugPrintClusters = true;

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
                    if (!eventFound && Math.Abs(timestamp - _event.MeanTimestamp) < eventWidth)
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
                    Console.WriteLine("Event at " + _event.MeanTimestamp + " with " + _event.Notes.Count + " notes.");
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

            return clusters;
        }
    }
}