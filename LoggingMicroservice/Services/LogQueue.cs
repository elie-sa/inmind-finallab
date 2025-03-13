using System.Collections.Concurrent;
using LoggingMicroservice.Models;

namespace LoggingMicroservice.Services;

public class LogQueue
{
    private readonly ConcurrentQueue<Log> _logQueue = new();

    public void Enqueue(Log log)
    {
        _logQueue.Enqueue(log);
    }

    public List<Log> DequeueAll()
    {
        var logs = new List<Log>();

        while (_logQueue.TryDequeue(out var log))
        {
            logs.Add(log);
        }

        return logs;
    }

    public int Count() => _logQueue.Count;
}