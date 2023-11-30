using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
	// Binary Heap 구조
	// https://chanhuiseok.github.io/posts/ds-4/
	public class PriorityQueue<T> where T : IComparable<T>
	{
		List<T> m_heap = new List<T>();

		public int Count { get { return m_heap.Count; } }

		// O(logN)
		public void Push(T data)
		{
			// 힙의 맨 끝에 새로운 데이터를 삽입한다
			m_heap.Add(data);

			int now = m_heap.Count - 1;
			// 맨 끝에서부터 위로 올라갈 수 있을만큼 위로 올라가야한다.
			while (now > 0)
			{
				int next = (now - 1) / 2;
				if (m_heap[now].CompareTo(m_heap[next]) < 0)
					break; // 종료

				// 성공 : 위에 있는 노드보다 값이 크다. -> 두 값을 교체한다
				T temp = m_heap[now];
				m_heap[now] = m_heap[next];
				m_heap[next] = temp;

				// 검사 위치를 이동한다
				now = next;
			}
		}

		// O(logN)
		public T Pop()
		{
			// 반환할 데이터를 따로 저장
			T ret = m_heap[0];

			// 맨 밑의(마지막) 데이터를 맨 위(루트)로 올린다.
			int lastIndex = m_heap.Count - 1;
			m_heap[0] = m_heap[lastIndex];
			m_heap.RemoveAt(lastIndex);
			lastIndex--;

			// 역으로 내려가기 시작
			int now = 0;
			while (true)
			{
				int left = 2 * now + 1;
				int right = 2 * now + 2;

				int next = now;
				// 왼쪽값이 현재값보다 크면, 왼쪽으로 이동
				if (left <= lastIndex && m_heap[next].CompareTo(m_heap[left]) < 0)
					next = left;
				// 오른값이 현재값(왼쪽 이동 포함)보다 크면, 오른쪽으로 이동
				if (right <= lastIndex && m_heap[next].CompareTo(m_heap[right]) < 0)
					next = right;

				// 왼쪽/오른쪽 모두 현재값보다 작으면 종료
				if (next == now)
					break;

				// 두 값을 교체한다
				T temp = m_heap[now];
				m_heap[now] = m_heap[next];
				m_heap[next] = temp;
				// 검사 위치를 이동한다
				now = next;
			}

			return ret;
		}

		public T Peek()
		{
			if (m_heap.Count == 0)
				return default(T);
			return m_heap[0];
		}
	}
}
