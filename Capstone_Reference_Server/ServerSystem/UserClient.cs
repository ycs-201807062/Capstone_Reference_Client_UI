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
		public int studentId;
		public int seqNo;
		public bool isLogin;
		// 결과값을 임시로 저장하는 객체 변수
		private ReceiveResult result;

		// 객체에 접근하는 객체의 수를 제한(Send)
		private readonly Semaphore semaphore;

		public UserClient(TcpClient tcpClient) :
			base(tcpClient)
		{
			studentId = 0;
			seqNo = 0;
			isLogin = false;

			// 수신 시 이벤트를 등록
			receiveEvent += WakeUp;

			// 수신 종료 시 이벤트를 등록
			stopEvent += Stop;
			
			// 통신 시작
			Start();

			semaphore = new(1, 1);
		}

		// Receive로 일어나기 위한 코드
		private void WakeUp()
		{
			Console.WriteLine(studentId + "\t: Receive event is occurred.");

			Console.WriteLine(studentId + "\t: Semaphore Attempt\n");
			// 세마포어 획득을 시도
			if (!semaphore.WaitOne(10))
				return;

			Console.WriteLine(studentId + "\t: Semaphore is assigned.");
			// Receive 큐가 빌때까지 반복
			while (!IsEmpty())
			{
				result = Receive();
				switch (result.Key)
				{
					case DataType.MESSAGE:
						Process.Message(this, result);
						break;
					case DataType.LOGIN:
						Process.Login(this, result);
						break;
					case DataType.LOGOUT:
						Process.Logout(this, result);
						break;
					case DataType.USER:
						Process.User(this, result);
						break;
				}
			}

			// 세마포어 반환
			Console.WriteLine(studentId + "\t: Semaphore is returned.\n");
			semaphore.Release();
		}
		
		// 어떠한 이유에든 수신이 종료되면 발생할 메소드
		private void Stop()
		{
			Console.WriteLine(studentId + "\t: Stop Signal Generation");
			
			// 수신 이벤트를 해제
			receiveEvent -= WakeUp;

			// 수신 종료 이벤트를 해제
			stopEvent -= Stop;
			
			Console.WriteLine("UserDeleted\n");
		}
	}
}
