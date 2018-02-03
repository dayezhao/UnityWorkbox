﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using PlayCity;
using UnityEngine;

namespace Arthas.Network
{
    /// <summary>
    /// 默认消息
    /// </summary>
    public class DefaultMessage : INetworkMessage
    {
        protected byte[] buffer = { };

        public bool WithLength { get; private set; }

        public object[] Parameters { get; private set; }

        public object Body { get { return buffer; } }

        public object Command { get; private set; }

        /// <summary>
        /// 默认消息构造
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="bodyBuffer">消息体</param>
        /// <param name="withLength">消息体是否包含长度</param>
        /// <param name="parameters">消息其他参数</param>
        public DefaultMessage(object command, byte[] bodyBuffer, bool withLength = true, params object[] parameters)
        {
            Command = command;
            buffer = bodyBuffer;
            WithLength = withLength;
            Parameters = parameters;
        }

        public virtual byte[] GetBuffer(bool littleEndian = false)
        {
            if (WithLength) return buffer;
            var lenBytes = littleEndian && BitConverter.IsLittleEndian
                ? BitConverter.GetBytes((short)buffer.Length)
                : BitConverter.GetBytes((short)buffer.Length).Reverse();
            var newBuffer = new byte[buffer.Length + lenBytes.Length];
            Buffer.BlockCopy(lenBytes, 0, newBuffer, 0, lenBytes.Length);
            Buffer.BlockCopy(buffer, 0, newBuffer, lenBytes.Length, buffer.Length);
            return newBuffer;
        }

        public virtual T GetValue<T>()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 默认的消息包装器
    /// </summary>
    public class DefaultMessageHandler : INetworkMessageHandler
    {
        public virtual INetworkMessage PackMessage(object command, object obj, params object[] parameters)
        {
            var bodyBuffer = obj as byte[];
            if (obj != null && bodyBuffer == null)
            {
                var msg = "<color=cyan>{0}</color> cannot support <color=cyan>{1}</color> type message , " +
                    "\n please implement your custom message!";
                throw new NotImplementedException(string.Format(msg, GetType(), obj.GetType()));
            }
            using (var stream = new MemoryStream())
            {
                var cmdBytes = BitConverter.GetBytes(Convert.ToInt16(command));
                stream.Write(cmdBytes, 0, cmdBytes.Length);
                if (bodyBuffer != null) stream.Write(bodyBuffer, 0, bodyBuffer.Length);
                return new DefaultMessage(command, stream.ToArray(), false, parameters);
            }
        }
        public virtual IList<INetworkMessage> ParseMessage(byte[] buffer)
        {
            var messages = new List<INetworkMessage>();
            using (var stream = new MemoryStream(buffer))
            using (var reader = new BinaryReader(stream))
            {
                while (reader.BaseStream.Position < buffer.Length)
                {
                    var len = reader.ReadInt16();
                    var cmd = reader.ReadInt16();
                    var content = reader.ReadBytes(len - sizeof(short));
                    var msg = new DefaultMessage(cmd, content);
                    messages.Add(msg);
                }
                return messages;
            }
        }

        public IList<INetworkMessage> ParseMessage(ByteBuf buffer)
        {
            var messages = new List<INetworkMessage>();
            doDecode(buffer, messages);
            return messages;
        }

        private void doDecode(ByteBuf input, IList<INetworkMessage> output) {
            if (input.ReadableBytes() < 6)
            {//不够包头长度
                return;
            }

            
            short length = BitConverter.ToInt16(input.GetRaw(), input.ReaderIndex());
            input.SkipBytes(2);
            if (input.ReadableBytes() + 4 < length)
            {//包体长度不够
                input.ResetReaderIndex();
                return;
            }

            short command = BitConverter.ToInt16(input.GetRaw(), input.ReaderIndex());
            input.SkipBytes(2);
            short responseCode = BitConverter.ToInt16(input.GetRaw(), input.ReaderIndex()); ;
            input.SkipBytes(2);
            int dataSize = length - 4;//减掉command部分
            
            if (dataSize > input.ReadableBytes())
            {
                input.ResetReaderIndex();
                return;
            }
            

            byte[] data = new byte[dataSize];
            if (dataSize > 0)
            {
                input.ReadBytes(data, 0, data.Length);//拿到数据
                input.MarkReaderIndex();
            }
            if (command != Commands.Heartbeat)
            {
                var msg = new PlayCityMessage(command, data, true, responseCode);
                output.Add(msg);
            }

            /*var len = reader.ReadInt16();
            var cmd = reader.ReadInt16();
            var code = reader.ReadInt16();
            var content = reader.ReadBytes(len - sizeof(int));
            var msg = new PlayCityMessage(cmd, content, true, code);*/

            doDecode(input, output);
        }
    }
}
