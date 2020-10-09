using System;
using System.Collections.Generic;
using System.IO;


namespace procedural_c_
{
    class Program
    {
        static void Main(string[] args)
        {
            Procedural p = new Procedural("257","5525",$"benchmark/dijkstra/graph.csv");
        }
    }

    public struct Heap {
        public int size;
        public int maxSize;
        public (string,int)[] array;
    }

    class Procedural
    {
        Dictionary<string,int> positions = new Dictionary<string, int>();
        Dictionary<string,List<(string,int)>> edgeMap = new Dictionary<string, List<(string, int)>>();
        Dictionary<string,string> backtrack = new Dictionary<string, string>();
        Dictionary<string,int> distances = new Dictionary<string, int>();
        Heap heap;
        string position;

        public Procedural(string start, string dest, string filepath)
        {
            string[] file = File.ReadAllLines(filepath);
            int startSize = 2^10;

            heap.array = new (string, int)[startSize];
            heap.maxSize = startSize;
            heap.size = 0;

            foreach (string edge in file)
            {
                string[] line = edge.Split(",");
                if (line.Length < 3)
                    continue;
                string from = line[0], to = line[1];
                int weight = Convert.ToInt32(line[2]);
                if (edgeMap.ContainsKey(from))
                    edgeMap[from].Add((to,weight));
                else
                    edgeMap.Add(from, new List<(string,int)>{(to,weight)});

            }

            dijkstra(start,dest);
            var bt = doBacktrack();
            foreach(string line in bt)
                System.Console.Write(line+" ");
            System.Console.WriteLine();
        }

        void dijkstra(string start, string dest)
        {
            positions.Add(start,0);
            distances.Add(start,0);
            insert((start,0));
            while(heap.size > 0)
            {
                var elem = pop();
                int currDist = elem.Item2;
                position = elem.Item1;
                if(position == dest)
                    break;
                
                if(!edgeMap.ContainsKey(position))
                    continue;
                
                foreach (var (newPos,cost) in edgeMap[position])
                {
                    int alternateDist = currDist + cost;
                    if(!distances.ContainsKey(newPos))
                    {
                        distances.Add(newPos,alternateDist);
                        backtrack.Add(newPos,position);
                        positions.Add(newPos,heap.size);
                        insert((newPos,alternateDist));
                    }
                    else if (alternateDist < distances[newPos])
                    {
                        backtrack[newPos] = position;
                        distances[newPos] = alternateDist;
                        replace(newPos,alternateDist);
                    }
                }
            }

        }

        List<String> doBacktrack()
        {
            List<string> res = new List<string>{position};
            while (backtrack.ContainsKey(position))
            {
                position = backtrack[position];
                res.Prepend(position);
            }

            return res;
        }

        void replace(string pos, int newDist)
        {
            if(positions.ContainsKey(pos))
            {
                int index = positions[pos];
                swap(index,0);
                pop();
            }
            positions[pos] = heap.size;
            insert((pos,newDist));
        }

        void insert ((string,int) element)
        {
            if(heap.size == heap.maxSize)
            {
                Array.Resize<(string,int)>(ref heap.array, heap.size*2);
                heap.maxSize *= 2;
            }

            heap.array[heap.size] = element;
            heap.size += 1;
            heapifyNode(heap.size-1);
        }

        (string,int) pop()
        {
            (string,int) res = heap.array[0];
            heap.array[0] = heap.array[heap.size-1];
            heap.size -= 1;
            heapify(0);

            //Maintain dict
            positions.Remove(res.Item1);

            return res;
        }

        void heapifyNode (int index)
        {
            // Find parent 
            int parent = (index - 1) / 2; 
        
            // For Max-Heap 
            // If current node is greater than its parent 
            // Swap both of them and call heapify again 
            // for the parent 
            if (smallerThan(heap.array[index], heap.array[parent])) 
            { 
                swap(index, parent); 

                // Recursively heapify the parent node 
                heapifyNode(parent); 
            } 
        }

        void heapify (int index)
        {
            // Code from https://www.geeksforgeeks.org/heap-sort/
            int smallest = index; // Initialize smallest as root 
            int l = 2*index + 1; // left = 2*i + 1 
            int r = 2*index + 2; // right = 2*i + 2 

            // If left child is smaller than root 
            if (l < heap.size && smallerThan(heap.array[l], heap.array[smallest]))
                smallest = l; 
    
            // If right child is smaller than smallest so far 
            if (r < heap.size && smallerThan(heap.array[r], heap.array[smallest])) 
                smallest = r; 
    
            // If smallest is not root 
            if (smallest != index) 
            { 
                swap(index,smallest);
    
                // Recursively heapify the affected sub-tree 
                heapify(smallest); 
            } 
        }
        void swap (int index1, int index2)
        {
            // Maintains dictionary to find items later
            positions[heap.array[index1].Item1] = index2;
            positions[heap.array[index2].Item1] = index1;

            var swap = heap.array[index1]; 
            heap.array[index1] = heap.array[index2]; 
            heap.array[index2] = swap; 
        }

        bool smallerThan((string,int) elem1, (string,int) elem2)
        {
            if(elem1.Item2 < elem2.Item2)
                return true;
            else if (elem1.Item2 == elem2.Item2 && elem1.Item1.CompareTo(elem2.Item1) == -1)
                return true;
            else return false;
        }
    }
}
