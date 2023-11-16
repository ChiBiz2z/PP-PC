using System;
using System.Threading;

namespace ProducerConsumer;

public class RingBuffer<T>
{
    private readonly T[] _buffer;
    private readonly int _capacity;
    private int _head;
    private int _tail;

    private readonly SemaphoreSlim _empty;
    private readonly SemaphoreSlim _mutex;
    private readonly SemaphoreSlim _full;

    public RingBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.");


        _capacity = capacity;
        _buffer = new T[capacity];
        _head = 0;
        _tail = 0;

        _full = new SemaphoreSlim(0, capacity);
        _empty = new SemaphoreSlim(capacity, capacity);
        _mutex = new SemaphoreSlim(1, 1);
    }

    public bool CheckDeq(Predicate<T> predicate)
    {
        _full.Wait();
        _mutex.Wait();
        var result = predicate(_buffer[_head]);
        _mutex.Release();
        _full.Release();
        return result;
    }


    public void Enqueue(T item)
    {
        _empty.Wait();
        _mutex.Wait();
        _buffer[_tail] = item;
        _tail = (_tail + 1) % _capacity;
        _mutex.Release();
        _full.Release();
    }


    public T Dequeue()
    {
        _full.Wait();
        _mutex.Wait();
        var item = _buffer[_head];
        _head = (_head + 1) % _capacity;
        _mutex.Release();
        _empty.Release();
        return item;
    }
}