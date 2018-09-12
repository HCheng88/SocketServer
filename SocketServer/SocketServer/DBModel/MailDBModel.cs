using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer
{
    public class MailDBModel : Singleton<MailDBModel>
    {
        /// <summary>
        /// 初始化监听消息
        /// </summary>
        public void Init()
        {
            EventDispatchet._instance.AddEventListener(ProtoCodeDef.Mail_Requet_ListProto, OnRequestList);
        }

        private void OnRequestList(Role role, byte[] buffer)
        {
            Console.WriteLine("客户端请求邮件列表");
            Mail_Get_listProto proto = new Mail_Get_listProto();
            proto.Count = 30;
            proto.MailID = 1001;
            proto.MailName = "金币大礼包";
            role.clicentSocket.SendMsg(proto.ToArray());
        }
    }
}
