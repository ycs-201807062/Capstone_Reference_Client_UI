using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protocol;
using static Protocol.UserProtocol;


// 프로그램 시작 전 setOwner를 통해 설정을 진행한 후 돌아가야함
// seqNo는 해당 사람이 맞는지 확인하는 값(유사 비밀번호)
namespace ServerSystem
{
	public class ClientContainer
	{
		private Dictionary<int, UserClient> loginDict;

		private static UserClient? owner;

		private static int seq = 1;

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

		public static void SetOwner(UserClient client)
		{
			if (null == owner)
			{
				owner = client;
				Instance.loginDict.Add(-1, owner);
			}	
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

			// studentNo로 사람 구별
			loginDict.Add(login.studentID, user);

			// 로그인 데이터 전송
			user.Send(Generater.Generate(login));

			Console.WriteLine("ClientContainer\t : Student " + login.studentID + " add Successed");
			return true;
		}

		// 나 자신을 지움
		public void RemoveUser(UserClient user)
		{
			Console.WriteLine("ClientContainer\t : Student " + user.studentID + " attempt remove");
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
		}

		// owner가 상대방을 지움
		public void RemoveUser(UserClient user, int studentID)
		{
			Console.WriteLine("ClientContainer\t : Student " + user.studentID + " remove");
			if(user != owner)
			{
				Console.WriteLine("ClientContainer\t : No Permissions to Delete");
			}

			loginDict.Remove(studentID);

		}

		public int GetSeq()
		{
			Random ran = new ();
			// 오버플로우 상관 없음
			seq += ran.Next(-10000000,10000000);
			return seq;
		}

		public void SendMessage(ref MessageProtocol.MESSAGE msg)
		{
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
			if (null != target && null != sender)
			{
				target.Send(Generater.Generate(msg));
				sender.Send(Generater.Generate(msg));
			}
			// 상대방 또는 내가 등록이 안되어있다면
			else
			{
				if(sender != null)
				{
					msg.content = "없는 사용자 입니다.";
					sender.Send(Generater.Generate(msg));
				}
			}
			Console.WriteLine("ClientContainer\t : Student " + msg.studentID + " Whisper Message Send Success");
		}

	}
}
