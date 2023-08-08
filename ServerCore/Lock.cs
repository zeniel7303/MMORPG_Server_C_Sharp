using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
	// 사실 C#엔 이미 구현되어있는게 있다.
	class SpicLock
	{
		volatile int m_locked = 0;

		public void Acquire()
		{
			while (true)
			{
				//int original = Interlocked.Exchange(ref m_locked, 1);
				//if (original == 0)
				//	break;

				int expected = 0;
				int desired = 1;
				if (Interlocked.CompareExchange(ref m_locked, desired, expected) == expected)
					break;
			}

		}

		public void Release()
		{
			m_locked = 0;
		}
	}

	// 스핀락에 비해 너무나도 느리다.
	class EventLock
	{
		// 커널단 bool이라 생각하면 편하다.
		AutoResetEvent m_available = new AutoResetEvent(true);

		public void Acquire()
		{
			m_available.WaitOne();  // 입장 시도
			//m_available.Reset();	// bool = false;
			// 이 부분은 위 함수에 포함되어 있으니 생략해도된다.
		}

		public void Release()
		{
			m_available.Set();      // bool = true;
		}
	}

	class ManualLock
    {
		ManualResetEvent m_available = new ManualResetEvent(true);

		public void Acquire()
		{
			m_available.WaitOne();	// 입장 시도
			m_available.Reset();	// 문 닫는다.
		}

		public void Release()
		{
			m_available.Set();      // bool = true;
		}
	}

	// ReaderWriteLock 구현 연습
    // 재귀적 락을 적용할지 (NO)
    // 스핀락 정책 (5000번 -> Yield)
    class LockWithNoRecursion
    {
		const int EMPTY_FLAG = 0x00000000;
		const int WRITE_MASK = 0x7FFF0000;
		const int READ_MASK	 = 0x0000FFFF;
		const int MAX_SPIN_COUNT = 5000;

		// [Unused(1)] [WriteThrteadId(15)] [ReadCount(16)]
		int m_flag;

		public void WriteLock()
        {
			// 아무도 WriteLock or ReadLock을 획득하고 있지 않을 때,
			// 경합해서 소유권을 얻는다.
			int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
			while(true)
            {
				for(int i = 0; i < MAX_SPIN_COUNT; i++)
                {
					// 시도를 해 성공하면 return
					if (Interlocked.CompareExchange(ref m_flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
						return;
					// 의사 코드
					//if (m_flag == EMPTY_FLAG)
					//	m_flag = desired;
				}

				Thread.Yield();
            }
        }

		public void WriteUnLock()
        {
			Interlocked.Exchange(ref m_flag, EMPTY_FLAG);
        }

		public void ReadLock()
        {
			// 아무도 WriteLock을 획득하고 있지 않으면, ReadCount를 1 늘린다.
			while(true)
            {
				for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
					int expected = (m_flag & WRITE_MASK);
					if (Interlocked.CompareExchange(ref m_flag, expected + 1, expected) == expected)
						return;
					// 의사 코드
					//if((m_flag & WRITE_MASK) == 0)
                    //{
					//	m_flag = m_flag + 1;
					//	return;
                    //}
                }

				Thread.Yield();
			}
        }

		public void ReadUnLock()
        {
			Interlocked.Decrement(ref m_flag);
        }
	}

	// 재귀적 락을 적용할지 (Yes) WriteLock->WriteLock OK, WriteLock->ReadLock OK
	// 락 순서 주의
	// WriteLock->ReadLock 순서로 락 했으면 언락은 ReadLock->WriteLock으로 해야한다.
	// 스핀락 정책 (5000번 -> Yield)
	class LockWithRecursion
	{
		const int EMPTY_FLAG = 0x00000000;
		const int WRITE_MASK = 0x7FFF0000;
		const int READ_MASK = 0x0000FFFF;
		const int MAX_SPIN_COUNT = 5000;

		// [Unused(1)] [WriteThrteadId(15)] [ReadCount(16)]
		int m_flag;
		int m_writeCount = 0;

		public void WriteLock()
		{
			// 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
			int lockThreadId = (m_flag & WRITE_MASK) >> 16;
			if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
			{
				m_writeCount++;
				return;
			}

			// 아무도 WriteLock or ReadLock을 획득하고 있지 않을 때,
			// 경합해서 소유권을 얻는다.
			int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
			while (true)
			{
				for (int i = 0; i < MAX_SPIN_COUNT; i++)
				{
					// 시도를 해 성공하면 return
					if (Interlocked.CompareExchange(ref m_flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
					{
						m_writeCount = 1;
						return;
					}
					// 의사 코드
					//if (m_flag == EMPTY_FLAG)
					//	m_flag = desired;
				}

				Thread.Yield();
			}
		}

		public void WriteUnLock()
		{
			int lockCount = --m_writeCount;
			if (lockCount == 0)
				Interlocked.Exchange(ref m_flag, EMPTY_FLAG);
		}

		public void ReadLock()
		{
			// 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
			int lockThreadId = (m_flag * WRITE_MASK) >> 16;
			if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
			{
				Interlocked.Increment(ref m_flag);
				return;
			}

			// 아무도 WriteLock을 획득하고 있지 않으면, ReadCount를 1 늘린다.
			while (true)
			{
				for (int i = 0; i < MAX_SPIN_COUNT; i++)
				{
					int expected = (m_flag & WRITE_MASK);
					if (Interlocked.CompareExchange(ref m_flag, expected + 1, expected) == expected)
						return;
					// 의사 코드
					//if((m_flag & WRITE_MASK) == 0)
					//{
					//	m_flag = m_flag + 1;
					//	return;
					//}
				}

				Thread.Yield();
			}
		}

		public void ReadUnLock()
		{
			Interlocked.Decrement(ref m_flag);
		}
	}
}