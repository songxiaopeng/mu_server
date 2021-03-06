﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GameServer.Core.Executor;
using GameServer.Core.GameEvent;
using GameServer.Core.GameEvent.EventOjectImpl;
using GameServer.Logic;
using GameServer.Logic.KuaFuIPStatistics;
using GameServer.Server;
using KF.Contract.Data;
using Server.Protocol;
using Server.Tools;

namespace Server.TCP
{
	
	public sealed class SocketListener
	{
		
		
		public int ConnectedSocketsCount
		{
			get
			{
				int i = 0;
				lock (this.ConnectedSocketsDict)
				{
					i = this.ConnectedSocketsDict.Count;
				}
				return i;
			}
		}

		
		
		public int ReadPoolCount
		{
			get
			{
				return this.readPool.Count;
			}
		}

		
		
		public int WritePoolCount
		{
			get
			{
				return this.writePool.Count;
			}
		}

		
		
		public long TotalBytesReadSize
		{
			get
			{
				long i = 0L;
				Interlocked.Exchange(ref i, this.totalBytesRead);
				return i;
			}
		}

		
		
		public long TotalBytesWriteSize
		{
			get
			{
				long i = 0L;
				Interlocked.Exchange(ref i, this.totalBytesWrite);
				return i;
			}
		}

		
		
		
		public bool DontAccept
		{
			get
			{
				return this._DontAccept;
			}
			set
			{
				this._DontAccept = value;
			}
		}

		
		// (add) Token: 0x06004054 RID: 16468 RVA: 0x003BB8C4 File Offset: 0x003B9AC4
		// (remove) Token: 0x06004055 RID: 16469 RVA: 0x003BB900 File Offset: 0x003B9B00
		public event SocketConnectedEvnetHandler SocketConnected = null;

		
		// (add) Token: 0x06004056 RID: 16470 RVA: 0x003BB93C File Offset: 0x003B9B3C
		// (remove) Token: 0x06004057 RID: 16471 RVA: 0x003BB978 File Offset: 0x003B9B78
		public event SocketClosedEventHandler SocketClosed = null;

		
		// (add) Token: 0x06004058 RID: 16472 RVA: 0x003BB9B4 File Offset: 0x003B9BB4
		// (remove) Token: 0x06004059 RID: 16473 RVA: 0x003BB9F0 File Offset: 0x003B9BF0
		public event SocketReceivedEventHandler SocketReceived = null;

		
		// (add) Token: 0x0600405A RID: 16474 RVA: 0x003BBA2C File Offset: 0x003B9C2C
		// (remove) Token: 0x0600405B RID: 16475 RVA: 0x003BBA68 File Offset: 0x003B9C68
		public event SocketSendedEventHandler SocketSended = null;

		
		internal SocketListener(int numConnections, int receiveBufferSize)
		{
			this.totalBytesRead = 0L;
			this.totalBytesWrite = 0L;
			this.numConnections = numConnections;
			this.ReceiveBufferSize = receiveBufferSize;
			int readBuffNum = numConnections * 3;
			this.bufferManager = new BufferManager(receiveBufferSize * readBuffNum, receiveBufferSize);
			this.ConnectedSocketsDict = new Dictionary<TMSKSocket, bool>(readBuffNum);
			this.readPool = new SocketAsyncEventArgsPool(readBuffNum);
			this.writePool = new SocketAsyncEventArgsPool(readBuffNum);
		}

		
		private void AddSocket(TMSKSocket socket)
		{
			lock (this.ConnectedSocketsDict)
			{
				this.ConnectedSocketsDict.Add(socket, true);
			}
		}

		
		private void RemoveSocket(TMSKSocket socket)
		{
			lock (this.ConnectedSocketsDict)
			{
				this.ConnectedSocketsDict.Remove(socket);
			}
		}

		
		private bool FindSocket(TMSKSocket socket)
		{
			bool ret = false;
			lock (this.ConnectedSocketsDict)
			{
				ret = this.ConnectedSocketsDict.ContainsKey(socket);
			}
			return ret;
		}

		
		private void CloseClientSocket(SocketAsyncEventArgs e, string reason)
		{
			AsyncUserToken aut = e.UserToken as AsyncUserToken;
			TMSKSocket s = null;
			try
			{
				s = aut.CurrentSocket;
				string ip = "未知";
				try
				{
					ip = string.Format("{0}", s.RemoteEndPoint);
				}
				catch (Exception)
				{
				}
				LogManager.WriteLog(LogTypes.Error, string.Format("远程连接关闭: {0}, 当前总共: {1}, 原因1:{2}, 原因2:{3}", new object[]
				{
					ip,
					this.ConnectedSocketsCount,
					reason,
					s.CloseReason
				}), null, true);
				this.CloseSocket(s, "");
			}
			finally
			{
				aut.CurrentSocket = null;
				aut.Tag = null;
				if (e.LastOperation == SocketAsyncOperation.Send)
				{
					e.SetBuffer(null, 0, 0);
					if (null != s)
					{
						s.PushWriteSocketAsyncEventArgs(e);
					}
				}
				else if (e.LastOperation == SocketAsyncOperation.Receive)
				{
					if (null != s)
					{
						s.PushReadSocketAsyncEventArgs(e);
					}
				}
			}
		}

		
		public Dictionary<long, Tuple<int, int, int>> GetSocketCnt()
		{
			Dictionary<long, Tuple<int, int, int>> tmpDict = new Dictionary<long, Tuple<int, int, int>>();
			lock (this.ConnectedSocketsDict)
			{
				foreach (KeyValuePair<TMSKSocket, bool> socket in this.ConnectedSocketsDict)
				{
					int startgame = 0;
					int login = 0;
					if (null != socket.Key.session)
					{
						startgame = ((socket.Key.session.SocketState == 4) ? 1 : 0);
						login = ((socket.Key.session.SocketTime[1] > 0L) ? 1 : 0);
					}
					Tuple<int, int, int> tmpTuple = null;
					if (tmpDict.TryGetValue(Global.GetIpAsIntSafe(socket.Key), out tmpTuple))
					{
						tmpTuple = new Tuple<int, int, int>(tmpTuple.Item1 + 1, tmpTuple.Item2 + startgame, tmpTuple.Item3 + login);
					}
					else
					{
						tmpTuple = new Tuple<int, int, int>(1, startgame, login);
					}
					tmpDict[Global.GetIpAsIntSafe(socket.Key)] = tmpTuple;
				}
			}
			return tmpDict;
		}

		
		internal void Init()
		{
			this.bufferManager.InitBuffer();
			int readBuffNum = this.numConnections * 3;
			for (int i = 0; i < readBuffNum; i++)
			{
				SocketAsyncEventArgs readWriteEventArg = new SocketAsyncEventArgs();
				readWriteEventArg.Completed += this.OnIOCompleted;
				readWriteEventArg.UserToken = new AsyncUserToken
				{
					CurrentSocket = null,
					Tag = null
				};
				this.bufferManager.SetBuffer(readWriteEventArg);
				this.readPool.Push(readWriteEventArg);
			}
			for (int i = 0; i < readBuffNum; i++)
			{
				SocketAsyncEventArgs readWriteEventArg = new SocketAsyncEventArgs();
				readWriteEventArg.Completed += this.OnIOCompleted;
				readWriteEventArg.UserToken = new AsyncUserToken
				{
					CurrentSocket = null,
					Tag = null
				};
				this.writePool.Push(readWriteEventArg);
			}
		}

		
		private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
		{
			try
			{
				if (null != this.listenSocket)
				{
					this.ProcessAccept(e);
				}
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Error, string.Format("在SocketListener::OnAcceptCompleted 中发生了异常错误", new object[0]), null, true);
				DataHelper.WriteFormatExceptionLog(ex, "OnAcceptCompleted", false, false);
			}
		}

		
		private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
		{
			try
			{
				SocketAsyncOperation lastOperation = e.LastOperation;
				if (lastOperation != SocketAsyncOperation.Receive)
				{
					if (lastOperation != SocketAsyncOperation.Send)
					{
						throw new ArgumentException("The last operation completed on the socket was not a receive or send");
					}
					this.ProcessSend(e);
				}
				else
				{
					this.ProcessReceive(e);
				}
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Error, string.Format("在SocketListener::OnIOCompleted 中发生了异常错误", new object[0]), null, true);
				DataHelper.WriteFormatExceptionLog(ex, "OnIOCompleted", false, false);
			}
		}

		
		private bool _ReceiveAsync(SocketAsyncEventArgs readEventArgs)
		{
			bool result;
			try
			{
				TMSKSocket s = (readEventArgs.UserToken as AsyncUserToken).CurrentSocket;
				result = s.ReceiveAsync(readEventArgs);
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Error, string.Format("在SocketListener::_ReceiveAsync 中发生了异常错误", new object[0]), null, true);
				string str = ex.Message.ToString();
				this.CloseClientSocket(readEventArgs, str.Replace('\n', ' '));
				result = true;
			}
			return result;
		}

		
		private bool _SendAsync(SocketAsyncEventArgs writeEventArgs, out bool exception)
		{
			exception = false;
			bool result;
			try
			{
				TMSKSocket s = (writeEventArgs.UserToken as AsyncUserToken).CurrentSocket;
				result = s.SendAsync(writeEventArgs);
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Error, string.Format("在SocketListener::_SendAsync 中发生了异常错误:{0}", ex.Message), null, true);
				exception = true;
				result = true;
			}
			return result;
		}

		
		internal bool SendData(TMSKSocket s, TCPOutPacket tcpOutPacket, bool pushBack = true)
		{
			if (s != null && tcpOutPacket != null)
			{
				ushort sendCmdId = tcpOutPacket.PacketCmdID;
				if (s.magic > 0 && sendCmdId > 100)
				{
					sendCmdId += s.magic;
				}
				Array.Copy(BitConverter.GetBytes(sendCmdId), 0, tcpOutPacket.GetPacketBytes(), 4, 2);
			}
			bool bRet = false;
			if (null != s)
			{
				if (s.Connected)
				{
					bRet = Global._SendBufferManager.AddOutPacket(s, tcpOutPacket);
				}
			}
			if (pushBack)
			{
				Global._TCPManager.TcpOutPacketPool.Push(tcpOutPacket);
			}
			return bRet;
		}

		
		public bool SendData(TMSKSocket s, byte[] buffer, int offset, int count, MemoryBlock item)
		{
			this.GTotalSendCount++;
			SocketAsyncEventArgs writeEventArgs = s.PopWriteSocketAsyncEventArgs();
			if (null == writeEventArgs)
			{
				writeEventArgs = new SocketAsyncEventArgs();
				writeEventArgs.Completed += this.OnIOCompleted;
				writeEventArgs.UserToken = new AsyncUserToken
				{
					CurrentSocket = null,
					Tag = null
				};
			}
			writeEventArgs.SetBuffer(buffer, offset, count);
			AsyncUserToken userToken = writeEventArgs.UserToken as AsyncUserToken;
			userToken.CurrentSocket = s;
			userToken.Tag = item;
			bool exception = false;
			if (!this._SendAsync(writeEventArgs, out exception))
			{
				this.ProcessSend(writeEventArgs);
			}
			if (exception)
			{
				if (null != this.SocketSended)
				{
					this.SocketSended(this, writeEventArgs);
				}
				writeEventArgs.SetBuffer(null, 0, 0);
				userToken.CurrentSocket = null;
				userToken.Tag = null;
				s.PushWriteSocketAsyncEventArgs(writeEventArgs);
			}
			return !exception;
		}

		
		public bool SendData(TMSKSocket s, byte[] buffer, int offset, int count, MemoryBlock item, SendBuffer sendBuffer)
		{
			this.GTotalSendCount++;
			SocketAsyncEventArgs writeEventArgs = s.PopWriteSocketAsyncEventArgs();
			if (null == writeEventArgs)
			{
				writeEventArgs = new SocketAsyncEventArgs();
				writeEventArgs.Completed += this.OnIOCompleted;
				writeEventArgs.UserToken = new AsyncUserToken
				{
					CurrentSocket = null,
					Tag = null
				};
			}
			writeEventArgs.SetBuffer(buffer, offset, count);
			AsyncUserToken userToken = writeEventArgs.UserToken as AsyncUserToken;
			userToken.CurrentSocket = s;
			userToken.Tag = item;
			userToken._SendBuffer = sendBuffer;
			bool exception = false;
			if (!this._SendAsync(writeEventArgs, out exception))
			{
				this.ProcessSend(writeEventArgs);
			}
			if (exception)
			{
				if (null != this.SocketSended)
				{
					this.SocketSended(this, writeEventArgs);
				}
				writeEventArgs.SetBuffer(null, 0, 0);
				userToken.CurrentSocket = null;
				userToken.Tag = null;
				s.PushWriteSocketAsyncEventArgs(writeEventArgs);
			}
			return !exception;
		}

		
		private void ProcessAccept(SocketAsyncEventArgs e)
		{
			TMSKSocket s = new TMSKSocket(e.AcceptSocket);
			s.SetAcceptIp();
			bool disableConnect = false;
			bool? inIpWriteList = null;
			if (this.EnabledIPListFilter)
			{
				lock (this.IPWhiteList)
				{
					if (this.EnabledIPListFilter && s != null && null != s.RemoteEndPoint)
					{
						IPEndPoint remoteIPEndPoint = s.RemoteEndPoint as IPEndPoint;
						if (remoteIPEndPoint != null && null != remoteIPEndPoint.Address)
						{
							string remoteIP = remoteIPEndPoint.Address.ToString();
							if (!string.IsNullOrEmpty(remoteIP) && !this.IPWhiteList.ContainsKey(remoteIP))
							{
								LogManager.WriteLog(LogTypes.Error, string.Format("新远程连接: {0}, 但是客户端IP处于IP过滤中:{1}", s.RemoteEndPoint, this.ConnectedSocketsCount), null, true);
								inIpWriteList = new bool?(false);
							}
							else
							{
								inIpWriteList = new bool?(true);
							}
						}
					}
				}
			}
			if (IPStatisticsManager.getInstance().GetIPInBeOperation(s, IPOperaType.BanConnect))
			{
				disableConnect = true;
			}
			if (this.DontAccept || disableConnect)
			{
				try
				{
					if (disableConnect)
					{
						LogManager.WriteLog(LogTypes.Error, string.Format("新远程连接: {0}, 但是客户端IP处于IP过滤中，直接关闭连接:{1}", s.RemoteEndPoint, this.ConnectedSocketsCount), null, true);
					}
					else if (this.DontAccept)
					{
						LogManager.WriteLog(LogTypes.Error, string.Format("新远程连接: {0}, 但是服务器端处于不接受新连接状态，直接关闭连接:{1}", s.RemoteEndPoint, this.ConnectedSocketsCount), null, true);
					}
				}
				catch (Exception)
				{
				}
				try
				{
					s.Shutdown(SocketShutdown.Both);
				}
				catch (Exception)
				{
				}
				try
				{
					s.Close(30);
				}
				catch (Exception)
				{
				}
				this.StartAccept(e);
			}
			else
			{
				byte[] inOptionValues = new byte[12];
				BitConverter.GetBytes(1U).CopyTo(inOptionValues, 0);
				BitConverter.GetBytes(120000U).CopyTo(inOptionValues, 4);
				BitConverter.GetBytes(5000U).CopyTo(inOptionValues, 8);
				s.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
				LingerOption lingerOption = new LingerOption(true, 10);
				s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
				SocketAsyncEventArgs readEventArgs = null;
				readEventArgs = s.PopReadSocketAsyncEventArgs();
				if (null == readEventArgs)
				{
					try
					{
						LogManager.WriteLog(LogTypes.Error, string.Format("新远程连接: {0}, 但是readPool内的缓存不足，直接关闭连接:{1}", s.RemoteEndPoint, this.ConnectedSocketsCount), null, true);
					}
					catch (Exception)
					{
					}
					try
					{
						s.Shutdown(SocketShutdown.Both);
					}
					catch (Exception)
					{
					}
					try
					{
						s.Close(30);
					}
					catch (Exception)
					{
					}
					this.StartAccept(e);
				}
				else
				{
					(readEventArgs.UserToken as AsyncUserToken).CurrentSocket = s;
					Global._SendBufferManager.Add(s);
					this.AddSocket(s);
					try
					{
						LogManager.WriteLog(LogTypes.Error, string.Format("新远程连接: {0}, 当前总共: {1}", s.RemoteEndPoint, this.ConnectedSocketsCount), null, true);
					}
					catch (Exception)
					{
					}
					if (null != this.SocketConnected)
					{
						this.SocketConnected(this, readEventArgs);
					}
					s.session.InIpWhiteList = inIpWriteList;
					s.session.SetSocketTime(0);
					if (!this._ReceiveAsync(readEventArgs))
					{
						this.ProcessReceive(readEventArgs);
					}
					this.StartAccept(e);
				}
			}
		}

		
		private unsafe void ProcessReceive(SocketAsyncEventArgs e)
		{
			AsyncUserToken userToken = e.UserToken as AsyncUserToken;
			TMSKSocket s = userToken.CurrentSocket;
			if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
			{
				Interlocked.Add(ref this.totalBytesRead, (long)e.BytesTransferred);
				bool recvReturn = true;
				if (null != this.SocketReceived)
				{
					if (GameManager.FlagUseWin32Decrypt)
					{
						int length = e.BytesTransferred;
						fixed (byte* p = e.Buffer)
						{
							Win32API.SortBytes(p, e.Offset, e.BytesTransferred, s.SortKey64);
						}
					}
					else
					{
						DataHelper.SortBytes(e.Buffer, e.Offset, e.BytesTransferred, s.SortKey64);
					}
					try
					{
						recvReturn = this.SocketReceived(this, e);
					}
					catch (Exception ex)
					{
						LogManager.WriteException(ex.ToString());
						recvReturn = false;
					}
				}
				if (recvReturn)
				{
					if (!this._ReceiveAsync(e))
					{
						this.ProcessReceive(e);
					}
				}
				else
				{
					ushort lastPacketCmd = TCPManager.getInstance().LastPacketCmdID(s);
					string reason = string.Format("CMD={0}", ((TCPGameServerCmds)lastPacketCmd).ToString());
					this.CloseClientSocket(e, reason);
				}
			}
			else
			{
				string reason = string.Format("[{0}]{1}", (int)e.SocketError, e.SocketError.ToString());
				this.CloseClientSocket(e, reason);
			}
		}

		
		private void ProcessSend(SocketAsyncEventArgs e)
		{
			if (null != this.SocketSended)
			{
				this.SocketSended(this, e);
			}
			if (e.SocketError == SocketError.Success)
			{
				Interlocked.Add(ref this.totalBytesWrite, (long)e.BytesTransferred);
			}
			e.SetBuffer(null, 0, 0);
			TMSKSocket s = (e.UserToken as AsyncUserToken).CurrentSocket;
			(e.UserToken as AsyncUserToken).CurrentSocket = null;
			(e.UserToken as AsyncUserToken).Tag = null;
			if (null != s)
			{
				s.PushWriteSocketAsyncEventArgs(e);
			}
		}

		
		internal void Start(string ip, int port)
		{
			if ("" == ip)
			{
				ip = "0.0.0.0";
			}
			IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
			this.listenSocket = new TMSKSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.listenSocket.Bind(localEndPoint);
			this.listenSocket.Listen(100);
			this.StartAccept(null);
		}

		
		public void Stop()
		{
			TMSKSocket s = this.listenSocket;
			this.listenSocket = null;
			s.Close();
		}

		
		public bool CloseSocket(TMSKSocket s, string reason = "")
		{
			bool result;
			if (!this.FindSocket(s))
			{
				Global._SendBufferManager.Remove(s);
				result = false;
			}
			else
			{
				if (!string.IsNullOrEmpty(reason))
				{
					s.CloseReason = reason;
				}
				this.RemoveSocket(s);
				if (null != this.SocketClosed)
				{
					this.SocketClosed(this, s);
				}
				Global._SendBufferManager.Remove(s);
				try
				{
					s.Shutdown(SocketShutdown.Both);
				}
				catch (Exception ex)
				{
					try
					{
						LogManager.WriteLog(LogTypes.Info, string.Format("CloseSocket s.Shutdown()异常: {0}, {1}", s.RemoteEndPoint, ex.Message), null, true);
					}
					catch (Exception)
					{
					}
				}
				try
				{
					s.Close(30);
				}
				catch (Exception ex)
				{
					try
					{
						LogManager.WriteLog(LogTypes.Info, string.Format("CloseSocket s.Close()异常: {0}, {1}", s.RemoteEndPoint, ex.Message), null, true);
					}
					catch (Exception)
					{
					}
				}
				result = true;
			}
			return result;
		}

		
		private void StartAccept(SocketAsyncEventArgs acceptEventArg)
		{
			if (acceptEventArg == null)
			{
				acceptEventArg = new SocketAsyncEventArgs();
				acceptEventArg.Completed += this.OnAcceptCompleted;
			}
			else
			{
				acceptEventArg.AcceptSocket = null;
			}
			if (!this.listenSocket.AcceptAsync(acceptEventArg))
			{
				this.ProcessAccept(acceptEventArg);
			}
		}

		
		public List<string> InitIPWhiteList(string[] ipList, bool enabeld = true)
		{
			List<string> resultList = new List<string>();
			List<string> result;
			lock (this.IPWhiteList)
			{
				this.EnabledIPListFilter = false;
				this.IPWhiteList.Clear();
				if (ipList != null && ipList.Length > 0)
				{
					foreach (string ipStr in ipList)
					{
						IPAddress ipAddress;
						if (IPAddress.TryParse(ipStr, out ipAddress))
						{
							resultList.Add(ipAddress.ToString());
							this.IPWhiteList[ipAddress.ToString()] = true;
						}
					}
					if (this.IPWhiteList.Count > 0)
					{
						this.EnabledIPListFilter = enabeld;
					}
				}
				result = resultList;
			}
			return result;
		}

		
		public void ClearTimeoutSocket()
		{
			long nowTicks = TimeUtil.NOW();
			lock (this.ConnectedSocketsDict)
			{
				foreach (TMSKSocket socket in this.ConnectedSocketsDict.Keys)
				{
					if (socket.session.SocketTime[1] == 0L && socket.Connected)
					{
						if (Math.Abs(nowTicks - socket.session.SocketTime[0]) > 30000L)
						{
							try
							{
								if (string.IsNullOrEmpty(socket.CloseReason))
								{
									socket.CloseReason = "ClearTimeoutSocket";
									GlobalEventSource.getInstance().fireEvent(new LoginFailByTimeoutEventObject(Global.GetIpAsIntSafe(socket)));
								}
								socket.Shutdown(SocketShutdown.Both);
							}
							catch
							{
							}
							try
							{
								socket.Close(30);
							}
							catch
							{
							}
						}
					}
				}
			}
		}

		
		private const int opsToPreAlloc = 2;

		
		public int GTotalSendCount = 0;

		
		private bool EnabledIPListFilter = false;

		
		private Dictionary<string, bool> IPWhiteList = new Dictionary<string, bool>();

		
		private int ReceiveBufferSize;

		
		private BufferManager bufferManager;

		
		private TMSKSocket listenSocket;

		
		private Dictionary<TMSKSocket, bool> ConnectedSocketsDict;

		
		public int numConnections;

		
		public SocketAsyncEventArgsPool readPool;

		
		public SocketAsyncEventArgsPool writePool;

		
		private long totalBytesRead;

		
		private long totalBytesWrite;

		
		private bool _DontAccept = true;
	}
}
