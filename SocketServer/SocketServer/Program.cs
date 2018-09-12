using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace SocketServer
{
    class Program
    {
        private static string mServerIP = "192.168.31.236";
        private static int mPoint = 5050;
        private static Socket mServerSocaket;

        static void Main(string[] args)
        {
            mServerSocaket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mServerSocaket.Bind(new IPEndPoint(IPAddress.Parse(mServerIP), mPoint));


            mServerSocaket.Listen(10);
            //mServerSocaket.LocalEndPoint.ToString()客户端ip
            Console.WriteLine("启动成功{0}已经监听", mServerSocaket.LocalEndPoint.ToString());

            //新建线程，监听函数的回调
            Thread mThread = new Thread(ListenClinetCallBack);
            //启动线程
            mThread.Start();
            MailDBModel._instance.Init();



            Console.ReadLine();
        }

        private static void ListenClinetCallBack()
        {
            while (true)
            {
                //接收客户端请求
                Socket clinSocket = mServerSocaket.Accept();
                //clinSocket.RemoteEndPoint.ToString()连接的客户端的ip
                Console.WriteLine("客户端{0}已经连接", clinSocket.RemoteEndPoint.ToString());

                Role role = new Role();
                ClicentSocket _ClicetSocket = new ClicentSocket(clinSocket, role);
                RoleMgr._instance.AllRole.Add(role);
            }
        }
    }
}
