using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketServer
{
    /// <summary>
    /// 客户端连接对象
    /// </summary>
    public class ClicentSocket
    {
        private Socket mClicentSocket;
        private Role mRole;
        private Thread mReceiveThread;
        #region 接收数据变量
        //接收数据缓冲区
        private byte[] ReceiveBuffer = new byte[10240];
        /// <summary>
        /// 一旦接收到数据 就存到缓存区里面
        /// </summary>
        private List<byte> dataCache = new List<byte>();
        #endregion
        #region 发送数据变量
        //先进先出，用来存放要发送的消息
        private Queue<byte[]> mSendQueue = new Queue<byte[]>();
        //检查队列委托
        private Action mSendQueueFunction;
        #endregion
        public ClicentSocket(Socket clicentSocket, Role role)
        {
            this.mClicentSocket = clicentSocket;
            this.mRole = role;
            role.clicentSocket = this;

            //启动线程接收数据
            mReceiveThread = new Thread(ReceiveMgs);
            mReceiveThread.Start();
            mSendQueueFunction += mSendQueuFunctionCallBack;

            //临时发送测试
            byte[] data = Creadata("欢迎来到我的世界！");
            this.SendMsg(data);
        }
        /// <summary>
        /// 内存流转字节
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public byte[] Creadata(string str)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(str);
                    return ms.ToArray();
                }
            }
        }
        #region 接收数据，内存流转字节，保存，拆包解析
        /// <summary>
        /// 接收数据
        /// </summary>
        private void ReceiveMgs()
        {
            //异步接收数据
            //1、接收到的数据；2、数据从那个位置开始；3、数据长度；4、发送和接收行为。；5、回调函数；6回调函数参数；
            mClicentSocket.BeginReceive(ReceiveBuffer, 0, ReceiveBuffer.Length, SocketFlags.None, ReceiveCallBack, mClicentSocket);
        }
        /// <summary>
        /// 异步接收数据回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                //结束挂起的异步接收，返回接收到的数据长度
                int lenght = mClicentSocket.EndReceive(ar);

                //如果接收的数据大于0表示接受到数据
                if (lenght > 0)
                {
                    byte[] receiveData = new byte[lenght];
                    //将接收到的数据拷贝到receiveData中
                    Buffer.BlockCopy(ReceiveBuffer, 0, receiveData, 0, lenght);
                    //存入数组（缓存）
                    dataCache.AddRange(receiveData);
                    //接收的数据
                    //如果缓存中存在数据全部拆包拆出来
                    while (dataCache.Count > 0)
                    {
                        //拆包拆出来的数据
                        byte[] data = DecodePacket(ref dataCache);

                        //协议编号
                        ushort protoCode = 0;
                        //保存，除去协议ushort的真正数据
                        byte[] protoConten = new byte[data.Length - 2];

                        using (MMO_MemoryStream ms = new MMO_MemoryStream(data))
                        {
                            protoCode = ms.ReadUShort();
                            ms.Read(protoConten, 0, protoConten.Length);
                        }
                        EventDispatchet._instance.Dispatch(protoCode, mRole, protoConten);
                    }
                    //接收完数据启动异步接收，等待下一次接收
                    ReceiveMgs();
                }
                else
                {
                    //如果接收数据长度是0说明客户端断开连接
                    Console.WriteLine("客户端{0}已经断开连接", mClicentSocket.RemoteEndPoint.ToString());
                    RoleMgr._instance.AllRole.Remove(mRole);
                }
            }
            catch (Exception e)
            {
                //如果接收数据长度是0说明客户端断开连接
                Console.WriteLine("客户端{0}已经断开连接", mClicentSocket.RemoteEndPoint.ToString());
                Console.WriteLine("客户端{0}已经断开连接", e.Message);
                RoleMgr._instance.AllRole.Remove(mRole);
            }
        }
        #endregion
        //=============================================================
        #region 发送数据


        /// <summary>
        /// 检查队列回调
        /// </summary>
        private void mSendQueuFunctionCallBack()
        {
            lock (mSendQueue)
            {
                //如果队列中有数据包 则发送数据包
                if (mSendQueue.Count > 0)
                {
                    //发送数据
                    Send(mSendQueue.Dequeue());
                }
            }
        }
        /// <summary>
        /// 将要发送的数据保存到队列中，启动委托
        /// </summary>
        /// <param name="msg"></param>
        public void SendMsg(byte[] data)
        {
            //byte[] data = Encoding.UTF8.GetBytes(msg);
            byte[] sendBuffer = EncodePacket(data);

            lock (mSendQueue)
            {
                //将发送的消息存在队列中
                mSendQueue.Enqueue(sendBuffer);
                //执行委托
                //mSendQueueFunction.BeginInvoke(null, null);
                if (mSendQueueFunction != null)
                {
                    mSendQueuFunctionCallBack();
                }
            }
        }
        /// <summary>
        /// 往客户端发送消息
        /// </summary>
        /// <param name="buffer"></param>
        public void Send(byte[] buffer)
        {
            mClicentSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallBack, mClicentSocket);
        }
        /// <summary>
        /// 往客户端发送消息回调
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallBack(IAsyncResult ar)
        {
            mClicentSocket.EndSend(ar);
            //如果队列中存在消息继续发送
            mSendQueuFunctionCallBack();
        }

        #endregion
        #region 沾包拆包

        /// <summary>
        /// 构造数据包 ： 包头 + 包尾
        /// </summary>
        /// <returns></returns>
        public byte[] EncodePacket(byte[] data)
        {
            //内存流对象
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    //先写入长度
                    bw.Write(data.Length);
                    //再写入数据
                    bw.Write(data);

                    byte[] byteArray = new byte[(int)ms.Length];
                    Buffer.BlockCopy(ms.GetBuffer(), 0, byteArray, 0, (int)ms.Length);

                    return byteArray;
                }
            }
        }
        /// <summary>
        /// 解析消息体 从缓存里取出一个一个完整的数据包 
        /// </summary>
        /// <returns></returns>
        public static byte[] DecodePacket(ref List<byte> dataCache)
        {
            //四个字节 构成一个int长度 不能构成一个完整的消息
            if (dataCache.Count < 4)
                return null;
            //throw new Exception("数据缓存长度不足4 不能构成一个完整的消息");

            using (MemoryStream ms = new MemoryStream(dataCache.ToArray()))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    // 1111 111 1
                    //ReadInt32（）读取四个字符并且将流的当前位置提升4
                    int length = br.ReadInt32();
                    int dataRemainLength = (int)(ms.Length - ms.Position);
                    //数据长度不够包头约定的长度 不能构成一个完整的消息
                    if (length > dataRemainLength)
                        return null;
                    //throw new Exception("数据长度不够包头约定的长度 不能构成一个完整的消息");

                    byte[] data = br.ReadBytes(length);
                    //更新一下数据缓存
                    dataCache.Clear();
                    dataCache.AddRange(br.ReadBytes(dataRemainLength));
                    return data;
                }
            }
        }
        #endregion
    }
}