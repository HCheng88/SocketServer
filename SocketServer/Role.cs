using System;
using System.Collections.Generic;
using System.Text;

namespace SocketServer
{
    /// <summary>
    /// 角色，一个角色代表一个客户端
    /// </summary>
    public class Role
    {
        public ClicentSocket clicentSocket{ get; set; }
    }
}
