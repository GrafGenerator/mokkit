using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mokkit.Containers.MockContainer;

public class MockCollection<TMock> : IMockCollection<TMock>
{
    private readonly List<MockRegistration<TMock>> _mocks = [];
    private bool _isReadOnly;

    public IMockCollection<TMock> AddMock<T>(TMock mock)
    {
        _mocks.Add(new MockRegistration<TMock>(typeof(T), mock));
        return this;
    }
    
    public IMockCollection<TMock> TryAddMock<T>(TMock mock)
    {
        var existing = _mocks.FirstOrDefault(x => x.Type == typeof(T));

        if (existing != null)
        {
            return this;
        }

        _mocks.Add(new MockRegistration<TMock>(typeof(T), mock));

        return this;
    }

    public TMock? GetMock<T>()
    {
        throw new System.NotImplementedException();
    }

    public TMock GetRequiredMock<T>()
    {
        throw new System.NotImplementedException();
    }

    public IReadOnlyCollection<MockRegistration<TMock>> Registrations => _mocks;

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

    public void MakeRedOnly()
    {
        _isReadOnly = true;
    }
}