using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 服务器返回道具详情
/// </summary>
public struct Mail_Ret_ListProto : IProto
{
    public ushort ProtoID { get { return 2001; } }

    public int ItemCount;//道具数量
    //数组如果位null会报错，需要校验
    public List<ItemInfo> ItemName;//道具集合

    public struct ItemInfo
    {
        public int ItemId;//道具id
        public string ItemName;//道具名字
    }

    public byte[] ToArray()
    {
        using (MMO_MemoryStream ms = new MMO_MemoryStream())
        {
            ms.WriteInt(ItemCount);
            for(int i = 0; i < ItemCount; i++)
            {
                ms.WriteInt(ItemName[i].ItemId);
                ms.WriteUTF8String(ItemName[i].ItemName);
            }
            return ms.ToArray();
        }
    }

    public static Mail_Ret_ListProto GetRet_Item(byte[] buffer)
    {
        Mail_Ret_ListProto proto = new Mail_Ret_ListProto();
        using (MMO_MemoryStream ms=new MMO_MemoryStream(buffer))
        {
            proto.ItemCount = ms.ReadInt();
            proto.ItemName = new List<ItemInfo>();
            for(int i = 0; i < proto.ItemCount; i++)
            {
                ItemInfo _Item = new ItemInfo();
                _Item.ItemId = ms.ReadInt();
                _Item.ItemName = ms.ReadUTF8String();
                proto.ItemName.Add(_Item);
            }
        }
        return proto;
    }
}
