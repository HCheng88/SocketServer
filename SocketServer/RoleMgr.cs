using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer
{
   public class RoleMgr
    {
        //私有的构造防止在外部被实例化
        private RoleMgr()
        {
            mAllRole = new List<Role>();
        }
        #region 单例
        private static object lock_obg = new object();
        private static RoleMgr Instance;

        public static RoleMgr _instance
        {
            get
            {
                if (Instance == null)
                {
                    lock (lock_obg)
                    {
                        if (Instance == null)
                            Instance = new RoleMgr();
                    }
                }
                return Instance;
            }
        }
        #endregion

        private List<Role> mAllRole;
        public List<Role> AllRole
        {
            get
            {
                return mAllRole;
            }
        }
    }
}
