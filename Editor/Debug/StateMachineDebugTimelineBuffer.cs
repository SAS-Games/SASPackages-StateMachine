using System;

namespace SAS.StateMachineGraph.Editor
{
    internal sealed class StateMachineDebugTimelineBuffer
    {
        internal const int DefaultCapacity = 1000;

        private readonly StateMachineDebugTimelineEntry[] _entries;
        private int _start;
        private int _count;

        internal StateMachineDebugTimelineBuffer(int capacity = DefaultCapacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _entries = new StateMachineDebugTimelineEntry[capacity];
        }

        internal int Count => _count;
        internal int Capacity => _entries.Length;

        internal void Add(StateMachineDebugTimelineEntry entry)
        {
            if (_count == _entries.Length)
            {
                _entries[_start] = entry;
                _start = (_start + 1) % _entries.Length;
                return;
            }

            var index = (_start + _count) % _entries.Length;
            _entries[index] = entry;
            _count++;
        }

        internal StateMachineDebugTimelineEntry GetEntry(int index)
        {
            if (index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _entries[(_start + index) % _entries.Length];
        }

        internal void Clear()
        {
            _start = 0;
            _count = 0;
        }
    }
}
