using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer
{
    public class ProtoCodeDef
    {
        //缺少MailProto
        /// <summary>
        /// 发送邮件
        /// </summary>
        public const ushort Mail = 1001;
        /// <summary>
        /// 测试协议
        /// </summary>
        public const ushort Test = 1004;
        /// <summary>
        /// 获取邮件详情
        /// </summary>
        public const ushort Mail_Get_Detail = 1005;


        /// <summary>
        /// 服务器返回邮件列表
        /// uu Mail_Get_list
        /// </summary>
        public const ushort Mail_Get_listProto = 1006;
        /// <summary>
        /// 请求邮件列表
        /// </summary>
        public const ushort Mail_Requet_ListProto = 1007;


        /// <summary>
        /// 服务器返回道具详情
        /// </summary>
        public const ushort Mail_Ret_ListProto = 2001;
        /// <summary>
        /// 服务器返回道具列表
        /// </summary>
        public const ushort Mail_Ret_list = 1008;

    }
}
