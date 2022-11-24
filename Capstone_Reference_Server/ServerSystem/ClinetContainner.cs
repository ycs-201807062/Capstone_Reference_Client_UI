using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protocol;

namespace ServerSystem
{
	public class ClinetContainner
	{
		public void AddUser(UserClient user, UserProtocol.USER userInfo)
		{

			// userInfo.seqNo = 

			// 유저의 시퀀스 번호를 지정
			user.studentId = 10;
			userInfo.userCode = user.studentId;

			user.Send(Generater.Generate(userInfo));
		}
	}
}
