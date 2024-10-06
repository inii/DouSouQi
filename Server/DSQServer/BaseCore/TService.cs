using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BaseCore
{
    public class TService
    {
        /// <summary>
        /// 启动这个TService 主线程调用
        /// </summary>
        /// <returns>返回true，启动成功 返回false，启动失败</returns>
        public bool Start()
        {
            if (IsRunning == true)
            {
                return false;
            }          

            acceptorEventArgs = new SocketAsyncEventArgs();
            acceptorEventArgs.Completed += OnAsyncCompleted;

            acceptor = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            acceptor.Bind(EndPoint);
            EndPoint = (IPEndPoint)acceptor.LocalEndPoint;

            acceptor.Listen(backlog);

            IsRunning = true;

            StartAccept(acceptorEventArgs);

            Console.WriteLine("服务器开始监听...");

            return true;
        }

        /// <summary>
        /// 停止这个TService 主线程调用
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
        }

        public virtual TSession CreateSession()
        {
            return new TSession(this);
        }

        public TService(string address, int port) : this(new IPEndPoint(IPAddress.Parse(address), port)) 
        {
            IsRunning = false;
        }

        private TService(IPEndPoint endPoint)
        {
            Id = Guid.NewGuid();
            EndPoint = endPoint;
        }

        /// <summary>
        /// 投递接收链接请求 主线程和工作线程都会调用
        /// </summary>
        /// <param name="eventArgs"></param>
        private void StartAccept(SocketAsyncEventArgs eventArgs)
        {
            eventArgs.AcceptSocket = null;

            bool isAsync = acceptor.AcceptAsync(eventArgs);
            if (isAsync == false)
            {
                ProcessAccept(eventArgs);
            }
        }

        private void OnAsyncCompleted(object sender, SocketAsyncEventArgs eventArgs)
        {
            if (IsRunning == true)
            {
                ProcessAccept(eventArgs);
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs.SocketError == SocketError.Success)
            {
                TSession session = CreateSession();

                sessionDic.TryAdd(session.id, session);

                session.OnConnected(eventArgs.AcceptSocket);
            }

            if (IsRunning == true)
            {
                StartAccept(eventArgs);
            }
        }

        /// <summary>
        /// 线程安全的字典，存放所有的TSession对象
        /// </summary>
        public readonly ConcurrentDictionary<Guid, TSession> sessionDic = new ConcurrentDictionary<Guid, TSession>();

        /// <summary>
        /// 请求链接的队列的最大长度
        /// </summary>
        private int backlog = 1024;

        /// <summary>
        /// 监听客户端链接请求用的socket对象
        /// </summary>
        private Socket acceptor;

        /// <summary>
        /// 客户端链接请求事件对象
        /// </summary>
        private SocketAsyncEventArgs acceptorEventArgs;

        /// <summary>
        /// 标志这个TService是否还在运行
        /// </summary>
        public bool IsRunning;

        public Guid Id { get; }

        public IPEndPoint EndPoint { get; private set; }
    }
}