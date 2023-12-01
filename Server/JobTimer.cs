using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server
{
	struct JobTimerElem : IComparable<JobTimerElem>
	{
		public int execTick; // 실행 시간
		public Action action; // 행위

		public int CompareTo(JobTimerElem _other)
		{
			// execTick이게 작을수록 먼저 나오길 기대하므로 상대방의 틱에서 자신의 틱을 뺀다.
			return _other.execTick - execTick;
		}
	}

	class JobTimer
	{
		PriorityQueue<JobTimerElem> m_priorityQueue = new PriorityQueue<JobTimerElem>();
		object m_lock = new object();

		public static JobTimer Instance { get; } = new JobTimer();

		public void Push(Action _action, int _tickAfter = 0)
		{
			JobTimerElem job;
			job.execTick = System.Environment.TickCount + _tickAfter;
			job.action = _action;

			lock (m_lock)
			{
				m_priorityQueue.Push(job);
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
					if (m_priorityQueue.Count == 0)
						break;

					job = m_priorityQueue.Peek();
					if (job.execTick > now)
						break;

					m_priorityQueue.Pop();
				}

				job.action.Invoke();
			}
		}
	}
}
