using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace BaseCore
{
    public class TSession
    {
        /// <summary>
        /// 当这个Session链接成功
        /// </summary>
        /// <param name="socket"></param>
        public void OnConnected(Socket socket)
        {
            this.socket = socket;

            socketRun = true;

            receiveBuffer = new byte[8192];

            receiveEventArgs = new SocketAsyncEventArgs();
            receiveEventArgs.Completed += OnReceiveCompleted;

            sendEventArgs = new SocketAsyncEventArgs();
            sendEventArgs.Completed += OnSendCompleted;

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

            OnConnected();

            StartReceive();          
        }

        public TSession(TService service)
        {
            id = Guid.NewGuid();
            this.service = service;
        }

        /// <summary>
        /// 主动发送数据，永远只有主线程调用
        /// </summary>
        /// <param name="buffer"></param>
        protected void SendAsync(byte[] buffer)
        {
            lock (this)
            {
                if (socketRun == false)
                {
                    return;
                }

                try
                {
                    sendEventArgs.SetBuffer(buffer, 0, buffer.Length);
                    bool isAsync = socket.SendAsync(sendEventArgs);
                    if (isAsync == false)
                    {
                        bool result = ProcessSend(sendEventArgs);
                        if (result == false)
                        {
                            CloseSession();
                        }
                    }
                }
                catch (Exception)
                {
                    CloseSession();
                }
            }
        }

        private void StartReceive()
        {
            lock (this)
            {
                if (socketRun == false)
                {
                    return;
                }

                try
                {
                    receiveEventArgs.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);

                    bool isAsync = socket.ReceiveAsync(receiveEventArgs);
                    if (isAsync == false)
                    {
                        bool result = ProcessReceive(receiveEventArgs);
                        if (result == true)
                        {
                            StartReceive();
                        }
                        else
                        {
                            CloseSession();
                        }
                    }
                }
                catch (Exception)
                {
                    CloseSession();
                }
            }
        }
        
        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs eventArgs)
        {
            lock (this)
            {
                if (eventArgs.LastOperation == SocketAsyncOperation.Receive)
                {
                    bool result = ProcessReceive(eventArgs);
                    if (result == true)
                    {
                        StartReceive();
                    }
                    else
                    {
                        CloseSession();
                    }
                }
            }
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs eventArgs)
        {
            lock (this)
            {
                if (eventArgs.LastOperation == SocketAsyncOperation.Send)
                {
                    bool result = ProcessSend(sendEventArgs);
                    if (result == false)
                    {
                        CloseSession();
                    }
                }
            }
        }

        private bool ProcessSend(SocketAsyncEventArgs eventArgs)
        {
            lock (this)
            {
                if (socketRun == false)
                {
                    return false;
                }

                if (eventArgs.SocketError == SocketError.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 处理接收事件
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns>接收数据是否成功 返回true，则处理成功 返回false,则处理失败</returns>
        private bool ProcessReceive(SocketAsyncEventArgs eventArgs)
        {
            lock (this)
            {
                if (socketRun == false)
                {
                    return false;
                }

                if (eventArgs.SocketError == SocketError.Success)
                {
                    int readSize = eventArgs.BytesTransferred;
                    if (readSize > 0)
                    {
                        OnReceive(receiveBuffer, 0, readSize);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public void CloseSession()
        {
            lock (this)
            {
                if (socketRun == false)
                {
                    return;
                }

                receiveEventArgs.Completed -= OnReceiveCompleted;
                sendEventArgs.Completed -= OnSendCompleted;

                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                }

                socket.Close();
                socket.Dispose();

                receiveEventArgs.Dispose();
                sendEventArgs.Dispose();

                socketRun = false;

                OnDisconnected();

                service.sessionDic.TryRemove(id, out TSession session);
            }
        }

        /// <summary>
        /// 给派生类使用，当链接建立时的回调函数
        /// </summary>
        public virtual void OnConnected() { }

        /// <summary>
        /// 给派生类使用，当收到一段字节数据的回调函数
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public virtual void OnReceive(byte[] buffer, int offset, int size) { }

        /// <summary>
        /// 给派生类使用，当链接断开时的回调函数
        /// </summary>
        public virtual void OnDisconnected() { }

        /// <summary>
        /// 接收缓冲区
        /// </summary>
        private byte[] receiveBuffer;

        /// <summary>
        /// 接收事件对象
        /// </summary>
        private SocketAsyncEventArgs receiveEventArgs;

        /// <summary>
        /// 发送事件对象
        /// </summary>
        private SocketAsyncEventArgs sendEventArgs;

        /// <summary>
        /// 标志这个Socket对象是否还在运行
        /// </summary>
        public bool socketRun;

        /// <summary>
        /// 这个TSession对象对应的Socket
        /// </summary>
        private Socket socket;

        /// <summary>
        /// 这个TSession对象的Id
        /// </summary>
        public Guid id { get; }

        /// <summary>
        /// 这个TSession对象所属的TService
        /// </summary>
        private TService service;
    }
}