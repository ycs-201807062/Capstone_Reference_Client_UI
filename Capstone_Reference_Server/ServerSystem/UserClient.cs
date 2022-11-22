using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using ServerToClient;
using Protocol;
using ReceiveResult = System.Collections.Generic.KeyValuePair<byte, object?>;

namespace ServerSystem
{
	public class UserClient : Client
	{
		public int userCode = 0;
		// 결과값을 임시로 저장하는 객체 변수
		private ReceiveResult result;

		// 객체에 접근하는 객체의 수를 제한(Send)
		private readonly Semaphore semaphore;

		public UserClient(TcpClient tcpClient) :
			base(tcpClient)
		{
			// 수신 시 이벤트를 등록
			ReceiveEvent += WakeUp;

			// 통신 시작
			Start();

			semaphore = new(1, 1);
		}

		// Receive로 일어나기 위한 코드
		private void WakeUp()
		{
			Console.WriteLine(userCode + "\t: Receive event is occurred.");
			// 연결 상태가 아니라면

			/*
			if (!tcpClient.Connected)
			{
				// 연결 종료
				//Disconnection();
				return;
			}
			
			// 소켓이 연결 상태가 아니라면
			if (this.Stream != null)
			{
				if (this.Stream.Socket.Connected == false)
				{
					Disconnection();
					return;
				}
			}
			// 현재 Receive가 실행중이 아니라면
			if(!this.receiveRun)
			{
				Disconnection();
			}
			*/

			Console.WriteLine(userCode + "\t: Semaphore Attempt");
			// 세마포어 획득을 시도
			if (!semaphore.WaitOne(10))
				return;

			Console.WriteLine(userCode + "\t: Semaphore is assigned.");

			// Receive 큐가 빌때까지 반복
			while (!IsEmpty())
			{
				
			}

			// 세마포어 반환
			Console.WriteLine(userCode + "\t: Semaphore is returned.\n");
			semaphore.Release();
		}
	}
}
