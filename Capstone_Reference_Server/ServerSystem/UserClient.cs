using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using ServerToClient;
using Protocol;
using ReceiveResult = System.Collections.Generic.KeyValuePair<byte, object?>;
using static Protocol.LoginProtocol;

namespace ServerSystem
{
	public partial class UserClient : Client
	{
		public int studentID;
		public int seqNo;
		public string name ="";
		public string nickName = "";
		public bool isLogin;
		// 결과값을 임시로 저장하는 객체 변수
		private ReceiveResult result;

		// 객체에 접근하는 객체의 수를 제한(Send)
		private readonly Semaphore semaphore;

		public UserClient(TcpClient tcpClient) :
			base(tcpClient)
		{
			studentID = 0;
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

		private partial void WakeUp();

		public void Login(LoginProtocol.LOGIN login)
		{
			this.seqNo = login.seqNo;
			this.studentID = login.studentID;
			this.name = login.name;
			this.nickName = login.nickName;
			this.isLogin = true;
		}

		public void logout()
		{
			this.seqNo = 0;
			this.studentID = 0;
			this.name = "";
			this.nickName = "";
			this.isLogin = false;
		}

		public void modifyUserInfo(UserProtocol.USER userInfo)
		{
			this.nickName = userInfo.nickname;
		}

		// 어떠한 이유에든 수신이 종료되면 발생할 메소드
		private void Stop()
		{
			Console.WriteLine(studentID + "\t: Stop Signal Generation");

			if(0 != studentID)
			{
				ClientContainer.Instance.RemoveUser(this);
			}

			seqNo = 0;
			studentID = 0;
			isLogin = false;
			
			// 수신 이벤트를 해제
			receiveEvent -= WakeUp;

			// 수신 종료 이벤트를 해제
			stopEvent -= Stop;

			StopReceive();

			Console.WriteLine("UserDeleted\n");
		}
	}
}
