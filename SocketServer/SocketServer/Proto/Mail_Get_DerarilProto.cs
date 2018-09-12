using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 获取邮件详情
/// </summary>
public struct Mail_Get_DerarilProto : IProto
{
    public ushort ProtoID { get { return 1005; } }
    public bool IsSuccess;
    public string Name;
    public ushort ErrorCode;

    /// <summary>
    /// 将结构体转换成字节数组
    /// </summary>
    /// <returns></returns>
    public byte[] ToArray()
    {
        using (MMO_MemoryStream ms = new MMO_MemoryStream())
        {
            ms.WriteUShort(ProtoID);//协议类型
            ms.WriteBool(IsSuccess);
            if (IsSuccess)
            {
                ms.WriteUTF8String(Name);
            }
            else
            {
                ms.WriteUShort(ErrorCode);
            }            
            return ms.ToArray();
        }
    }
    /// <summary>
    /// 根据字节数组转换成结构体
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static Mail_Get_DerarilProto GetProto(byte[] buffer)
    {
        Mail_Get_DerarilProto proto = new Mail_Get_DerarilProto();
        using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
        {
            proto.IsSuccess = ms.ReadBool();
            if (proto.IsSuccess)
            {
                proto.Name = ms.ReadUTF8String();
            }
            else
            {
                proto.ErrorCode = ms.ReadUShort();
            }            
        }
        return proto;
    }
}
