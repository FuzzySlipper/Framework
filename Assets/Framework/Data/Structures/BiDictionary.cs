using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    /// <summary>
/// This is a dictionary guaranteed to have only one of each value and key. 
/// It may be searched either by TFirst or by TSecond, giving a unique answer because it is 1 to 1.
/// It implements garbage-collector-friendly IEnumerable.
/// </summary>
/// <remarks>From https://stackoverflow.com/a/35949314/1460422</remarks>
/// <typeparam name="TFirst">The type of the "key"</typeparam>
/// <typeparam name="TSecond">The type of the "value"</typeparam>
public class BiDictionary<TFirst, TSecond> : IEnumerable<BiDictionary<TFirst, TSecond>.Pair>
{


    public struct Pair
    {
        public TFirst  First;
        public TSecond Second;
    }


    public struct Enumerator : IEnumerator<Pair>, IEnumerator
    {

        public Enumerator(Dictionary<TFirst, TSecond>.Enumerator dictEnumerator)
        {
            _dictEnumerator = dictEnumerator;
        }

        public Pair Current
        {
            get
            {
                Pair pair;
                pair.First = _dictEnumerator.Current.Key;
                pair.Second = _dictEnumerator.Current.Value;
                return pair;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public void Dispose()
        {
            _dictEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            return _dictEnumerator.MoveNext();
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        private Dictionary<TFirst, TSecond>.Enumerator _dictEnumerator;

    }

    /// <summary>
    /// Tries to add the pair to the dictionary.
    /// Throws an exception if either element is already in the dictionary
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    public void Add(TFirst first, TSecond second)
    {
        if (_firstToSecond.ContainsKey(first) || _secondToFirst.ContainsKey(second))
            throw new ArgumentException("Duplicate first or second");

        _firstToSecond.Add(first, second);
        _secondToFirst.Add(second, first);
    }

    /// <summary>
    /// Find the TSecond corresponding to the TFirst first
    /// Throws an exception if first is not in the dictionary.
    /// </summary>
    /// <param name="first">the key to search for</param>
    /// <returns>the value corresponding to first</returns>
    public TSecond GetByFirst(TFirst first)
    {
        TSecond second;
        if (!_firstToSecond.TryGetValue(first, out second))
            throw new ArgumentException("first");

        return second;
    }

    /// <summary>
    /// Find the TFirst corresponing to the Second second.
    /// Throws an exception if second is not in the dictionary.
    /// </summary>
    /// <param name="second">the key to search for</param>
    /// <returns>the value corresponding to second</returns>
    public TFirst GetBySecond(TSecond second)
    {
        TFirst first;
        if (!_secondToFirst.TryGetValue(second, out first))
            throw new ArgumentException("second");

        return first;
    }


    /// <summary>
    /// Remove the record containing first.
    /// If first is not in the dictionary, throws an Exception.
    /// </summary>
    /// <param name="first">the key of the record to delete</param>
    public void RemoveByFirst(TFirst first)
    {
        TSecond second;
        if (!_firstToSecond.TryGetValue(first, out second))
            throw new ArgumentException("first");

        _firstToSecond.Remove(first);
        _secondToFirst.Remove(second);
    }

    /// <summary>
    /// Remove the record containing second.
    /// If second is not in the dictionary, throws an Exception.
    /// </summary>
    /// <param name="second">the key of the record to delete</param>
    public void RemoveBySecond(TSecond second)
    {
        TFirst first;
        if (!_secondToFirst.TryGetValue(second, out first))
            throw new ArgumentException("second");

        _secondToFirst.Remove(second);
        _firstToSecond.Remove(first);
    }

    /// <summary>
    /// Tries to add the pair to the dictionary.
    /// Returns false if either element is already in the dictionary        
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns>true if successfully added, false if either element are already in the dictionary</returns>
    public bool TryAdd(TFirst first, TSecond second)
    {
        if (_firstToSecond.ContainsKey(first) || _secondToFirst.ContainsKey(second))
            return false;

        _firstToSecond.Add(first, second);
        _secondToFirst.Add(second, first);
        return true;
    }


    /// <summary>
    /// Find the TSecond corresponding to the TFirst first.
    /// Returns false if first is not in the dictionary.
    /// </summary>
    /// <param name="first">the key to search for</param>
    /// <param name="second">the corresponding value</param>
    /// <returns>true if first is in the dictionary, false otherwise</returns>
    public bool TryGetByFirst(TFirst first, out TSecond second)
    {
        return _firstToSecond.TryGetValue(first, out second);
    }

    /// <summary>
    /// Find the TFirst corresponding to the TSecond second.
    /// Returns false if second is not in the dictionary.
    /// </summary>
    /// <param name="second">the key to search for</param>
    /// <param name="first">the corresponding value</param>
    /// <returns>true if second is in the dictionary, false otherwise</returns>
    public bool TryGetBySecond(TSecond second, out TFirst first)
    {
        return _secondToFirst.TryGetValue(second, out first);
    }

    /// <summary>
    /// Remove the record containing first, if there is one.
    /// </summary>
    /// <param name="first"></param>
    /// <returns> If first is not in the dictionary, returns false, otherwise true</returns>
    public bool TryRemoveByFirst(TFirst first)
    {
        TSecond second;
        if (!_firstToSecond.TryGetValue(first, out second))
            return false;

        _firstToSecond.Remove(first);
        _secondToFirst.Remove(second);
        return true;
    }

    /// <summary>
    /// Remove the record containing second, if there is one.
    /// </summary>
    /// <param name="second"></param>
    /// <returns> If second is not in the dictionary, returns false, otherwise true</returns>
    public bool TryRemoveBySecond(TSecond second)
    {
        TFirst first;
        if (!_secondToFirst.TryGetValue(second, out first))
            return false;

        _secondToFirst.Remove(second);
        _firstToSecond.Remove(first);
        return true;
    }

    /// <summary>
    /// The number of pairs stored in the dictionary
    /// </summary>
    public Int32 Count
    {
        get { return _firstToSecond.Count; }
    }

    /// <summary>
    /// Removes all items from the dictionary.
    /// </summary>
    public void Clear()
    {
        _firstToSecond.Clear();
        _secondToFirst.Clear();
    }


    public Enumerator GetEnumerator()
    {
        //enumerator.Reset(firstToSecond.GetEnumerator());
        return new Enumerator(_firstToSecond.GetEnumerator());
    }

    IEnumerator<Pair> IEnumerable<Pair>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }



    private Dictionary<TFirst, TSecond> _firstToSecond  = new Dictionary<TFirst, TSecond>();
    private Dictionary<TSecond, TFirst> _secondToFirst  = new Dictionary<TSecond, TFirst>();

}
}
