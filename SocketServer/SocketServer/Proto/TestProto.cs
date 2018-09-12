using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 测试协议
/// </summary>
public struct TestProto : IProto
{
    //很据接口id的不同区分是哪一类数据
    //发送完之后先读取前两个字节（ushort）
    //就能根据ushort来判断是那种协议按照那种协议来解析
    public ushort ProtoID { get { return 1004; } }
    public int Id;
    public string Name;
    public int Type;
    public float Price;
    /// <summary>
    /// 将结构体转换成字节数组
    /// </summary>
    /// <returns></returns>
    public byte[] ToArray()
    {
        using (MMO_MemoryStream ms = new MMO_MemoryStream())
        {
            ms.WriteUShort(ProtoID);//协议类型
            ms.WriteInt(Id);
            ms.WriteUTF8String(Name);
            ms.WriteInt(Type);
            ms.WriteFloat(Price);
            return ms.ToArray();
        }
    }
    /// <summary>
    /// 根据字节数组转换成结构体
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static TestProto GetProto(byte[] buffer)
    {
        TestProto proto = new TestProto();
        using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
        {
            proto.Id = ms.ReadInt();
            proto.Name = ms.ReadUTF8String();
            proto.Type = ms.ReadInt();
            proto.Price = ms.ReadFloat();
        }
        return proto;
    }
}
