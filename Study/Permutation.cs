using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Study
{
    internal class Permutation
    {
        public static void Start(List<int> list)
        {
            Permutate(list, list.Count);
        }

        private static void Permutate(List<int> list, int size)
        {
            if (size == 1)
            {
                foreach(var num in list)
                {
                    Console.Write($"{num} ");
                }
                Console.WriteLine();
                return;
            }

            for (var i = 0; i<size; i++)
            {
                Permutate(list, size - 1);

                if (size % 2 == 1)
                {
                    Swap(list, 0, size - 1);
                }
                else
                {
                    Swap(list, i, size - 1);
                }
            }
        }

        private static void Swap(List<int> list, int index1, int index2)
        {
            if (index1 == index2)
            {
                return;
            }
            var temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        public static void Combination1(List<int> list, int index, int r, int depth, List<int> result) // 1, 2, 3
        {
            if (depth == r)
            {
                foreach (var num in result)
                {
                    Console.Write($"{num} ");
                }
                Console.WriteLine();
                return;
            }

            for(var i = index; i < list.Count; i++)
            {
                result.Add(list[i]);
                Combination1(list, i + 1, r, depth + 1, result);
                result.RemoveAt(result.Count - 1);
            }
        }

        public static void Combination2(List<int> list, int index, int r, int depth, List<int> result)
        {
            if (r == 0)
            {
                foreach (var num in result)
                {
                    Console.Write($"{num} ");
                }
                Console.WriteLine();
            }
            else if (depth == list.Count)
            {
                return;
            }
            else
            {
                result[index] = list[depth];
                Combination2(list, index + 1, r - 1, depth + 1, result);
                Combination2(list, index, r, depth + 1, result);
            }
        }
    }
}
