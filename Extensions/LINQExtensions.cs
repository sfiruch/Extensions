using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public static class LINQExtensions
{
    private static int MurmurHash(int _input)
    {
        unchecked
        {
            //adapted from https://stackoverflow.com/a/3818803/742404
            const uint MURMURHASH2A_MULTIPLIER = 0x5bd1e995;

            //update hash with _input
            uint mmh2ak = (uint)unchecked((_input) * MURMURHASH2A_MULTIPLIER);
            mmh2ak ^= mmh2ak >> 24;
            mmh2ak *= MURMURHASH2A_MULTIPLIER;
            uint hash = unchecked(2166136261U * MURMURHASH2A_MULTIPLIER);
            hash ^= mmh2ak;
            hash ^= hash >> 13;
            hash *= MURMURHASH2A_MULTIPLIER;
            hash ^= hash >> 15;
            return (int)hash;
        }
    }

    public static T RandomElement<T>(this IList<T> _source, Random _rng)
        => _source[_rng.Next(_source.Count)];

    public static T RandomElement<T>(this IEnumerable<T> _source, Random _rng)
        => _source.ElementAt(_rng.Next(_source.Count()));

    public static T RandomElement<T>(this IEnumerable<T> _source, int _seed)
        => _source.ElementAt((_seed & int.MaxValue) % _source.Count());

    /// <summary>
    /// Compute the geometric mean in a numerically robust fashion.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_source"></param>
    /// <param name="_selector"></param>
    /// <returns></returns>
    public static double GeometricMean<T>(this IEnumerable<T> _source, Func<T, double> _selector)
    {
        var totalSum = 0d;
        var totalCount = 0;

        foreach (var v in _source)
        {
            totalSum += System.Math.Log(_selector(v));
            totalCount++;
        }

        return Math.Exp(totalSum / totalCount);
    }

    /// <summary>
    /// Computes the median
    /// </summary>
    /// <param name="_source"></param>
    /// <returns></returns>
    public static double Median(this IEnumerable<int> _source)
    {
        var data = _source.OrderBy(n => n).ToArray();
        if ((data.Length & 1) == 0)
            return 0.5 * (data[data.Length / 2 - 1] + data[data.Length / 2]);
        else
            return data[data.Length / 2];
    }

    /// <summary>
    /// Computes the median
    /// </summary>
    /// <param name="_source"></param>
    /// <returns></returns>
    public static float Median(this IEnumerable<float> _source)
    {
        var data = _source.OrderBy(n => n).ToArray();
        if ((data.Length & 1) == 0)
            return 0.5f * (data[data.Length / 2 - 1] + data[data.Length / 2]);
        else
            return data[data.Length / 2];
    }

    /// <summary>
    /// Computes the median
    /// </summary>
    /// <param name="_source"></param>
    /// <returns></returns>
    public static double Median(this IEnumerable<double> _source)
    {
        var data = _source.OrderBy(n => n).ToArray();
        if ((data.Length & 1) == 0)
            return 0.5 * (data[data.Length / 2 - 1] + data[data.Length / 2]);
        else
            return data[data.Length / 2];
    }

    public static double Median<TSource>(this IEnumerable<TSource> _source, Func<TSource, int> _selector) => _source.Select(_selector).Median();
    public static float Median<TSource>(this IEnumerable<TSource> _source, Func<TSource, float> _selector) => _source.Select(_selector).Median();
    public static double Median<TSource>(this IEnumerable<TSource> _source, Func<TSource, double> _selector) => _source.Select(_selector).Median();

    /// <summary>
    /// Combines multiple arrays into one big array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_arrays"></param>
    /// <returns></returns>
    public static T[] Combine<T>(this IEnumerable<T[]> _arrays) => _arrays.SelectMany(i => i).ToArray();


    /// <summary>
    /// Computes the standard deviation of the supplied values
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static double StdDev(this IEnumerable<int> values)
    {
        var count = values.Count();
        if (count <= 1)
            return 0;

        var avg = values.Average();
        return System.Math.Sqrt(values.Sum(d => (d - avg) * (d - avg)) / count);
    }

    /// <summary>
    /// Computes the standard deviation of the supplied values
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static double StdDev(this IEnumerable<float> values)
    {
        var count = values.Count();
        if (count <= 1)
            return 0;

        var avg = values.Average();
        return System.Math.Sqrt(values.Sum(d => (d - avg) * (d - avg)) / count);
    }

    /// <summary>
    /// Computes the standard deviation of the supplied values
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static double StdDev(this IEnumerable<double> values)
    {
        var count = values.Count();
        if (count <= 1)
            return 0;

        var avg = values.Average();
        return System.Math.Sqrt(values.Sum(d => (d - avg) * (d - avg)) / count);
    }

    public static int IndexOf<T>(this IEnumerable<T> _source, T _element, int _default)
    {
        var i = 0;
        if (ReferenceEquals(_element, null))
            foreach (var e in _source)
            {
                if (ReferenceEquals(e, null))
                    return i;
                i++;
            }
        else
            foreach (var e in _source)
            {
                if (!ReferenceEquals(_element, null) && _element.Equals(e))
                    return i;
                i++;
            }

        return _default;
    }

    public static int IndexOf<T>(this IEnumerable<T> _source, T _element)
    {
        var r = _source.IndexOf(_element, -1);
        if (r == -1)
            throw new KeyNotFoundException();
        return r;
    }



    ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
    ///<param name="_source">The enumerable to search.</param>
    ///<param name="_predicate">The expression to test the items against.</param>
    ///<returns>The index of the first matching item, or -1 if no items match.</returns>
    public static int IndexOf<T>(this IEnumerable<T> _source, Func<T, bool> _predicate, int _default)
    {
        var alreadyFound = false;
        var result = _default;
        var i = 0;
        foreach (var e in _source)
        {
            if (_predicate(e))
            {
                if (alreadyFound)
                    throw new InvalidOperationException();

                alreadyFound = true;
                result = i;
            }
            i++;
        }
        return result;
    }

    public static int IndexOf<T>(this IEnumerable<T> _source, Func<T, bool> _predicate)
    {
        var r = _source.IndexOf(_predicate, -1);
        if (r == -1)
            throw new KeyNotFoundException();
        return r;
    }

    /// <summary>
    /// Computes _elements.AsParallel().SelectMany(_selector).Take(_limit) but more
    /// efficiently. PLINQ invokes the _selector for *all* elements, even if enough
    /// elements have been collected.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <param name="_elements"></param>
    /// <param name="_selector"></param>
    /// <param name="_limit"></param>
    /// <returns></returns>
    public static IEnumerable<R> ParallelReduceTake<T, R>(this IEnumerable<T> _elements, Func<T, IEnumerable<R>> _selector, int _limit)
    {
        var b = new BlockingCollection<(int Index, T Element)>(1);
        var results = new Dictionary<int, R[]>();
        var elementCount = new Dictionary<int, int>();

        int? GetNumberOfElementsBefore(int _index)
        {
            if (_index < 0)
                throw new ArgumentOutOfRangeException(nameof(_index));

            if (elementCount.TryGetValue(_index, out var count))
                return count;

            var prev = (_index == 0) ? 0 : GetNumberOfElementsBefore(_index - 1);
            if (!prev.HasValue)
                return null;

            if (!results.TryGetValue(_index, out var r))
                return null;

            return elementCount[_index] = r.Length + prev.Value;
        }

        var tasks = Enumerable.Range(0, Environment.ProcessorCount).Select(core => Task.Run(() =>
        {
            try
            {
                for (; ; )
                {
                    T element;
                    int index;
                    try
                    {
                        (index, element) = b.Take();
                    }
                    catch (InvalidOperationException)
                    {
                        if (b.IsCompleted)
                            return;
                        else
                            throw;
                    }

                    var elementResults = _selector(element).ToArray();
                    lock (results)
                    {
                        results[index] = elementResults;
                        for (var i = index; i >= 0; i--)
                        {
                            var noe = GetNumberOfElementsBefore(i);
                            if (noe >= _limit)
                            {
                                b.CompleteAdding();
                                return;
                            }
                            if (noe < _limit)
                                break;
                        }
                    }
                }
            }
            catch
            {
                b.CompleteAdding();
                throw;
            }
        })).ToArray();

        {
            var index = 0;
            foreach (var element in _elements)
                try
                {
                    b.Add((index++, element));
                }
                catch (InvalidOperationException)
                {
                    if (b.IsAddingCompleted)
                        break;
                    else
                        throw;
                }
        }

        b.CompleteAdding();
        Task.WaitAll(tasks);

        return results
            .OrderBy(e => e.Key)
            .SelectMany(e => e.Value)
            .Take(_limit);
    }


    /// <summary>
    /// Partitions the given list around a pivot element such that all elements on left of pivot are <= pivot
    /// and the ones at thr right are > pivot. This method can be used for sorting, N-order statistics such as
    /// as median finding algorithms.
    /// Reference: Introduction to Algorithms 3rd Edition, Corman et al, pp 171
    /// </summary>
    private static int Partition<T>(this T[] list, int start, int end) where T : IComparable<T>
    {
        list.Swap(end, (start + end) / 2);

        var pivot = list[end];
        var lastLow = start - 1;
        for (var i = start; i < end; i++)
            if (list[i].CompareTo(pivot) <= 0)
                list.Swap(i, ++lastLow);
        list.Swap(end, ++lastLow);
        return lastLow;
    }

    /// <summary>
    /// Returns Nth smallest element from the list. Here n starts from 0 so that n=0 returns minimum, n=1 returns 2nd smallest element etc.
    /// Note: specified list would be mutated in the process.
    /// Reference: Introduction to Algorithms 3rd Edition, Corman et al, pp 216
    /// </summary>
    private static T NthOrderStatistic<T>(this T[] list, int n) where T : IComparable<T>
        => NthOrderStatistic(list, n, 0, list.Length - 1);

    private static T NthOrderStatistic<T>(this T[] list, int n, int start, int end) where T : IComparable<T>
    {
        Debug.Assert(n < list.Length);
        Debug.Assert(end >= 0);
        Debug.Assert(end < list.Length);

        for (; ; )
        {
            var pivotIndex = list.Partition(start, end);
            if (pivotIndex == n)
                return list[pivotIndex];

            if (n < pivotIndex)
                end = pivotIndex - 1;
            else
                start = pivotIndex + 1;
        }
    }

    public static void Swap<T>(this T[] list, int i, int j)
    {
        if (i != j)
            (list[i], list[j]) = (list[j], list[i]);
    }

    /// <summary>
    /// Attention: THIS WILL REORDER THE LIST!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static T MedianWithReordering<T>(this T[] list) where T : IComparable<T>
        => list.NthOrderStatistic((list.Length - 1) / 2);



    // COPYPASTA FROM http://stackoverflow.com/questions/33336540/how-to-use-linq-to-find-all-combinations-of-n-items-from-a-set-of-numbers
    public static IEnumerable<IEnumerable<T>> AllCombinations<T>(this IEnumerable<T> elements, int k)
    {
        return k == 0 ? new[] { new T[0] } :
          elements.SelectMany((e, i) =>
            elements.Skip(i + 1).AllCombinations(k - 1).Select(c => (new[] { e }).Concat(c)));
    }
}
