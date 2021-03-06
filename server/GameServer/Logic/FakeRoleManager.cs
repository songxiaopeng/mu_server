﻿using System;
using System.Collections.Generic;
using System.Windows;
using GameServer.Core.Executor;
using GameServer.Interface;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;

namespace GameServer.Logic
{
	
	public static class FakeRoleManager
	{
		
		private static FakeRoleItem AddFakeRole(SafeClientData clientData, FakeRoleTypes fakeRoleType)
		{
			FakeRoleItem fakeRoleItem = new FakeRoleItem
			{
				FakeRoleID = (int)GameManager.FakeRoleIDMgr.GetNewID(),
				FakeRoleType = (int)fakeRoleType,
				MyRoleDataMini = Global.ClientDataToRoleDataMini(clientData)
			};
			lock (FakeRoleManager._ID2FakeRoleDict)
			{
				FakeRoleManager._ID2FakeRoleDict[fakeRoleItem.FakeRoleID] = fakeRoleItem;
			}
			string roleID_Type = string.Format("{0}_{1}", fakeRoleItem.MyRoleDataMini.RoleID, (int)fakeRoleType);
			lock (FakeRoleManager._RoleIDType2FakeRoleDict)
			{
				FakeRoleManager._RoleIDType2FakeRoleDict[roleID_Type] = fakeRoleItem;
			}
			return fakeRoleItem;
		}

		
		private static FakeRoleItem AddFakeRole(RoleData4Selector clientData, FakeRoleTypes fakeRoleType)
		{
			FakeRoleItem result;
			if (null == clientData)
			{
				result = null;
			}
			else
			{
				FakeRoleItem fakeRoleItem = new FakeRoleItem
				{
					FakeRoleID = (int)GameManager.FakeRoleIDMgr.GetNewID(),
					FakeRoleType = (int)fakeRoleType,
					MyRoleDataMini = RoleManager.getInstance().GetRoleDataMiniFromRoleData4Selector(clientData)
				};
				lock (FakeRoleManager._ID2FakeRoleDict)
				{
					FakeRoleManager._ID2FakeRoleDict[fakeRoleItem.FakeRoleID] = fakeRoleItem;
				}
				string roleID_Type = string.Format("{0}_{1}", fakeRoleItem.MyRoleDataMini.RoleID, (int)fakeRoleType);
				lock (FakeRoleManager._RoleIDType2FakeRoleDict)
				{
					FakeRoleManager._RoleIDType2FakeRoleDict[roleID_Type] = fakeRoleItem;
				}
				result = fakeRoleItem;
			}
			return result;
		}

		
		public static FakeRoleItem FindFakeRoleByID(int FakeRoleID)
		{
			FakeRoleItem FakeRoleItem = null;
			lock (FakeRoleManager._ID2FakeRoleDict)
			{
				FakeRoleManager._ID2FakeRoleDict.TryGetValue(FakeRoleID, out FakeRoleItem);
			}
			return FakeRoleItem;
		}

		
		public static FakeRoleItem FindFakeRoleByRoleIDType(int roleID, FakeRoleTypes fakeRoleType)
		{
			FakeRoleItem fakeRoleItem = null;
			string roleID_Type = string.Format("{0}_{1}", roleID, (int)fakeRoleType);
			lock (FakeRoleManager._RoleIDType2FakeRoleDict)
			{
				FakeRoleManager._RoleIDType2FakeRoleDict.TryGetValue(roleID_Type, out fakeRoleItem);
			}
			return fakeRoleItem;
		}

		
		private static void RemoveFakeRole(int FakeRoleID)
		{
			FakeRoleItem fakeRoleItem = null;
			lock (FakeRoleManager._ID2FakeRoleDict)
			{
				FakeRoleManager._ID2FakeRoleDict.TryGetValue(FakeRoleID, out fakeRoleItem);
				if (null != fakeRoleItem)
				{
					FakeRoleManager._ID2FakeRoleDict.Remove(fakeRoleItem.FakeRoleID);
				}
			}
			if (null != fakeRoleItem)
			{
				string roleID_Type = string.Format("{0}_{1}", fakeRoleItem.MyRoleDataMini.RoleID, fakeRoleItem.FakeRoleType);
				lock (FakeRoleManager._RoleIDType2FakeRoleDict)
				{
					FakeRoleManager._RoleIDType2FakeRoleDict.Remove(roleID_Type);
				}
			}
		}

		
		private static void RemoveFakeRoleByRoleIDType(int roleID, FakeRoleTypes fakeRoleType)
		{
			FakeRoleItem fakeRoleItem = null;
			string roleID_Type = string.Format("{0}_{1}", roleID, (int)fakeRoleType);
			lock (FakeRoleManager._RoleIDType2FakeRoleDict)
			{
				FakeRoleManager._RoleIDType2FakeRoleDict.TryGetValue(roleID_Type, out fakeRoleItem);
				if (null != fakeRoleItem)
				{
					FakeRoleManager._RoleIDType2FakeRoleDict.Remove(roleID_Type);
				}
			}
			if (null != fakeRoleItem)
			{
				lock (FakeRoleManager._ID2FakeRoleDict)
				{
					FakeRoleManager._ID2FakeRoleDict.Remove(fakeRoleItem.FakeRoleID);
				}
			}
		}

		
		private static List<FakeRoleItem> RemoveFakeRoleByType(FakeRoleTypes fakeRoleType)
		{
			List<FakeRoleItem> fakeRoleItemList = new List<FakeRoleItem>();
			lock (FakeRoleManager._ID2FakeRoleDict)
			{
				foreach (FakeRoleItem item in FakeRoleManager._ID2FakeRoleDict.Values)
				{
					if (item.FakeRoleType == (int)fakeRoleType)
					{
						fakeRoleItemList.Add(item);
					}
				}
				foreach (FakeRoleItem item in fakeRoleItemList)
				{
					FakeRoleManager._ID2FakeRoleDict.Remove(item.FakeRoleID);
				}
			}
			lock (FakeRoleManager._RoleIDType2FakeRoleDict)
			{
				foreach (FakeRoleItem item in fakeRoleItemList)
				{
					string roleID_Type = string.Format("{0}_{1}", item.MyRoleDataMini.RoleID, item.FakeRoleType);
					FakeRoleManager._RoleIDType2FakeRoleDict.Remove(roleID_Type);
				}
			}
			return fakeRoleItemList;
		}

		
		private static List<FakeRoleItem> GetFakeRoleListByType(FakeRoleTypes fakeRoleType)
		{
			List<FakeRoleItem> fakeRoleItemList = new List<FakeRoleItem>();
			lock (FakeRoleManager._ID2FakeRoleDict)
			{
				foreach (FakeRoleItem item in FakeRoleManager._ID2FakeRoleDict.Values)
				{
					if (item.FakeRoleType == (int)fakeRoleType)
					{
						fakeRoleItemList.Add(item);
					}
				}
			}
			return fakeRoleItemList;
		}

		
		public static int ProcessNewFakeRole(SafeClientData clientData, int mapCode, FakeRoleTypes fakeRoleType, int direction = -1, int toPosX = -1, int toPosY = -1, int ToExtensionID = -1)
		{
			int result;
			if (mapCode <= 0 || !GameManager.MapGridMgr.DictGrids.ContainsKey(mapCode))
			{
				LogManager.WriteLog(LogTypes.Error, string.Format("为RoleID离线挂机时失败, MapCode={0}, RoleID={1}", clientData.MapCode, clientData.RoleID), null, true);
				result = -1;
			}
			else
			{
				FakeRoleManager.RemoveFakeRoleByRoleIDType(clientData.RoleID, fakeRoleType);
				FakeRoleItem fakeRoleItem = FakeRoleManager.AddFakeRole(clientData, fakeRoleType);
				if (null == fakeRoleItem)
				{
					LogManager.WriteLog(LogTypes.Error, string.Format("为RoleID生成假人对象时失败, MapCode={0}, RoleID={1}", clientData.MapCode, clientData.RoleID), null, true);
					result = -1;
				}
				else
				{
					fakeRoleItem.MyRoleDataMini.MapCode = mapCode;
					if (toPosX >= 0 && toPosY >= 0)
					{
						fakeRoleItem.MyRoleDataMini.PosX = toPosX;
						fakeRoleItem.MyRoleDataMini.PosY = toPosY;
					}
					if (direction >= 0)
					{
						fakeRoleItem.MyRoleDataMini.RoleDirection = direction;
					}
					if (ToExtensionID >= 0)
					{
						fakeRoleItem.ToExtensionID = ToExtensionID;
					}
					if (FakeRoleTypes.LiXianGuaJi == fakeRoleType)
					{
						if (clientData.OfflineMarketState <= 0)
						{
							fakeRoleItem.MyRoleDataMini.StallName = "";
						}
					}
					if (FakeRoleTypes.DiaoXiang2 == fakeRoleType)
					{
						if (fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams == null || fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams.Count <= 0)
						{
							int fashionID = 0;
							foreach (FashionData item in FashionManager.getInstance().RuntimeData.FashingDict.Values)
							{
								if (item.Type == 1)
								{
									fashionID = item.ID;
									break;
								}
							}
							if (null == fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams)
							{
								fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams = new List<int>();
							}
							for (int i = fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams.Count; i < 53; i++)
							{
								fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams.Add(0);
							}
							fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams[26] = fashionID;
						}
					}
					if (FakeRoleTypes.BangHuiMatchBZ == fakeRoleType || FakeRoleTypes.CompDaLingZhu_1 == fakeRoleType || FakeRoleTypes.CompDaLingZhu_2 == fakeRoleType || FakeRoleTypes.CompDaLingZhu_3 == fakeRoleType)
					{
						int fashionID = 0;
						foreach (FashionData item in FashionManager.getInstance().RuntimeData.FashingDict.Values)
						{
							if (item.Type == 4)
							{
								fashionID = item.ID;
								break;
							}
						}
						if (fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams == null || fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams.Count <= 0)
						{
							if (null == fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams)
							{
								fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams = new List<int>();
							}
							for (int i = fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams.Count; i < 53; i++)
							{
								fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams.Add(0);
							}
							fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams[26] = fashionID;
						}
						else if (53 == fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams.Count)
						{
							fakeRoleItem.MyRoleDataMini.RoleCommonUseIntPamams[26] = fashionID;
						}
					}
					fakeRoleItem.MyRoleDataMini.LifeV = Math.Max(1, clientData.LifeV);
					fakeRoleItem.MyRoleDataMini.MagicV = Math.Max(1, clientData.MagicV);
					GameManager.MapGridMgr.DictGrids[fakeRoleItem.MyRoleDataMini.MapCode].MoveObject(-1, -1, fakeRoleItem.MyRoleDataMini.PosX, fakeRoleItem.MyRoleDataMini.PosY, fakeRoleItem);
					result = fakeRoleItem.FakeRoleID;
				}
			}
			return result;
		}

		
		public static int ProcessNewFakeRole(RoleData4Selector clientData, int mapCode, FakeRoleTypes fakeRoleType, int direction = -1, int toPosX = -1, int toPosY = -1, int ToExtensionID = -1)
		{
			int result;
			if (mapCode <= 0 || !GameManager.MapGridMgr.DictGrids.ContainsKey(mapCode))
			{
				LogManager.WriteLog(LogTypes.Error, string.Format("生成雕像目标地图不存在, MapCode={0}, RoleID={1}", mapCode, clientData.RoleID), null, true);
				result = -1;
			}
			else
			{
				FakeRoleManager.RemoveFakeRoleByRoleIDType(clientData.RoleID, fakeRoleType);
				FakeRoleItem fakeRoleItem = FakeRoleManager.AddFakeRole(clientData, fakeRoleType);
				if (null == fakeRoleItem)
				{
					LogManager.WriteLog(LogTypes.Error, string.Format("为RoleID生成假人对象时失败, MapCode={0}, RoleID={1}", mapCode, clientData.RoleID), null, true);
					result = -1;
				}
				else
				{
					fakeRoleItem.MyRoleDataMini.MapCode = mapCode;
					if (toPosX >= 0 && toPosY >= 0)
					{
						fakeRoleItem.MyRoleDataMini.PosX = toPosX;
						fakeRoleItem.MyRoleDataMini.PosY = toPosY;
					}
					if (direction >= 0)
					{
						fakeRoleItem.MyRoleDataMini.RoleDirection = direction;
					}
					if (ToExtensionID >= 0)
					{
						fakeRoleItem.ToExtensionID = ToExtensionID;
					}
					GameManager.MapGridMgr.DictGrids[fakeRoleItem.MyRoleDataMini.MapCode].MoveObject(-1, -1, fakeRoleItem.MyRoleDataMini.PosX, fakeRoleItem.MyRoleDataMini.PosY, fakeRoleItem);
					result = fakeRoleItem.FakeRoleID;
				}
			}
			return result;
		}

		
		public static void ProcessDelFakeRoleByType(FakeRoleTypes fakeRoleType, bool bBroadcastDelMsg = false)
		{
			List<FakeRoleItem> fakeRoleItemList = FakeRoleManager.GetFakeRoleListByType(fakeRoleType);
			foreach (FakeRoleItem item in fakeRoleItemList)
			{
				FakeRoleManager.ProcessDelFakeRole(item.FakeRoleID, bBroadcastDelMsg);
			}
		}

		
		public static void ProcessDelFakeRole(int FakeRoleID, bool bBroadcastDelMsg = false)
		{
			FakeRoleItem FakeRoleItem = FakeRoleManager.FindFakeRoleByID(FakeRoleID);
			if (null != FakeRoleItem)
			{
				FakeRoleManager.RemoveFakeRole(FakeRoleID);
				GameManager.MapGridMgr.DictGrids[FakeRoleItem.MyRoleDataMini.MapCode].RemoveObject(FakeRoleItem);
				if (bBroadcastDelMsg)
				{
					GameManager.ClientMgr.NotifyAllDelFakeRole(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, FakeRoleItem);
				}
			}
		}

		
		public static void ProcessFakeRoleGoBack(int FakeRoleID)
		{
			FakeRoleItem fakeRoleItem = FakeRoleManager.FindFakeRoleByID(FakeRoleID);
			if (null != fakeRoleItem)
			{
				GameManager.ClientMgr.NotifyAllDelFakeRole(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, fakeRoleItem);
				int toMapCode = fakeRoleItem.CurrentMapCode;
				GameMap gameMap = null;
				if (GameManager.MapMgr.DictMaps.TryGetValue(toMapCode, out gameMap))
				{
					int defaultBirthPosX = gameMap.DefaultBirthPosX;
					int defaultBirthPosY = gameMap.DefaultBirthPosY;
					int defaultBirthRadius = gameMap.BirthRadius;
					Point newPos = Global.GetMapPoint(ObjectTypes.OT_FAKEROLE, toMapCode, defaultBirthPosX, defaultBirthPosY, defaultBirthRadius);
					int toMapX = (int)newPos.X;
					int toMapY = (int)newPos.Y;
					int oldX = fakeRoleItem.MyRoleDataMini.PosX;
					int oldY = fakeRoleItem.MyRoleDataMini.PosY;
					fakeRoleItem.MyRoleDataMini.PosX = toMapX;
					fakeRoleItem.MyRoleDataMini.PosY = toMapY;
					fakeRoleItem.MyRoleDataMini.LifeV = fakeRoleItem.MyRoleDataMini.MaxLifeV;
					GameManager.MapGridMgr.DictGrids[toMapCode].MoveObject(oldX, oldY, toMapX, toMapY, fakeRoleItem);
				}
			}
		}

		
		public static void ProcessDelFakeRole(int roleID, FakeRoleTypes fakeRoleType)
		{
			FakeRoleItem FakeRoleItem = FakeRoleManager.FindFakeRoleByRoleIDType(roleID, fakeRoleType);
			if (null != FakeRoleItem)
			{
				FakeRoleManager.RemoveFakeRole(FakeRoleItem.FakeRoleID);
				GameManager.MapGridMgr.DictGrids[FakeRoleItem.MyRoleDataMini.MapCode].RemoveObject(FakeRoleItem);
			}
		}

		
		public static void NotifyOthersShowFakeRole(SocketListener sl, TCPOutPacketPool pool, FakeRoleItem FakeRoleItem)
		{
			if (null != FakeRoleItem)
			{
				GameManager.MapGridMgr.DictGrids[FakeRoleItem.MyRoleDataMini.MapCode].MoveObject(-1, -1, FakeRoleItem.MyRoleDataMini.PosX, FakeRoleItem.MyRoleDataMini.PosY, FakeRoleItem);
			}
		}

		
		public static void NotifyOthersHideFakeRole(SocketListener sl, TCPOutPacketPool pool, FakeRoleItem FakeRoleItem)
		{
			if (null != FakeRoleItem)
			{
				GameManager.MapGridMgr.DictGrids[FakeRoleItem.MyRoleDataMini.MapCode].RemoveObject(FakeRoleItem);
			}
		}

		
		private static bool ProcessFakeRoleDead(SocketListener sl, TCPOutPacketPool pool, long nowTicks, FakeRoleItem fakeRoleItem)
		{
			bool result;
			if (fakeRoleItem.CurrentLifeV > 0)
			{
				result = false;
			}
			else
			{
				long subTicks = nowTicks - fakeRoleItem.FakeRoleDeadTicks;
				if (subTicks < 2000L)
				{
					result = false;
				}
				else
				{
					FakeRoleManager.ProcessFakeRoleGoBack(fakeRoleItem.FakeRoleID);
					result = true;
				}
			}
			return result;
		}

		
		public static void ProcessAllFakeRoleItems(SocketListener sl, TCPOutPacketPool pool)
		{
			List<FakeRoleItem> FakeRoleItemList = new List<FakeRoleItem>();
			lock (FakeRoleManager._ID2FakeRoleDict)
			{
				foreach (FakeRoleItem val in FakeRoleManager._ID2FakeRoleDict.Values)
				{
					FakeRoleItemList.Add(val);
				}
			}
			long nowTicks = TimeUtil.NOW();
			for (int i = 0; i < FakeRoleItemList.Count; i++)
			{
				FakeRoleItem FakeRoleItem = FakeRoleItemList[i];
				if (FakeRoleManager.ProcessFakeRoleDead(sl, pool, nowTicks, FakeRoleItem))
				{
				}
			}
		}

		
		public static void SendMySelfFakeRoleItems(SocketListener sl, TCPOutPacketPool pool, GameClient client, List<object> objsList, int totalRoleAndMonsterNum)
		{
			if (null != objsList)
			{
				for (int i = 0; i < objsList.Count; i++)
				{
					FakeRoleItem fakeRoleItem = objsList[i] as FakeRoleItem;
					if (null != fakeRoleItem)
					{
						if (GameManager.TestGameShowFakeRoleForUser || (fakeRoleItem.FakeRoleType != 1 && fakeRoleItem.FakeRoleType != 2))
						{
							if (fakeRoleItem.CurrentLifeV > 0)
							{
								if (totalRoleAndMonsterNum >= 30)
								{
									if (fakeRoleItem.FakeRoleType == 2)
									{
										goto IL_A0;
									}
								}
								GameManager.ClientMgr.NotifyMySelfNewFakeRole(sl, pool, client, fakeRoleItem);
							}
						}
					}
					IL_A0:;
				}
			}
		}

		
		public static void DelMySelfFakeRoleItems(SocketListener sl, TCPOutPacketPool pool, GameClient client, List<object> objsList)
		{
			if (null != objsList)
			{
				for (int i = 0; i < objsList.Count; i++)
				{
					FakeRoleItem fakeRoleItem = objsList[i] as FakeRoleItem;
					if (null != fakeRoleItem)
					{
						if (GameManager.TestGameShowFakeRoleForUser || (fakeRoleItem.FakeRoleType != 1 && fakeRoleItem.FakeRoleType != 2))
						{
							GameManager.ClientMgr.NotifyMySelfDelFakeRole(sl, pool, client, fakeRoleItem.FakeRoleID);
						}
					}
				}
			}
		}

		
		public static void LookupEnemiesInCircle(GameClient client, int mapCode, int toX, int toY, int radius, List<object> enemiesList)
		{
			MapGrid mapGrid = GameManager.MapGridMgr.DictGrids[mapCode];
			List<object> objList = mapGrid.FindObjects(toX, toY, radius);
			if (null != objList)
			{
				Point center = new Point((double)toX, (double)toY);
				for (int i = 0; i < objList.Count; i++)
				{
					if (objList[i] is FakeRoleItem)
					{
						if (client == null || Global.IsOpposition(client, objList[i] as FakeRoleItem))
						{
							if (client == null || client.ClientData.CopyMapID == (objList[i] as FakeRoleItem).CopyMapID)
							{
								Point pt = new Point((double)(objList[i] as FakeRoleItem).MyRoleDataMini.PosX, (double)(objList[i] as FakeRoleItem).MyRoleDataMini.PosY);
								if (Global.InCircle(pt, center, (double)radius))
								{
									enemiesList.Add(objList[i] as FakeRoleItem);
								}
							}
						}
					}
				}
			}
		}

		
		public static void LookupEnemiesInCircleByAngle(GameClient client, int direction, int mapCode, int toX, int toY, int radius, List<int> enemiesList, double angle, bool near180)
		{
			List<object> objList = new List<object>();
			FakeRoleManager.LookupEnemiesInCircleByAngle(client, direction, mapCode, toX, toY, radius, objList, angle, near180);
			for (int i = 0; i < objList.Count; i++)
			{
				enemiesList.Add((objList[i] as FakeRoleItem).FakeRoleID);
			}
		}

		
		public static void LookupEnemiesInCircleByAngle(GameClient client, int direction, int mapCode, int toX, int toY, int radius, List<object> enemiesList, double angle, bool near180)
		{
			MapGrid mapGrid = GameManager.MapGridMgr.DictGrids[mapCode];
			List<object> objList = mapGrid.FindObjects(toX, toY, radius);
			if (null != objList)
			{
				double loAngle = 0.0;
				double hiAngle = 0.0;
				Global.GetAngleRangeByDirection(direction, angle, out loAngle, out hiAngle);
				Point center = new Point((double)toX, (double)toY);
				for (int i = 0; i < objList.Count; i++)
				{
					if (objList[i] is FakeRoleItem)
					{
						if (client == null || Global.IsOpposition(client, objList[i] as FakeRoleItem))
						{
							if (client == null || client.ClientData.CopyMapID == (objList[i] as FakeRoleItem).CopyMapID)
							{
								Point pt = new Point((double)(objList[i] as FakeRoleItem).MyRoleDataMini.PosX, (double)(objList[i] as FakeRoleItem).MyRoleDataMini.PosY);
								if (Global.InCircleByAngle(pt, center, (double)radius, loAngle, hiAngle))
								{
									enemiesList.Add(objList[i]);
								}
							}
						}
					}
				}
			}
		}

		
		public static void LookupRolesInSquare(GameClient client, int mapCode, int radius, int nWidth, List<object> rolesList)
		{
			MapGrid mapGrid = GameManager.MapGridMgr.DictGrids[mapCode];
			List<object> objsList = mapGrid.FindObjects(client.ClientData.PosX, client.ClientData.PosY, radius);
			if (null != objsList)
			{
				Point source = new Point((double)client.ClientData.PosX, (double)client.ClientData.PosY);
				Point toPos = Global.GetAPointInCircle(source, radius, client.ClientData.RoleYAngle);
				int toX = (int)toPos.X;
				int toY = (int)toPos.Y;
				Point center = default(Point);
				center.X = (double)((client.ClientData.PosX + toX) / 2);
				center.Y = (double)((client.ClientData.PosY + toY) / 2);
				int fDirectionX = toX - client.ClientData.PosX;
				int fDirectionY = toY - client.ClientData.PosY;
				for (int i = 0; i < objsList.Count; i++)
				{
					if (objsList[i] is FakeRoleItem)
					{
						if ((objsList[i] as FakeRoleItem).CurrentLifeV > 0)
						{
							if (client == null || client.ClientData.CopyMapID == (objsList[i] as FakeRoleItem).CopyMapID)
							{
								Point target = new Point((objsList[i] as FakeRoleItem).CurrentPos.X, (objsList[i] as FakeRoleItem).CurrentPos.Y);
								if (Global.InSquare(center, target, radius, nWidth, fDirectionX, fDirectionY))
								{
									rolesList.Add(objsList[i]);
								}
								else if (Global.InCircle(target, source, 100.0))
								{
									rolesList.Add(objsList[i]);
								}
							}
						}
					}
				}
			}
		}

		
		public static void LookupEnemiesAtGridXY(IObject attacker, int gridX, int gridY, List<object> enemiesList)
		{
			int mapCode = attacker.CurrentMapCode;
			MapGrid mapGrid = GameManager.MapGridMgr.DictGrids[mapCode];
			List<object> objList = mapGrid.FindObjects(gridX, gridY);
			if (null != objList)
			{
				for (int i = 0; i < objList.Count; i++)
				{
					if (objList[i] is FakeRoleItem)
					{
						if (attacker == null || attacker.CurrentCopyMapID == (objList[i] as FakeRoleItem).CopyMapID)
						{
							enemiesList.Add(objList[i]);
						}
					}
				}
			}
		}

		
		public static void LookupAttackEnemies(IObject attacker, int direction, List<object> enemiesList)
		{
			int mapCode = attacker.CurrentMapCode;
			MapGrid mapGrid = GameManager.MapGridMgr.DictGrids[mapCode];
			Point grid = attacker.CurrentGrid;
			int gridX = (int)grid.X;
			int gridY = (int)grid.Y;
			Point p = Global.GetGridPointByDirection(direction, gridX, gridY);
			FakeRoleManager.LookupEnemiesAtGridXY(attacker, (int)p.X, (int)p.Y, enemiesList);
		}

		
		public static void LookupAttackEnemyIDs(IObject attacker, int direction, List<int> enemiesList)
		{
			List<object> objList = new List<object>();
			FakeRoleManager.LookupAttackEnemies(attacker, direction, objList);
			for (int i = 0; i < objList.Count; i++)
			{
				enemiesList.Add((objList[i] as FakeRoleItem).FakeRoleID);
			}
		}

		
		public static void LookupRangeAttackEnemies(IObject obj, int toX, int toY, int direction, string rangeMode, List<object> enemiesList)
		{
			MapGrid mapGrid = GameManager.MapGridMgr.DictGrids[obj.CurrentMapCode];
			int gridX = toX / mapGrid.MapGridWidth;
			int gridY = toY / mapGrid.MapGridHeight;
			List<Point> gridList = Global.GetGridPointByDirection(direction, gridX, gridY, rangeMode, true);
			if (gridList.Count > 0)
			{
				for (int i = 0; i < gridList.Count; i++)
				{
					FakeRoleManager.LookupEnemiesAtGridXY(obj, (int)gridList[i].X, (int)gridList[i].Y, enemiesList);
				}
			}
		}

		
		public static bool CanAttack(FakeRoleItem enemy)
		{
			return !GameManager.TestGameShowFakeRoleForUser && null != enemy && enemy.GetFakeRoleData().FakeRoleType == 2;
		}

		
		public static int NotifyInjured(SocketListener sl, TCPOutPacketPool pool, GameClient client, FakeRoleItem enemy, int burst, int injure, double injurePercent, int attackType, bool forceBurst, int addInjure, double attackPercent, int addAttackMin, int addAttackMax, double baseRate = 1.0, int addVlue = 0, int nHitFlyDistance = 0)
		{
			int ret = 0;
			if ((enemy as FakeRoleItem).CurrentLifeV > 0)
			{
				injure = 1000;
				injure = (int)((double)injure * injurePercent);
				ret = injure;
				(enemy as FakeRoleItem).CurrentLifeV -= injure;
				(enemy as FakeRoleItem).CurrentLifeV = Global.GMax((enemy as FakeRoleItem).CurrentLifeV, 0);
				int enemyLife = (enemy as FakeRoleItem).CurrentLifeV;
				(enemy as FakeRoleItem).AttackedRoleID = client.ClientData.RoleID;
				GameManager.ClientMgr.SpriteInjure2Blood(sl, pool, client, injure);
				(enemy as FakeRoleItem).AddAttacker(client.ClientData.RoleID, Global.GMax(0, injure));
				GameManager.SystemServerEvents.AddEvent(string.Format("假人减血, Injure={0}, Life={1}", injure, enemyLife), EventLevels.Debug);
				if ((enemy as FakeRoleItem).CurrentLifeV <= 0)
				{
					GameManager.SystemServerEvents.AddEvent(string.Format("假人死亡, roleID={0}", (enemy as FakeRoleItem).FakeRoleID), EventLevels.Debug);
					FakeRoleManager.ProcessFakeRoleDead(sl, pool, client, enemy as FakeRoleItem);
				}
				Point hitToGrid = new Point(-1.0, -1.0);
				if (nHitFlyDistance > 0)
				{
					MapGrid mapGrid = GameManager.MapGridMgr.DictGrids[client.ClientData.MapCode];
					int nGridNum = nHitFlyDistance * 100 / mapGrid.MapGridWidth;
					if (nGridNum > 0)
					{
						hitToGrid = ChuanQiUtils.HitFly(client, enemy, nGridNum);
					}
				}
				if ((enemy as FakeRoleItem).AttackedRoleID >= 0 && (enemy as FakeRoleItem).AttackedRoleID != client.ClientData.RoleID)
				{
					GameClient findClient = GameManager.ClientMgr.FindClient((enemy as FakeRoleItem).AttackedRoleID);
					if (null != findClient)
					{
						GameManager.ClientMgr.NotifySpriteInjured(sl, pool, findClient, findClient.ClientData.MapCode, findClient.ClientData.RoleID, (enemy as FakeRoleItem).FakeRoleID, 0, 0, (double)enemyLife, findClient.ClientData.Level, hitToGrid, 0, EMerlinSecretAttrType.EMSAT_None, 0);
						ClientManager.NotifySelfEnemyInjured(sl, pool, findClient, findClient.ClientData.RoleID, enemy.FakeRoleID, 0, 0, (double)enemyLife, 0L, 0, EMerlinSecretAttrType.EMSAT_None, 0);
					}
				}
				GameManager.ClientMgr.NotifySpriteInjured(sl, pool, client, client.ClientData.MapCode, client.ClientData.RoleID, (enemy as FakeRoleItem).FakeRoleID, burst, injure, (double)enemyLife, client.ClientData.Level, hitToGrid, 0, EMerlinSecretAttrType.EMSAT_None, 0);
				ClientManager.NotifySelfEnemyInjured(sl, pool, client, client.ClientData.RoleID, enemy.FakeRoleID, burst, injure, (double)enemyLife, 0L, 0, EMerlinSecretAttrType.EMSAT_None, 0);
				if (!client.ClientData.DisableChangeRolePurpleName)
				{
					GameManager.ClientMgr.ForceChangeRolePurpleName2(sl, pool, client);
				}
			}
			return ret;
		}

		
		public static void NotifyInjured(SocketListener sl, TCPOutPacketPool pool, GameClient client, int roleID, int enemy, int enemyX, int enemyY, int burst, int injure, double attackPercent, int addAttack, double baseRate = 1.0, int addVlue = 0, int nHitFlyDistance = 0)
		{
			object obj = FakeRoleManager.FindFakeRoleByID(enemy);
			if (null != obj)
			{
				if ((obj as FakeRoleItem).CurrentLifeV > 0)
				{
					injure = 10000;
					(obj as FakeRoleItem).CurrentLifeV -= injure;
					(obj as FakeRoleItem).CurrentLifeV = Global.GMax((obj as FakeRoleItem).CurrentLifeV, 0);
					int enemyLife = (obj as FakeRoleItem).CurrentLifeV;
					(obj as FakeRoleItem).AttackedRoleID = client.ClientData.RoleID;
					GameManager.ClientMgr.SpriteInjure2Blood(sl, pool, client, injure);
					GameManager.SystemServerEvents.AddEvent(string.Format("假人减血, Injure={0}, Life={1}", injure, enemyLife), EventLevels.Debug);
					if ((obj as FakeRoleItem).CurrentLifeV <= 0)
					{
						GameManager.SystemServerEvents.AddEvent(string.Format("假人死亡, roleID={0}", (obj as FakeRoleItem).FakeRoleID), EventLevels.Debug);
						FakeRoleManager.ProcessFakeRoleDead(sl, pool, client, obj as FakeRoleItem);
					}
					int ownerRoleID = (obj as FakeRoleItem).GetAttackerFromList();
					if (ownerRoleID >= 0 && ownerRoleID != client.ClientData.RoleID)
					{
						GameClient findClient = GameManager.ClientMgr.FindClient(ownerRoleID);
						if (null != findClient)
						{
							GameManager.ClientMgr.NotifySpriteInjured(sl, pool, findClient, findClient.ClientData.MapCode, findClient.ClientData.RoleID, (obj as FakeRoleItem).FakeRoleID, 0, 0, (double)enemyLife, findClient.ClientData.Level, new Point(-1.0, -1.0), 0, EMerlinSecretAttrType.EMSAT_None, 0);
							ClientManager.NotifySelfEnemyInjured(sl, pool, findClient, findClient.ClientData.RoleID, (obj as FakeRoleItem).FakeRoleID, 0, 0, (double)enemyLife, 0L, 0, EMerlinSecretAttrType.EMSAT_None, 0);
						}
					}
					GameManager.ClientMgr.NotifySpriteInjured(sl, pool, client, client.ClientData.MapCode, client.ClientData.RoleID, (obj as FakeRoleItem).FakeRoleID, burst, injure, (double)enemyLife, client.ClientData.Level, new Point(-1.0, -1.0), 0, EMerlinSecretAttrType.EMSAT_None, 0);
					ClientManager.NotifySelfEnemyInjured(sl, pool, client, client.ClientData.RoleID, (obj as FakeRoleItem).FakeRoleID, burst, injure, (double)enemyLife, 0L, 0, EMerlinSecretAttrType.EMSAT_None, 0);
					if (!client.ClientData.DisableChangeRolePurpleName)
					{
						GameManager.ClientMgr.ForceChangeRolePurpleName2(sl, pool, client);
					}
				}
			}
		}

		
		private static void ProcessFakeRoleDead(SocketListener sl, TCPOutPacketPool pool, GameClient client, FakeRoleItem fakeRoleItem)
		{
			if (!fakeRoleItem.HandledDead)
			{
				fakeRoleItem.HandledDead = true;
				fakeRoleItem.FakeRoleDeadTicks = TimeUtil.NOW();
				int ownerRoleID = fakeRoleItem.GetAttackerFromList();
				if (ownerRoleID >= 0 && ownerRoleID != client.ClientData.RoleID)
				{
					GameClient findClient = GameManager.ClientMgr.FindClient(ownerRoleID);
					if (null != findClient)
					{
						client = findClient;
					}
				}
			}
		}

		
		private static Dictionary<int, FakeRoleItem> _ID2FakeRoleDict = new Dictionary<int, FakeRoleItem>();

		
		private static Dictionary<string, FakeRoleItem> _RoleIDType2FakeRoleDict = new Dictionary<string, FakeRoleItem>();
	}
}
