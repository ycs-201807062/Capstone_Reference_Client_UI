using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Protocol;

using ReceiveResult = System.Collections.Generic.KeyValuePair<byte, object?>;

namespace ServerSystem
{
	public partial class UserClient
	{

		// Receive로 일어나기 위한 코드
		private partial void WakeUp()
		{
			Console.WriteLine(studentID + "\t: Receive event is occurred.");

			Console.WriteLine(studentID + "\t: Semaphore Attempt");
			// 세마포어 획득을 시도
			if (!semaphore.WaitOne(10))
				return;

			Console.WriteLine(studentID + "\t: Semaphore is assigned.---------------------------");
			// Receive 큐가 빌때까지 반복
			while (!IsEmpty())
			{
				result = Receive();
				switch (result.Key)
				{
					case DataType.MESSAGE:
						MessageProcess(result);
						break;
					case DataType.LOGIN:
						LoginProcess(result);
						break;
					case DataType.LOGOUT:
						LogoutProcess(result);
						break;
					case DataType.USER:
						UserProcess(result);
						break;
					case DataType.GAME_START:
						GameProcess(result);
						break;
				}
			}

			// 세마포어 반환
			Console.WriteLine(studentID + "\t: Semaphore is returned.---------------------------\n\n");
			semaphore.Release();
		}

		private void LoginProcess(ReceiveResult result)
		{
			LoginProtocol.LOGIN? login = result.Value as LoginProtocol.LOGIN;

			// 빈 객체라면 종료
			if (login == null)
				return;

			Console.WriteLine(login.studentID + "\t: Attempt Login");

			// 이미 로그인 상태라면 종료
			if (this.isLogin)
			{
				Console.WriteLine("already logged in");
				return;
			}
			// 컨테이너 등록 실패 시
			if(!ClientContainer.Instance.AddUser(this, ref login))
			{
				Console.WriteLine("Login Failed");
				return;
			}

			ClientContainer.Instance.SendUserList(this);
			Console.WriteLine("Login Success");
		}

		private void LogoutProcess(ReceiveResult result)
		{
			LogoutProtocol.LOGOUT? logout = result.Value as LogoutProtocol.LOGOUT;

			// 빈 객체라면 종료
			if (logout == null)
				return;

			Console.WriteLine(studentID + "\t: Attempt Logout");

			// 로그인 상태가 아니라면 종료
			if (!isLogin)
			{
				Console.WriteLine("\t: not logged in");
				return;
			}

			// seqNo가 일치하지 않다면 종료
			if (logout.seqNo != seqNo)
			{
				Console.WriteLine(studentID + "\t: seqCode different");
				return;
			}

			this.Stop();
		}

		private void UserProcess(ReceiveResult result)
		{
			UserProtocol.USER? userInfo = result.Value as UserProtocol.USER;

			// 빈 객체라면 종료
			if (userInfo == null)
				return;

			// 내가 로그인 상태가 아니라면 종료
			if (!isLogin)
			{
				Console.WriteLine("\t: not logged in");
				return;
			}

			// 유저 정보 변경 (시퀀스 번호와 나의 정보가 같다면)
			if(userInfo.seqNo == seqNo && userInfo.studentID == studentID)
			{
				Console.WriteLine(studentID + "\t: Attempt userdata modify");
				modifyUserInfo(userInfo);
			}
			
			// 삭제 요청인 경우
			if(-1 == userInfo.seqNo)
			{
				// 해당 유저를 삭제시도 한다.
				ClientContainer.Instance.RemoveUser(this,userInfo.studentID);
			}

			// 유저 데이터 요청
			Console.WriteLine(studentID + "\t: Attempt get userdata");

		}

		private void MessageProcess(ReceiveResult result)
		{
			MessageProtocol.MESSAGE? message = result.Value as MessageProtocol.MESSAGE;
			
			// 빈 객체라면 종료
			if (message == null)
				return;

			Console.WriteLine(message.studentID + "\t: Attempt Message Send");

			// 로그인 상태가 아니라면 종료
			if (!isLogin)
			{
				Console.WriteLine("\t: not logged in");
				return;
			}

			// 자신의 seqNo가 일치하지 않다면 종료
			if (!message.check(studentID,seqNo))
			{
				Console.WriteLine(studentID + "\t: studentID or seqCode different");
				return;
			}

			Console.WriteLine(studentID + "\t: Sending Message");

			if (0 == message.targetID)
				ClientContainer.Instance.SendMessage(ref message);
			else
				ClientContainer.Instance.SendWhisperMessage(ref message);
		}

		private void GameProcess(ReceiveResult result)
		{
			ClientContainer.Instance.StartGame(this);
		}

	}
}
