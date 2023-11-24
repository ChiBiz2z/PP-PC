using System;
using System.Linq;
using System.Threading;

namespace ProducerConsumer;

public class RingBuffer
{
    private readonly string[] _buffer;
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
        _buffer = new string[capacity];
        _head = 0;
        _tail = 0;

        _full = new SemaphoreSlim(0, capacity);
        _empty = new SemaphoreSlim(capacity, capacity);
        _mutex = new SemaphoreSlim(1, 1);
    }

    public void Enqueue(string item)
    {
        _empty.Wait();
        _mutex.Wait();
        _buffer[_tail] = item;
        _tail = (_tail + 1) % _capacity;
        _mutex.Release();
        _full.Release();
    }

    public string Dequeue(Predicate<char> predicate)
    {
        _full.Wait();
        _mutex.Wait();
        var check = predicate(_buffer[_head].ToCharArray().FirstOrDefault());
        var item = _buffer[_head];
        if (check)
        {
            _head = (_head + 1) % _capacity;
            _empty.Release();
        }
        else
        {
            _full.Release();
        }

        _mutex.Release();
        return check ? item : string.Empty;
    }
}