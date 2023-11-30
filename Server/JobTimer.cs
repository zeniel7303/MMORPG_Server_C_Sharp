using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server
{
	struct JobTimerElem : IComparable<JobTimerElem>
	{
		public int execTick; // 실행 시간
		public Action action;

		public int CompareTo(JobTimerElem _other)
		{
			return _other.execTick - execTick;
		}
	}

	class JobTimer
	{
		PriorityQueue<JobTimerElem> m_pq = new PriorityQueue<JobTimerElem>();
		object m_lock = new object();

		public static JobTimer Instance { get; } = new JobTimer();

		public void Push(Action action, int tickAfter = 0)
		{
			JobTimerElem job;
			job.execTick = System.Environment.TickCount + tickAfter;
			job.action = action;

			lock (m_lock)
			{
				m_pq.Push(job);
			}
		}

		public void Flush()
		{
			while (true)
			{
				int now = System.Environment.TickCount;

				JobTimerElem job;

				lock (m_lock)
				{
					if (m_pq.Count == 0)
						break;

					job = m_pq.Peek();
					if (job.execTick > now)
						break;

					m_pq.Pop();
				}

				job.action.Invoke();
			}
		}
	}
}
