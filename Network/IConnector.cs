﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arthas.Network
{
    /// <summary>
    /// 连接器接口
    /// </summary>
    public interface IConnector
    {
        bool IsConnected { get; }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        void Connect(string ip, int port, Action<bool> callback = null);

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="buffer"></param>
        void Send(byte[] buffer);

        /// <summary>
        /// 消息接收事件
        /// </summary>
        event Action MessageRespondEvent;

        /// <summary>
        /// 关闭
        /// </summary>
        void Close();
    }
}
