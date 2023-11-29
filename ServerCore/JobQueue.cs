using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action _job);
    }

    public class JobQueue : IJobQueue
    {
        Queue<Action> m_jobQueue = new Queue<Action>();
        object m_lock = new object();
        bool m_flush = false;

        public void Push(Action _job)
        {
            bool flush = false;
            lock(m_lock)
            {
                m_jobQueue.Enqueue(_job);
                if (!m_flush)
                    flush = m_flush = true;
            }

            if (flush)
                Flush();
        }

        void Flush()
        {
            while(true)
            {
                Action action = Pop();
                if (action == null)
                    return;

                action.Invoke();
            }
        }

        Action Pop()
        {
            lock(m_lock)
            {
                if(m_jobQueue.Count == 0)
                {
                    m_flush = false;
                    return null;
                }

                return m_jobQueue.Dequeue();
            }
        }
    }
}
