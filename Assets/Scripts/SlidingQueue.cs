using System.Collections.Generic;

public struct Record
{
    public int finishtime;
    public int waittime;
    public int treattime;
    public int priority;
}

public class SlidingQueue
{
    Queue<Record> queue;
    public int count=>queue.Count;
    public int size;

    public SlidingQueue(int size)
    {
        queue = new Queue<Record>();
        this.size = size;
    }

    public void Push(Record item)
    {
        queue.Enqueue(item);
    }

    public void Update(int nowtime)
    {
        while(queue.Count > 0 && queue.Peek().finishtime < nowtime-size)
        {
            queue.Dequeue();
        }
    }

    public List<Record> Getlist()
    {
        return new List<Record>(queue);
    }
}
