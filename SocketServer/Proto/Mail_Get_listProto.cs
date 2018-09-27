using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 服务器返回邮件列表
/// </summary>
public struct Mail_Get_listProto : IProto
{
    public ushort ProtoID { get { return 1006; } }

    public int Count;//数量
    public int MailID;//邮件编号
    public string MailName;//邮件编号

    /// <summary>
    /// 将结构体转换成字节数组
    /// </summary>
    /// <returns></returns>
    public byte[] ToArray()
    {
        using (MMO_MemoryStream ms = new MMO_MemoryStream())
        {
            ms.WriteUShort(ProtoID);//协议类型
            ms.WriteInt(Count);
            ms.WriteInt(MailID);
            ms.WriteUTF8String(MailName);
            return ms.ToArray();
        }
    }
    /// <summary>
    /// 根据字节数组转换成结构体
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static Mail_Get_listProto GetProto(byte[] buffer)
    {
        Mail_Get_listProto proto = new Mail_Get_listProto();
        using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
        {
            proto.Count = ms.ReadInt();            
            proto.MailID = ms.ReadInt();
            proto.MailName = ms.ReadUTF8String();
        }
        return proto;
    }
}
