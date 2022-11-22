using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using ServerCommunicater = ServerToClient.Server;
using ServerToClient;

namespace ServerSystem
{
	public class ServerSystem
	{
		public ServerSystem()
		{
			Console.WriteLine("MyMate Server Start");

			// 서버 생성
			ServerCommunicater server = ServerCommunicater.Instance;
			server.clientAccept = AccpetRun;


			Console.WriteLine("Server ip : " + Default.Network.Address);
		}
		static public void AccpetRun(Client client)
		{
			Console.WriteLine("Attempt\t: 클라이언트 접근");
			if (client.socket != null)
				Console.WriteLine("포트 번호 : " + client.socket.RemoteEndPoint + "\n");

			// 새로운 유저 생성
			new UserClient(client.tcpClient);
		}
	}
}
