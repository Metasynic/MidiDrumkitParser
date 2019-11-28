using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sanford.Multimedia;
using Sanford.Multimedia.Midi;

namespace MIDI_Drumkit_Parser
{
    /* NoteEvent is a piece of data to represent one single note played in on the drumkit. */
    public struct NoteEvent
    {
        public byte Channel;
        public byte Velocity;
        public TimeSpan Timestamp;
        public NoteEvent(byte channel, byte velocity, TimeSpan timestamp)
        {
            Channel = channel;
            Velocity = velocity;
            Timestamp = timestamp;
        }
    }

    /* The main MidiReader class handles recording the MIDI note input from the drumkit. */
    public class MidiReader
    {
        /* The debugPrintMIDI shows raw events as they occur, 
         * debugPrintTimestamps shows the sequence of notes after recording. */
        bool debugPrintMIDI = false;
        bool debugPrintTimestamps = false;

        /* Variables to be used. Mostly MIDI stuff and the recording start time. */
        List<NoteEvent> noteEvents;
        const int sysExBufferSize = 128;
        InputDevice inputDevice;
        SynchronizationContext syncContext;
        DateTime baseTime;

        public MidiReader()
        {
            /* If the constructor can't find any MIDI devices, we wait 2 seconds then quit. */
            if (InputDevice.DeviceCount == 0)
            {
                Console.WriteLine("No MIDI devices found.");
                Thread.Sleep(2000);
                Environment.Exit(0);
                return;
            }

            /* Otherwise, we set our MIDI variables and hook up the appropriate event handlers. */
            else
            {
                try
                {
                    syncContext = SynchronizationContext.Current;
                    inputDevice = new InputDevice(0);
                    inputDevice.ChannelMessageReceived += onChannelMessageReceived;
                    inputDevice.SysCommonMessageReceived += onSysCommonMessageReceived;
                    inputDevice.SysExMessageReceived += onSysExMessageReceived;
                    inputDevice.SysRealtimeMessageReceived += onSysRealtimeMessageReceived;
                    inputDevice.Error += new EventHandler<ErrorEventArgs>(inputDevice_Error);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    return;
                }
            }

            /* Finally, initialize the list of events. */
            noteEvents = new List<NoteEvent>();
        }

        /* When we want to start reading, start recording from the input device. */
        public void Start()
        {
            try
            {
                inputDevice.StartRecording();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        /* Likewise, when we're finished reading, stop recording and then close the device.
         * If we want to print the captured note sequence, we do so now. */
        public void Stop()
        {
            try
            {
                inputDevice.StopRecording();
                inputDevice.Reset();
                inputDevice.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            if (debugPrintTimestamps)
            {
                foreach (NoteEvent ne in noteEvents)
                {
                    Console.WriteLine("Note " + ne.Channel + ", Vel " + ne.Velocity + ", Time " + ne.Timestamp.TotalMilliseconds);
                }
            }
        }

        /* Self-explanatory. */
        public List<NoteEvent> FetchEventList()
        {
            return noteEvents;
        }

        /* This is the only interesting event function. After printing the note for debugging purposes,
         * we take NoteOn MIDI events and add them to our list with an appropriate timestamp. */
        void onChannelMessageReceived(object obj, ChannelMessageEventArgs e)
        {
            ChannelMessage m = e.Message;
            if (debugPrintMIDI)
            {
                Console.WriteLine("Channel Message: " + m.Command + " " + m.MidiChannel + " "
                 + m.Data1.ToString() + " " + m.Data2.ToString());
            }

            if (m.Command.ToString() == "NoteOn")
            {
                if (noteEvents.Count == 0)
                    baseTime = DateTime.Now;
                noteEvents.Add(new NoteEvent(Convert.ToByte(m.Data1.ToString()), Convert.ToByte(m.Data2.ToString()), DateTime.Now - baseTime));
            }
        }

        /* The rest of these functions are just event handler methods that print
         * appropriate messages for debugging. */
        void inputDevice_Error(object obj, ErrorEventArgs e)
        {
            Console.WriteLine(e.Error.Message);
            Console.WriteLine(e.Error.StackTrace);
        }

        void onSysExMessageReceived(object obj, SysExMessageEventArgs e)
        {
            SysExMessage m = e.Message;
            if (debugPrintMIDI)
            {
                Console.WriteLine("SysEx Message: " + m.ToString());
            }
        }

        void onSysCommonMessageReceived(object obj, SysCommonMessageEventArgs e)
        {
            SysCommonMessage m = e.Message;
            if (debugPrintMIDI)
            {
                Console.WriteLine("SysCommon Message: " + m.SysCommonType.ToString() + " "
                    + m.Data1.ToString() + " " + m.Data2.ToString());
            }
        }

        void onSysRealtimeMessageReceived(object obj, SysRealtimeMessageEventArgs e)
        {
            SysRealtimeMessage m = e.Message;
            if (debugPrintMIDI)
            {
                Console.WriteLine("SysRealtime Message: " + m.Message + " " +
                    m.Status + " " + m.Timestamp);
            }
        }
    }
}

