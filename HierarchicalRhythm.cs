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

        public void Print()
        {
            // TODO: This needs to be BFS, not DFS
            Console.Write("Node: ");
            if (drums.Count > 0)
            {
                foreach(Drum drum in drums)
                {
                    Console.Write(drum + " ");
                }
            }
            Console.WriteLine();

            if (Left != null)
            {
                Console.WriteLine("Left: {");
                Left.Print();
                Console.WriteLine("}");
            }
            if (Right != null)
            {
                Console.WriteLine("Right: {");
                Right.Print();
                Console.WriteLine("}");
            }
        }
    }

    public class HierarchicalRhythm
    {
        public HierarchicalRhythmNode Root;
        public double interval;

        // Here "depth" is the base 2 logarithm of the number of units in the rhythm.
        public HierarchicalRhythm(double _interval, int depth)
        {
            interval = _interval;
            Root = new HierarchicalRhythmNode();
            Root.Deepen(depth);
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
            Root.Print();
        }
    }
}
