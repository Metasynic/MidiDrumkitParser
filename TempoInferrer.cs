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
        }
        
        /* We add a new note and update the central time for the event at the same time. */

        public void AddNote(NoteEvent note)
        {
            Notes.Add(note);
            MeanTimestamp += (note.Timestamp.TotalMilliseconds - MeanTimestamp) / Notes.Count;
        }
    }

    /* The TempoInferrer class is the main class for the algorithm. */
    public static class TempoInferrer
    {
        /* This bool defines whether we'll print our list of events after finding them. */
        static bool debugPrintEvents = true;

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
                    if (!eventFound && Math.Abs(timestamp - _event.MeanTimestamp) < 70)
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
    }
}
