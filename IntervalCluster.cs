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
        public double Time;
        public List<NoteEvent> Notes;

        public BeatEvent(NoteEvent note)
        {
            Time = note.Timestamp.TotalMilliseconds;
            Notes = new List<NoteEvent>();
            Notes.Add(note);
        }

        /* We add a new note and update the central time for the event at the same time. */

        public void AddNote(NoteEvent note)
        {
            Notes.Add(note);
            Time += (note.Timestamp.TotalMilliseconds - Time) / Notes.Count;
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
            Length = Math.Abs(event1.Time - event2.Time);
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
            foreach (EventInterval interval in cluster.Intervals)
            {
                AddInterval(interval);
            }
        }

        /* Fetch hypothetical BPM for simplicity. */
        public double GetBPM()
        {
            return (1 / MeanLength) * 60 * 1000;
        }
    }

}
