using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer
{
    /// <summary>
    /// 观察者
    /// </summary>
    public class EventDispatchet : Singleton<EventDispatchet>
    {
        public delegate void OnActionHandle(Role role, byte[] buffer);

        private Dictionary<ushort, List<OnActionHandle>> dic = new Dictionary<ushort, List<OnActionHandle>>();
        /// <summary>
        /// 添加监听
        /// </summary>
        /// <param name="protoCode"></param>
        /// <param name="handler"></param>
        public void AddEventListener(ushort protoCode, OnActionHandle handler)
        {
            if (dic.ContainsKey(protoCode))
            {
                dic[protoCode].Add(handler);
            }
            else
            {
                List<OnActionHandle> HandleList = new List<OnActionHandle>();
                HandleList.Add(handler);
                dic[protoCode] = HandleList;
            }
        }
        /// <summary>
        /// 删除监听
        /// </summary>
        /// <param name="protoCode"></param>
        /// <param name="handler"></param>
        public void RemoveEventListener(ushort protoCode, OnActionHandle handler)
        {
            if (dic.ContainsKey(protoCode))
            {
                List<OnActionHandle> HandleList = dic[protoCode];
                HandleList.Remove(handler);
                if (HandleList.Count == 0)
                {
                    dic.Remove(protoCode);
                }
            }
        }
        /// <summary>
        /// 发送消息，运行委托
        /// </summary>
        /// <param name="protoCode"></param>
        /// <param name="param"></param>
        public void Dispatch(ushort protoCode, Role role, byte[] buffer)
        {
            if (dic.ContainsKey(protoCode))
            {
                List<OnActionHandle> HandleList = dic[protoCode];
                if (HandleList.Count > 0 && HandleList != null)
                {
                    for (int i = 0; i < HandleList.Count; i++)
                    {
                        if (HandleList[i] != null)
                        {
                            HandleList[i](role, buffer);
                        }
                    }
                }
            }
        }
    }
}
