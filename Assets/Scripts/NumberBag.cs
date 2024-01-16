using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class NumberBag
{
    private List<int> _bagNumbers;

    public NumberBag(SudokuBoard board, int seed)
    {
        _bagNumbers = new List<int>();

        Random random = new Random(seed);
        Dictionary<int, int> missingNumberCounts = board.GetMissingNumberCounts();

        foreach (int key in missingNumberCounts.Keys)
        {
            for (int i = 0; i < missingNumberCounts[key]; i++)
            {
                _bagNumbers.Add(key);
            }
        }

        for (int i = 0; i < _bagNumbers.Count * 10; i++)
        {
            int indexA = random.Next(0, _bagNumbers.Count);
            int indexB = random.Next(0, _bagNumbers.Count);

            (_bagNumbers[indexA], _bagNumbers[indexB]) = (_bagNumbers[indexB], _bagNumbers[indexA]);
        }
    }


    public List<int> PeekNextNumbers(int count)
    {
        count = Mathf.Min(count, _bagNumbers.Count);

        List<int> output = new List<int>();

        for (int i = 0; i < count; i++)
        {
            output.Add(_bagNumbers[i]);
        }

        return output;
    }

    public void ConsumeNumbers(List<int> numbers)
    {
        foreach (int number in numbers)
        {
            _bagNumbers.Remove(number);
        }
    }
}