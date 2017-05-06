﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Arthas.Network
{
    /// <summary>
    /// 消息头接口
    /// </summary>
    public interface IMessageHeader
    {
        /// <summary>
        /// 是否为小字节序
        /// </summary>
        bool IsLittleEndian { get; }

        short Command { get; set; }

        /// <summary>
        /// 
        /// </summary>
        byte[] GetBuffer();

        /// <summary>
        /// 根据字节创建消息头
        /// </summary>
        /// <returns></returns>
        void Overwrite(byte[] buffer);

        /// <summary>
        /// 从字节中移除消息头
        /// </summary>
        /// <returns></returns>
        void ExceptHeader(ref byte[] buffer);
    }

    /// <summary>
    /// 消息接口
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// 消息头
        /// </summary>
        IMessageHeader Header { get; }
        
        /// <summary>
        /// 获取消息体
        /// </summary>
        /// <returns></returns>
        byte[] GetBuffer(bool containsHead = false);

        /// <summary>
        /// 获取消息包含长度
        /// </summary>
        /// <returns></returns>
        byte[] GetBufferWithLength();

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetValue<T>();
    }

    /// <summary>
    /// 消息包装接口
    /// </summary>
    public interface IMessageWrapper
    {
        /// <summary>
        /// 请求消息头
        /// </summary>
        IMessageHeader RequestHeader { get; set; }

        /// <summary>
        /// 响应消息头
        /// </summary>
        IMessageHeader ResponseHeader { get; set; }

        /// <summary>
        /// 从字符串创建消息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        IMessage FromString(string str);

        /// <summary>
        /// 从一个对象创建消息
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        IMessage FromObject(object obj);

        /// <summary>
        /// 从字节数组创建消息
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="containHeader"></param>
        /// <returns></returns>
        IMessage FromBuffer(byte[] buffer, bool containHeader = false);
    }
}