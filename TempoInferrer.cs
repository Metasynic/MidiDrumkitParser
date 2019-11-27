using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    public struct BeatEvent
    {
        public double MeanTimestamp;
        public List<NoteEvent> Notes;

        public BeatEvent(NoteEvent note)
        {
            MeanTimestamp = note.Timestamp.TotalMilliseconds;
            Notes = new List<NoteEvent>();
        }

        public void AddNote(NoteEvent note)
        {
            Notes.Add(note);
            MeanTimestamp += (note.Timestamp.TotalMilliseconds - MeanTimestamp) / Notes.Count;
        }
    }

    public static class TempoInferrer
    {
        static bool debugPrintEvents = true;
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
