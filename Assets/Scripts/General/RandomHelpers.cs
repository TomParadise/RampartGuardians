using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomHelpers
{
    //generates a random list of ints between min(inclusive) and max(exclusive)
    //ignore parameter will not be included in the list
    public static List<int> GenerateRandomList(int min, int max, int? ignore = null)
    {
        var numbers = new List<int>(max);
        for (int i = min; i < max; i++)
        {
            if (ignore == null ||
                (ignore != null && i != ignore.Value))
            {
                numbers.Add(i);
            }
        }
        List<int> randomList = new List<int>();
        int listSize = numbers.Count;
        for (int i = 0; i < listSize; i++)
        {
            var thisNumber = Random.Range(0, numbers.Count);
            randomList.Add(numbers[thisNumber]);
            numbers.RemoveAt(thisNumber);
        }
        return randomList;
    }

    //generates a random list of ints between min(inclusive) and max(exclusive)
    //ignore parameter will not be included in the list
    public static List<int> GenerateRandomList(int min, int max, List<int> ignore)
    {
        var numbers = new List<int>(max);
        for (int i = min; i < max; i++)
        {
            if (!ignore.Contains(i))
            {
                numbers.Add(i);
            }
        }
        List<int> randomList = new List<int>();
        int listSize = numbers.Count;
        for (int i = 0; i < listSize; i++)
        {
            var thisNumber = Random.Range(0, numbers.Count);
            randomList.Add(numbers[thisNumber]);
            numbers.RemoveAt(thisNumber);
        }
        return randomList;
    }

    //generates a randomised list containing a set amount of numbers between min(inclusive) and max(exclusive)
    public static List<int> GenerateRandomListOfCount(int min, int max, int count)
    {
        var numbers = new List<int>(max);
        for (int i = min; i < max; i++)
        {
            numbers.Add(i);
        }
        for(int i = 0; i < count; i++)
        {
            numbers.RemoveAt(Random.Range(0, numbers.Count));
        }
        return numbers;
    }
}
