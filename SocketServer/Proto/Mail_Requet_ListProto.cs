using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 请求邮件列表
/// </summary>
public struct Mail_Requet_ListProto : IProto
{
    public ushort ProtoID { get { return 1007; } }


    public byte[] ToArray()
    {
        using (MMO_MemoryStream ms = new MMO_MemoryStream())
        {
            ms.WriteUShort(ProtoID);//协议类型            
            return ms.ToArray();
        }
    }
    /// <summary>
    /// 根据字节数组转换成结构体
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static Mail_Requet_ListProto GetProto(byte[] buffer)
    {
        Mail_Requet_ListProto proto = new Mail_Requet_ListProto();
        using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
        {
           
        }
        return proto;
    }
}

