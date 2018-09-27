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

        private const int mComPressLen = 200;
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
                        byte[] data = Unpacking(ref dataCache);
                        data = DeMakeData(data);


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
            byte[] sendBuffer = MakeData(data);

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

        #region 沾包拆包，加密解密
        /// <summary>
        /// 沾包，加密数据包
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] MakeData(byte[] data)
        {
            byte[] retBuffer = null;
            //1、是否压缩
            bool IsComPressLen = data.Length > mComPressLen ? true : false;
            if (IsComPressLen)
            {
                //压缩
                data = ZlibHelper.CompressBytes(data);
            }            
            //2、异或
            data = SecurityUtil.Xor(data);
            //3、crc校验
            ushort crc = CRC16.CalculateCrc16(data);
            using (MMO_MemoryStream ms = new MMO_MemoryStream())
            {
                //1、包头(+3==Crc（2字节）+IsComPressLen（2字节）)
                ms.WriteUShort((ushort)(data.Length + 3));
                ms.WriteBool(IsComPressLen);
                ms.WriteUShort(crc);
                ms.Write(data, 0, data.Length);
                retBuffer = ms.ToArray();
            }
            return retBuffer;
        }
        /// <summary>
        /// 解析加密后的数据包得到原始数据包
        /// </summary>
        /// <param name="dataCache"></param>
        public static byte[] DeMakeData(byte[] buffer)
        {
            // 拆包得到数据包
            byte[] bufferNew = new byte[buffer.Length - 3];
            bool IsComPressLen = false;
            ushort crc = 0;
            using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
            {
                //是否压缩
                IsComPressLen = ms.ReadBool();
                crc = ms.ReadUShort();
                ms.Read(bufferNew, 0, bufferNew.Length);
            }
            //1、crc校验
            ushort newCrc = CRC16.CalculateCrc16(bufferNew);
            if (newCrc == crc)
            {
                //2、异或后的原始数据包
                bufferNew = SecurityUtil.Xor(bufferNew);
                if (IsComPressLen)
                {
                    //3、解压缩 bufferNew是真实数据
                    bufferNew = ZlibHelper.deCompressBytes(bufferNew);
                }
            }
            return bufferNew;
        }

        /// <summary>
        /// 拆包   ushout + 加密后数据包
        /// </summary>
        /// <param name="dataCache"></param>
        /// <returns></returns>
        public byte[] Unpacking(ref List<byte> dataCache)
        {
            //2个字节 构成一个ushort长度 不能构成一个完整的消息
            if (dataCache.Count < 2)
                return null;
            //throw new Exception("数据缓存长度不足2 不能构成一个完整的消息");
            byte[] test = dataCache.ToArray();
            using (MemoryStream ms = new MemoryStream(dataCache.ToArray()))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    // 1111 111 1
                    //ReadUInt16（）读取2个字符并且将流的当前位置提升2(删除2个字符)
                    int length = br.ReadUInt16();
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