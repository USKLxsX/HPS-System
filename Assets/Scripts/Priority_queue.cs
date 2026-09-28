using System;
using System.Collections.Generic;

public class Patient
{
    public int id;
    public string name;
    public int priority;
    public int arrivetime;
}

public class Priority_queue
{
    List<Patient> queue;
    public int count=>queue.Count;

    public Priority_queue()
    {
        queue = new List<Patient>();
    }

    public void Push(Patient item)
    {
        queue.Add(item);
        up(count - 1);
    }

    public Patient Pop()
    {
        if (count == 0){
            throw new InvalidOperationException("优先队列为空");
        }

        var item = queue[0];
        queue[0] = queue[count - 1];
        queue.RemoveAt(count - 1);
        down(0);
        return item;
    }

    public bool Empty()
    {
        return queue.Count == 0;
    }

    bool Bigger(int n1,int n2)
    {
        Patient p1 = queue[n1];
        Patient p2 = queue[n2];
        if (p1.priority == p2.priority)
        {
            return p1.arrivetime < p2.arrivetime;
        }
        else
        {
            return p1.priority > p2.priority;
        }
    }

    void up(int id)
    {
        int parent = (id - 1) / 2;
        while (id > 0)
        {
            if(Bigger(id,parent))
            {
                Swap(id, parent);
                id = parent;
                parent = (id - 1) / 2;
            }
            else
            {
                break;
            }
        }
    }

    void down(int id)
    {
        int left = id * 2 + 1;
        int right = id * 2 + 2;
        int index = id;
        while (true)
        {
            if (left < count && Bigger(left, index))
            {
                index = left;
            }
            if(right < count && Bigger(right, index))
            {
                index = right;
            }
            if (index == id)
            {
                break;
            }

            Swap(index, id);
            id = index;
            left = id * 2 + 1;
            right = id * 2 + 2;
        }
    }

    void Swap(int i, int j)
    {
        var temp = queue[i];
        queue[i] = queue[j];
        queue[j] = temp;
    }

    public List<Patient> GetAllPatients()
    {
        return new List<Patient>(queue);
    }

}
