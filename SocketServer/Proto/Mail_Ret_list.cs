using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 从服务器返回道具列表
/// </summary>
public class Mail_Ret_list : IProto
{
    public ushort ProtoID { get { return 1008; } }

    public int ItemCount;//数量

    public List<string> ItemNameList;//道具名称

    /// <summary>
    /// 将结构体转换成字节数组
    /// </summary>
    /// <returns></returns>
    public byte[] ToArray()
    {
        using (MMO_MemoryStream ms = new MMO_MemoryStream())
        {
            ms.WriteUShort(ProtoID);//协议类型
            ms.WriteInt(ItemCount);
            for (int i=0;i<ItemCount;i++)
            {
                ms.WriteUTF8String(ItemNameList[i]);
            }
            return ms.ToArray();
        }
    }
    /// <summary>
    /// 根据字节数组转换成结构体
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static Mail_Ret_list GetProto(byte[] buffer)
    {
        Mail_Ret_list proto = new Mail_Ret_list();        
        using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
        {
            proto.ItemCount = ms.ReadInt();
            proto.ItemNameList = new List<string>();
            for (int i = 0; i < proto.ItemCount; i++)
            {
                string itemName = ms.ReadUTF8String();
                proto.ItemNameList.Add(itemName);
            }
        }
        return proto;
    }
}
