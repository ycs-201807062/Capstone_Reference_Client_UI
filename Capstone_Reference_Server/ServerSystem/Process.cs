using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Protocol;

using ReceiveResult = System.Collections.Generic.KeyValuePair<byte, object?>;

namespace ServerSystem
{
	static public class Process
	{
		public static void Login(UserClient user, ReceiveResult result)
		{
			LoginProtocol.LOGIN? login = result.Value as LoginProtocol.LOGIN;

			// 빈 객체라면 종료
			if (login == null)
				return;

			Console.WriteLine(login.id + "\t: Attempt Login");

			// 이미 로그인 상태라면 종료
			if (user.isLogin)
			{
				Console.WriteLine("already logged in");
				return;
			}

			Console.WriteLine("Login Success");


		}

		public static void Logout(UserClient user, ReceiveResult result)
		{
			LogoutProtocol.LOGOUT? logout = result.Value as LogoutProtocol.LOGOUT;

			// 빈 객체라면 종료
			if (logout == null)
				return;

			// 로그인 상태가 아니라면 종료
			if (!user.isLogin)
			{
				Console.WriteLine("\t: not logged in");
				return;
			}
				
			Console.WriteLine(user.seqNo + "\t: Attempt Logout");


			// seqNo가 일치하지 않다면 종료
			/*
			if(logout.id != user.seqNo)
			{
				Console.WriteLine(user.seqNo + "\t: seqCode different");
				return;
			}
			*/

		}

		public static void User(UserClient user, ReceiveResult result)
		{
			UserProtocol.USER? userInfo = result.Value as UserProtocol.USER;

			// 빈 객체라면 종료
			if (userInfo == null)
				return;

			// 로그인 상태가 아니라면 종료
			if (!user.isLogin)
			{
				Console.WriteLine("\t: not logged in");
				return;
			}

			// 유저 정보 변경
			// if(userInfo.seqNo ==  user.seqNo && userInfo.studentId == user.studentId)
			Console.WriteLine(user.seqNo + "\t: Attempt userdata modify");

			Console.WriteLine(user.seqNo + "\t: Attempt get userdata");
		}

		public static void Message(UserClient user, ReceiveResult result)
		{
			MessageProtocol.MESSAGE? message = result.Value as MessageProtocol.MESSAGE;

			// 빈 객체라면 종료
			if (message == null)
				return;

			// 로그인 상태가 아니라면 종료
			if (!user.isLogin)
			{
				Console.WriteLine("\t: not logged in");
				return;
			}

			// seqNo가 일치하지 않다면 종료
			/*
			if (message.seqNo != user.seqNo)
			{
				Console.WriteLine(user.seqNo + "\t: seqCode different");
				return;
			}
			*/
		}

	}
}
