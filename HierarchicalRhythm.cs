using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_Drumkit_Parser
{
    public class HierarchicalRhythmNode
    {
        public HierarchicalRhythmNode Left;
        public HierarchicalRhythmNode Right;
        public HashSet<Drum> drums;

        public HierarchicalRhythmNode()
        {
            drums = new HashSet<Drum>();
        }

        public void SetDrums(HashSet<Drum> _drums)
        {
            drums = _drums;
        }

        public void Deepen(int depth)
        {
            if (depth == 0)
            {
                return;
            }
            else
            {
                Left = new HierarchicalRhythmNode();
                Left.Deepen(depth - 1);
                Right = new HierarchicalRhythmNode();
                Right.Deepen(depth - 1);
            }
        }

        public void Print(int level)
        {
            if (level == 0)
            {
                Console.Write("{");
                foreach(Drum drum in drums)
                {
                    Console.Write(drum + " ");
                }
                Console.Write("} ");
            }
            else
            {
                Left.Print(level - 1);
                Right.Print(level - 1);
            }
        }
    }

    public class HierarchicalRhythm
    {
        public HierarchicalRhythmNode Root;
        public double interval;
        private int depth;

        // Here "depth" is the base 2 logarithm of the number of units in the rhythm.
        public HierarchicalRhythm(double _interval, int _depth)
        {
            interval = _interval;
            Root = new HierarchicalRhythmNode();
            Root.Deepen(_depth);
            depth = _depth;
        }

        // Here "level" is the base 2 logarithm of the number of units on that level.
        private void AddDrumAt(int level, int index, Drum drum, HierarchicalRhythmNode node)
        {
            if (level < 0)
                throw new Exception("Attempted to add drum at level " + level);

            // Top of tree, insert here
            if (level == 0)
            {
                node.drums.Add(drum);
            }
            else
            {
                // Go down the left branch
                if (index < Math.Pow(2, level))
                {
                    AddDrumAt(level - 1, index, drum, node.Left);
                }
                // Go down the right branch
                else
                {
                    int newIndex = index - (int)Math.Pow(2, level - 1);
                    AddDrumAt(level - 1, newIndex, drum, node.Right);
                }
            }
        }

        public void AddDrum(int level, int index, Drum drum)
        {
            AddDrumAt(level, index, drum, Root);
        }

        public void Print()
        {
            for (int i = 0; i <= depth; i++)
            {
                Root.Print(i);
                Console.WriteLine();
            }
        }
    }
}
