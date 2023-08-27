using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mokkit.Containers.Moq;

public class MockCollection<TMock> : IMockCollection<TMock>
{
    private readonly List<MockRegistration<TMock>> _mocks = [];
    private bool _isReadOnly;

    public IMockCollection<TMock> AddMock<T>(Func<TMock> factory)
    {
        var existing = _mocks.FirstOrDefault(x => x.InnerType == typeof(T));
        
        if (existing != null)
        {
            throw new InvalidOperationException($"Mock for type {typeof(T)} already added to container.");
        }
        
        _mocks.Add(new MockRegistration<TMock>(typeof(T), factory));

        return this;
    }
    
    public IMockCollection<TMock> TryAddMock<T>(Func<TMock> factory)
    {
        var existing = _mocks.FirstOrDefault(x => x.InnerType == typeof(T));

        if (existing != null)
        {
            return this;
        }

        _mocks.Add(new MockRegistration<TMock>(typeof(T), factory));

        return this;
    }

    public IReadOnlyCollection<MockRegistration<TMock>> Registrations => this.ToList().AsReadOnly();

    public IEnumerator<MockRegistration<TMock>> GetEnumerator()
    {
        return _mocks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_mocks).GetEnumerator();
    }

    public void Add(MockRegistration<TMock> item)
    {
        _mocks.Add(item);
    }

    public void Clear()
    {
        _mocks.Clear();
    }

    public bool Contains(MockRegistration<TMock> item)
    {
        return _mocks.Contains(item);
    }

    public void CopyTo(MockRegistration<TMock>[] array, int arrayIndex)
    {
        _mocks.CopyTo(array, arrayIndex);
    }

    public bool Remove(MockRegistration<TMock> item)
    {
        return _mocks.Remove(item);
    }

    public int Count => _mocks.Count;

    public bool IsReadOnly => _isReadOnly;

    public int IndexOf(MockRegistration<TMock> item)
    {
        return _mocks.IndexOf(item);
    }

    public void Insert(int index, MockRegistration<TMock> item)
    {
        _mocks.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _mocks.RemoveAt(index);
    }

    public MockRegistration<TMock> this[int index]
    {
        get => _mocks[index];
        set => _mocks[index] = value;
    }

    public void MakeReadOnly()
    {
        _isReadOnly = true;
    }
}