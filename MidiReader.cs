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
    public struct NoteEvent
    {
        public byte Index;
        public byte Velocity;
        public TimeSpan Timestamp;
        public NoteEvent(byte index, byte velocity, TimeSpan timestamp)
        {
            Index = index;
            Velocity = velocity;
            Timestamp = timestamp;
        }
    }
    public class MidiReader
    {
        bool debugPrintEvents = false;
        bool debugPrintTimestamps = true;

        List<NoteEvent> noteEvents;
        const int sysExBufferSize = 128;
        InputDevice inputDevice;
        SynchronizationContext syncContext;
        DateTime baseTime;

        public MidiReader()
        {
            if (InputDevice.DeviceCount == 0)
            {
                Console.WriteLine("No MIDI devices found.");
                Thread.Sleep(2000);
                Environment.Exit(0);
                return;
            }
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

            noteEvents = new List<NoteEvent>();
            baseTime = DateTime.Now;
        }

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
                    Console.WriteLine("Note " + ne.Index + ", Vel " + ne.Velocity + ", Time " + ne.Timestamp.TotalMilliseconds);
                }
            }
        }

        public List<NoteEvent> FetchEventList()
        {
            return noteEvents;
        }

        void inputDevice_Error(object obj, ErrorEventArgs e)
        {
            Console.WriteLine(e.Error.Message);
            Console.WriteLine(e.Error.StackTrace);
        }

        void onChannelMessageReceived(object obj, ChannelMessageEventArgs e)
        {
            ChannelMessage m = e.Message;
            if (debugPrintEvents)
            {
                Console.WriteLine("Channel Message: " + m.Command + " " + m.MidiChannel + " "
                 + m.Data1.ToString() + " " + m.Data2.ToString());
            }

            if (m.Command.ToString() == "NoteOn")
            {
                noteEvents.Add(new NoteEvent(Convert.ToByte(m.Data1.ToString()), Convert.ToByte(m.Data2.ToString()), DateTime.Now - baseTime));
            }
        }

        void onSysExMessageReceived(object obj, SysExMessageEventArgs e)
        {
            SysExMessage m = e.Message;
            if (debugPrintEvents)
            {
                Console.WriteLine("SysEx Message: " + m.ToString());
            }
        }

        void onSysCommonMessageReceived(object obj, SysCommonMessageEventArgs e)
        {
            SysCommonMessage m = e.Message;
            if (debugPrintEvents)
            {
                Console.WriteLine("SysCommon Message: " + m.SysCommonType.ToString() + " "
                    + m.Data1.ToString() + " " + m.Data2.ToString());
            }
        }

        void onSysRealtimeMessageReceived(object obj, SysRealtimeMessageEventArgs e)
        {
            SysRealtimeMessage m = e.Message;
            if (debugPrintEvents)
            {
                Console.WriteLine("SysRealtime Message: " + m.Message + " " +
                    m.Status + " " + m.Timestamp);
            }
        }
    }
}

