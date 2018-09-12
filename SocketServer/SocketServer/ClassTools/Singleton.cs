using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer
{
    /// <summary>
    /// 泛型单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> : IDisposable where T : new()
    {
        private static T Instance;
        public static T _instance
        {
            get
            {
                if (Instance == null)
                {
                    Instance = new T();
                }
                return Instance;
            }
        }


        public virtual void Dispose()
        {

        }
    }
}
