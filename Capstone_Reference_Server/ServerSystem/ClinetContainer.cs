using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Protocol;
using static Protocol.UserProtocol;
using System.Security.Claims;


// 프로그램 시작 전 setOwner를 통해 설정을 진행한 후 돌아가야함
// seqNo는 해당 사람이 맞는지 확인하는 값(유사 비밀번호)
namespace ServerSystem
{
	public class ClientContainer
	{
		public Dictionary<int, UserClient> loginDict;

		private static UserClient? owner;

		private static int seq = 1;

		public bool blockSend = false;

		// 싱글톤 구현
		static private ClientContainer? instance;
		static public ClientContainer Instance
		{
			get
			{
				if (null == instance)
				{
					instance = new();
				}
				return instance;
			}
		}
		private ClientContainer()
		{
			loginDict = new Dictionary<int, UserClient>();
		}

		public void SetOwner(UserClient client)
		{
			if (null == owner)
			{
				Console.WriteLine("Important\t : OwnerLogined\t\t owner name\t: " + client.name);
				owner = client;
				Instance.loginDict.Add(-1, owner);
			}
		}
		public bool CheckOwner(UserClient client)
		{
			do
			{
				if (owner == null)
					break;
				if (-1 != client.studentID)
					break;
				if (client.seqNo != owner.seqNo)
					break;

				Console.WriteLine("ClientContainer\t : Permission verification complete");
				return true;
			} while (false);

			Console.WriteLine("ClientContainer\t : No Permissions");

			return false;
		}

		public bool AddUser(UserClient user, ref LoginProtocol.LOGIN login)
		{
			Console.WriteLine("ClientContainer\t : Student " + login.studentID + " add Attempt");

			// 이미 등록된 학번이라면 실패
			if (loginDict.ContainsKey(login.studentID))
			{
				Console.Write("ClientContainer\t : Student " + login.studentID + " add fail ");
				Console.WriteLine("alread Logined");
				return false;
			}
			// 시퀀스 번호를 받아 넣음
			login.seqNo = GetSeq();

			user.Login(login);

			if (-1 == login.studentID)
				SetOwner(user);
			else
			{
				loginDict.Add(login.studentID, user);
			}
			// 로그인 데이터 전송
			user.Send(Generater.Generate(login));

			Console.WriteLine("ClientContainer\t : Student " + login.studentID + " add Successed");

			Thread.Sleep(10);

			// seqNo를 0으로 초기화해서 전송한다.
			login.seqNo = 0;
			Console.WriteLine("id : " + login.studentID + "\tname : " + login.name);
			Send(Generater.Generate(new UserProtocol.USER(login.studentID, login.name, login.nickName)));
			return true;
		}

		// 나 자신을 지움
		public void RemoveUser(UserClient user)
		{
			Console.WriteLine("ClientContainer\t : Student " + user.studentID + " attempt remove");
			// 로그인 된 유저가 아님
			if (0 == user.seqNo)
				return;

			loginDict.TryGetValue(user.studentID, out UserClient? target);
			if (null == target)
				return;

			if (user.seqNo != target.seqNo)
				return;

			Console.WriteLine("ClientContainer\t : Authentication completed");

			loginDict.Remove(user.studentID);
			Console.WriteLine("ClientContainer\t : Student " + user.studentID + " remove Success");


			var usertemp = user.GetInfo();
			usertemp.seqNo = -1;
			Send(Generater.Generate(usertemp));
		}

		// owner가 상대방을 지움
		public void RemoveUser(UserClient user, int studentID)
		{
			Console.WriteLine("ClientContainer\t : Student " + studentID + " remove");

			if(!CheckOwner(user))
			{
				Console.WriteLine("ClientContainer\t : No Permissions to Delete");
				return;
			}
			loginDict.TryGetValue(studentID, out var value);
			if (null == value)
			{
				Console.WriteLine("ClientContainer\t : dosen't Exist");
				return;
			}

			value.Stop();
			loginDict.Remove(studentID);
			
			Send(Generater.Generate(new UserProtocol.USER(studentID, "", "", -1)));
		}

		public void KickUser(int studentID)
		{
			loginDict.TryGetValue(studentID, out var value);
			if (null == value)
			{
				Console.WriteLine("ClientContainer\t : dosen't Exist");
				return;
			}

			value.Stop();
			loginDict.Remove(studentID);

			Send(Generater.Generate(new UserProtocol.USER(studentID, "", "", -1)));
		}

		public int GetSeq()
		{
			Random ran = new ();
			// 오버플로우 상관 없음
			seq += ran.Next(-10000000,10000000);
			if (-1 == seq || 0 == seq)
				seq += ran.Next(2, 10000000);
			return seq;
		}

		public void SendMessage(ref MessageProtocol.MESSAGE msg)
		{
			// 현재 채팅이 막혀있다면
			if(blockSend)
			{
				// 보내는 사람이 교수가 아니라면
				// 추후 수정 필요
				if(-1 != msg.studentID)
				{
					return;
				}
			}
			var value = Generater.Generate(msg);
			foreach(var i in loginDict)
			{
				i.Value.Send(value);
			}
			Console.WriteLine("ClientContainer\t : Student " + msg.studentID + " Message Send Success");
		}

		public void SendWhisperMessage(ref MessageProtocol.MESSAGE msg)
		{
			loginDict.TryGetValue(msg.studentID, out UserClient? sender);
			loginDict.TryGetValue(msg.targetID, out UserClient? target);
			// 상대방과 내가 모두 존재한다면
			if (null != target)
			{
				target.Send(Generater.Generate(msg));
			}
			// 상대방 등록이 안되어있다면
			else
			{
				msg.content = "없는 사용자 입니다.";
			}
			if(null != sender)
				sender.Send(Generater.Generate(msg));

			Console.WriteLine("ClientContainer\t : Student " + msg.studentID + " Whisper Message Send Success");
		}

		// 모든 유저에게 정보 전달
		public void Send(List<byte> data)
		{
			foreach(var user in loginDict)
			{
				user.Value.Send(data);
			}
		}

		// 해당 유저한테 현재까지의 유저 정보를 전달.
		public void SendUserList(UserClient target)
		{
			foreach (var user in loginDict)
			{
				// Console.WriteLine("user : " + user.Value.studentID);
				// target.Send(Generater.Generate(user.Value.GetInfo()));
				/*
				UserProtocol.USER tempUser = new();
				tempUser.studentID = user.Value.studentID;
				tempUser.name = user.Value.name;
				*/

				target.Send(Generater.Generate(user.Value.GetInfo()));
			}
		}

		// 해당 유저에게 타겟의 정보를 전송한다.
		public void SendUserInfo(UserClient user, int targetId)
		{
			loginDict.TryGetValue(targetId, out UserClient? target);
			if (null == target)
				return;
			user.Send(Generater.Generate(target.GetInfo()));
		}

		public void StartGame(UserClient user)
		{
			Console.WriteLine("ClientContainer\t : Student " + user.studentID + " attempt game start");
			// 권한 확인
			if (CheckOwner(user))
			{
				// 시작 데이터 전송
				GameStartProtocol.GameStart gs = new GameStartProtocol.GameStart();
				gs.meanless = 96;
				Send(Generater.Generate(gs));
				return;
			}
			return;
		}

		public void PrtUsers()
		{
			Console.WriteLine("-----------------------UserList-----------------------");
			foreach (var user in loginDict)
			{
				Console.WriteLine("UserCode\t" + user.Value.studentID + "UserName\t" + user.Value.name);
			}
			Console.WriteLine("------------------------------------------------------");
		}
	}
}
