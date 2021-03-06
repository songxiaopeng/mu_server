﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;
using GameServer.Core.GameEvent;
using GameServer.Core.GameEvent.EventOjectImpl;
using GameServer.Logic.Marriage.CoupleArena;
using GameServer.Server;
using Server.Data;
using Server.Tools;
using Server.Tools.Pattern;

namespace GameServer.Logic.Ornament
{
	
	public class OrnamentManager : IManager, ICmdProcessorEx, ICmdProcessor, IEventListener
	{
		
		public static OrnamentManager getInstance()
		{
			return OrnamentManager.instance;
		}

		
		public bool initialize()
		{
			this.evHandlerDict = new Dictionary<OrnamentGoalType, Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>>();
			this.evHandlerDict[OrnamentGoalType.OGT_Talent] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_Talent);
			this.evHandlerDict[OrnamentGoalType.OGT_KingOfBattle] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_KingOfBattle);
			this.evHandlerDict[OrnamentGoalType.OGT_YongZheZhanChang] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_YongZheZhanChang);
			this.evHandlerDict[OrnamentGoalType.OGT_HuanYingSiYuan] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_HuanYingSiYuan);
			this.evHandlerDict[OrnamentGoalType.OGT_JingJiChallenge] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_JingJiChallenge);
			this.evHandlerDict[OrnamentGoalType.OGT_BHMatchGoldChampion] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_BHMatchGoldChampion);
			this.evHandlerDict[OrnamentGoalType.OGT_BHMatchJoin] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_BHMatchJoin);
			this.evHandlerDict[OrnamentGoalType.OGT_BHMatchGoldMVP] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_BHMatchGoldMVP);
			this.evHandlerDict[OrnamentGoalType.OGT_BHMatchWin] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_BHMatchWin);
			this.evHandlerDict[OrnamentGoalType.OGT_KingOfBattleMVP] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_KingOfBattleMVP);
			this.evHandlerDict[OrnamentGoalType.OGT_YongZheZhanChangMVP] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_YongZheZhanChangMVP);
			this.evHandlerDict[OrnamentGoalType.OGT_TianTiPT] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_TianTiPT);
			this.evHandlerDict[OrnamentGoalType.OGT_TianTiDiamond] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_TianTiDiamond);
			this.evHandlerDict[OrnamentGoalType.OGT_CoupleArenaDuanWei] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_CoupleArenaDuanWei);
			this.evHandlerDict[OrnamentGoalType.OGT_KuaFuLueDuo_Attacker] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_GoalAddNum);
			this.evHandlerDict[OrnamentGoalType.OGT_KuaFuLueDuo_Defender] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_GoalAddNum);
			this.evHandlerDict[OrnamentGoalType.OGT_EscapeRoleKill] = new Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>(this._Handle_EscapeKillRole);
			return this.InitConfig();
		}

		
		public bool startup()
		{
			TCPCmdDispatcher.getInstance().registerProcessorEx(1615, 1, 1, OrnamentManager.getInstance(), TCPCmdFlags.IsStringArrayParams);
			TCPCmdDispatcher.getInstance().registerProcessorEx(1616, 2, 2, OrnamentManager.getInstance(), TCPCmdFlags.IsStringArrayParams);
			TCPCmdDispatcher.getInstance().registerProcessorEx(1617, 3, 3, OrnamentManager.getInstance(), TCPCmdFlags.IsStringArrayParams);
			TCPCmdDispatcher.getInstance().registerProcessorEx(1618, 1, 1, OrnamentManager.getInstance(), TCPCmdFlags.IsStringArrayParams);
			GlobalEventSource.getInstance().registerListener(37, OrnamentManager.getInstance());
			GlobalEventSource.getInstance().registerListener(14, OrnamentManager.getInstance());
			return true;
		}

		
		public bool showdown()
		{
			GlobalEventSource.getInstance().removeListener(37, OrnamentManager.getInstance());
			GlobalEventSource.getInstance().removeListener(14, OrnamentManager.getInstance());
			return true;
		}

		
		public bool destroy()
		{
			return true;
		}

		
		public bool processCmd(GameClient client, string[] cmdParams)
		{
			return false;
		}

		
		public bool processCmdEx(GameClient client, int nID, byte[] bytes, string[] cmdParams)
		{
			bool result;
			if (!GlobalNew.IsGongNengOpened(client, GongNengIDs.Ornament, false))
			{
				GameManager.ClientMgr.NotifyImportantMsg(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, client, StringUtil.substitute(GLang.GetLang(512, new object[0]), new object[0]), GameInfoTypeIndexes.Error, ShowGameInfoTypes.ErrAndBox, 0);
				result = true;
			}
			else
			{
				switch (nID)
				{
				case 1615:
					result = this.ProcessOrnamentGetDataCmd(client, nID, bytes, cmdParams);
					break;
				case 1616:
					result = this.ProcessOrnamentSlotForgeCmd(client, nID, bytes, cmdParams);
					break;
				case 1617:
					result = this.ProcessOrnamentActiveCmd(client, nID, bytes, cmdParams);
					break;
				case 1618:
					result = this.ProcessOrnamentGetGoodsListCmd(client, nID, bytes, cmdParams);
					break;
				default:
					result = true;
					break;
				}
			}
			return result;
		}

		
		public void OnLogin(GameClient client)
		{
			bool isDisable = GameFuncControlManager.IsGameFuncDisabled(GameFuncType.System2Dot2);
			bool isOpen = GlobalNew.IsGongNengOpened(client, GongNengIDs.Ornament, false);
			if (isOpen && !isDisable)
			{
				GoodsData goodsData = client.UsingEquipMgr.GetGoodsDataByCategoriy(client, 23);
				if (goodsData != null && goodsData.Site == 0)
				{
					if (!Global.CanAddGoods(client, goodsData.GoodsID, 1, goodsData.Binding, "1900-01-01 12:00:00", true, false))
					{
						if (Global.UseMailGivePlayerAward(client, goodsData, GLang.GetLang(513, new object[0]), GLang.GetLang(514, new object[0]), 1.0))
						{
							Global.DestroyGoods(client, goodsData);
						}
					}
					else
					{
						string cmdData = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", new object[]
						{
							client.ClientData.RoleID,
							2,
							goodsData.Id,
							goodsData.GoodsID,
							0,
							goodsData.Site,
							goodsData.GCount,
							goodsData.BagIndex,
							""
						});
						Global.ModifyGoodsByCmdParams(client, cmdData, "客户端修改", null);
					}
				}
				GlobalEventSource.getInstance().fireEvent(new OrnamentGoalEventObject(client, OrnamentGoalType.OGT_Talent, new int[0]));
				GlobalEventSource.getInstance().fireEvent(new OrnamentGoalEventObject(client, OrnamentGoalType.OGT_BHMatchJoin, new int[0]));
				GlobalEventSource.getInstance().fireEvent(new OrnamentGoalEventObject(client, OrnamentGoalType.OGT_BHMatchGoldMVP, new int[0]));
				GlobalEventSource.getInstance().fireEvent(new OrnamentGoalEventObject(client, OrnamentGoalType.OGT_BHMatchWin, new int[0]));
				GlobalEventSource.getInstance().fireEvent(new OrnamentGoalEventObject(client, OrnamentGoalType.OGT_EscapeRoleKill, new int[0]));
				GlobalEventSource.getInstance().fireEvent(new OrnamentGoalEventObject(client, OrnamentGoalType.OGT_TianTiPT, new int[0]));
				GlobalEventSource.getInstance().fireEvent(new OrnamentGoalEventObject(client, OrnamentGoalType.OGT_TianTiDiamond, new int[0]));
				this.InitOrnamentSlotData(client);
				this.RefreshOrnamentProps(client);
				this.HandleBHMatchGoldAccident(client);
			}
		}

		
		private void HandleBHMatchGoldAccident(GameClient client)
		{
			Dictionary<int, OrnamentData> OrnamentDataDict = client.ClientData.OrnamentDataDict;
			lock (OrnamentDataDict)
			{
				List<int> tmpGoalIdList = null;
				Dictionary<int, OrnamentConfigData> tmpGoalConfigDict = null;
				lock (this.ConfigMutex)
				{
					if (!this.Func2GoalId.TryGetValue(OrnamentGoalType.OGT_BHMatchGoldChampion, out tmpGoalIdList) || tmpGoalIdList.Count <= 0)
					{
						return;
					}
					if ((tmpGoalConfigDict = this.OrnamentConfig) == null || tmpGoalConfigDict.Count <= 0)
					{
						return;
					}
				}
				foreach (int goalId in tmpGoalIdList)
				{
					OrnamentData itemData = null;
					if (OrnamentDataDict.TryGetValue(goalId, out itemData))
					{
						OrnamentConfigData itemConfig = null;
						if (tmpGoalConfigDict.TryGetValue(goalId, out itemConfig))
						{
							List<int> roleAnalysisExData = BangHuiMatchManager.getInstance().GetBHMatchRAnalysisExData(client);
							if (itemData.Param1 > roleAnalysisExData[0])
							{
								if (itemData.Param1 >= itemConfig.GoalNum)
								{
									GoodsData goodsData = this.GetOrnamentGoodsDataByGoodsID(client, itemData.ID);
									if (null != goodsData)
									{
										Global.DestroyGoods(client, goodsData);
									}
								}
								itemData.Param1 = roleAnalysisExData[0];
								this.UpdateDb(client.ClientData.RoleID, itemData, client.ServerId);
							}
						}
					}
				}
			}
		}

		
		public int GetOrnamentCharmPoint(GoodsData goodsData)
		{
			bool isDisable = GameFuncControlManager.IsGameFuncDisabled(GameFuncType.System2Dot2);
			int result;
			if (isDisable)
			{
				result = 0;
			}
			else if (null == goodsData)
			{
				result = 0;
			}
			else if (goodsData.GCount <= 0)
			{
				result = 0;
			}
			else
			{
				Dictionary<int, OrnamentConfigData> tempOrnamentConfig = null;
				lock (this.ConfigMutex)
				{
					tempOrnamentConfig = this.OrnamentConfig;
				}
				OrnamentConfigData oConfigData = null;
				if (!tempOrnamentConfig.TryGetValue(goodsData.GoodsID, out oConfigData))
				{
					result = 0;
				}
				else
				{
					result = oConfigData.RecoverPoints * goodsData.GCount;
				}
			}
			return result;
		}

		
		private void InitOrnamentSlotData(GameClient client)
		{
			OrnamentData oData = null;
			if (!client.ClientData.OrnamentDataDict.TryGetValue(1, out oData))
			{
				oData = new OrnamentData();
				oData.ID = 1;
				oData.Param1 = 1;
				client.ClientData.OrnamentDataDict[oData.ID] = oData;
				this.UpdateDb(client.ClientData.RoleID, oData, client.ServerId);
			}
		}

		
		private int CalcOrnamentSlotForgeTotalLev(GameClient client)
		{
			int result = 0;
			foreach (KeyValuePair<int, OrnamentData> kvp in client.ClientData.OrnamentDataDict)
			{
				if (kvp.Value.ID > 0 && kvp.Value.ID < 6)
				{
					result += kvp.Value.Param1;
				}
			}
			return result;
		}

		
		private void TryActiveOrnamentSlot(GameClient client)
		{
			int totalLev = this.CalcOrnamentSlotForgeTotalLev(client);
			List<int> tempOrnamentSlotOpenConfig = null;
			lock (this.ConfigMutex)
			{
				tempOrnamentSlotOpenConfig = this.OrnamentSlotOpenConfig;
			}
			int SlotOpenIndexMax = 0;
			for (int SlotIndex = 0; SlotIndex < tempOrnamentSlotOpenConfig.Count; SlotIndex++)
			{
				if (totalLev >= tempOrnamentSlotOpenConfig[SlotIndex])
				{
					SlotOpenIndexMax = SlotIndex + 1;
				}
			}
			if (SlotOpenIndexMax > 0 && SlotOpenIndexMax < 6)
			{
				OrnamentData oData = null;
				if (!client.ClientData.OrnamentDataDict.TryGetValue(SlotOpenIndexMax, out oData))
				{
					oData = new OrnamentData();
					oData.ID = SlotOpenIndexMax;
					oData.Param1 = 1;
					client.ClientData.OrnamentDataDict[oData.ID] = oData;
					this.UpdateDb(client.ClientData.RoleID, oData, client.ServerId);
				}
			}
		}

		
		private bool UpdateDb(int roleid, OrnamentData itemData, int serverId)
		{
			bool result;
			if (!Global.sendToDB<bool, OrnamentUpdateDbData>(13223, new OrnamentUpdateDbData
			{
				RoleId = roleid,
				Data = itemData
			}, serverId))
			{
				LogManager.WriteLog(LogTypes.Error, string.Format("饰品系统更新玩家数据失败, roleid={0}, id={1}", roleid, itemData.ID), null, true);
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		
		public int _CanUsingOrnament(GameClient client, int toBagIndex, List<GoodsData> usingList)
		{
			int i = 0;
			while (usingList != null && i < usingList.Count)
			{
				if (toBagIndex == usingList[i].BagIndex)
				{
					return -4;
				}
				i++;
			}
			if (toBagIndex <= 0 || toBagIndex >= 6)
			{
				return -55;
			}
			OrnamentData oData = null;
			if (!client.ClientData.OrnamentDataDict.TryGetValue(toBagIndex, out oData))
			{
				return -55;
			}
			return 0;
		}

		
		public GoodsData GetOrnamentGoodsDataByDbID(GameClient client, int id)
		{
			GoodsData result;
			if (null == client.ClientData.OrnamentGoodsDataList)
			{
				result = null;
			}
			else
			{
				lock (client.ClientData.OrnamentGoodsDataList)
				{
					for (int i = 0; i < client.ClientData.OrnamentGoodsDataList.Count; i++)
					{
						if (client.ClientData.OrnamentGoodsDataList[i].Id == id)
						{
							return client.ClientData.OrnamentGoodsDataList[i];
						}
					}
				}
				result = null;
			}
			return result;
		}

		
		public GoodsData GetOrnamentGoodsDataByGoodsID(GameClient client, int id)
		{
			GoodsData result;
			if (null == client.ClientData.OrnamentGoodsDataList)
			{
				result = null;
			}
			else
			{
				lock (client.ClientData.OrnamentGoodsDataList)
				{
					for (int i = 0; i < client.ClientData.OrnamentGoodsDataList.Count; i++)
					{
						if (client.ClientData.OrnamentGoodsDataList[i].GoodsID == id)
						{
							return client.ClientData.OrnamentGoodsDataList[i];
						}
					}
				}
				result = null;
			}
			return result;
		}

		
		public GoodsData AddOrnamentGoodsData(GameClient client, int id, int goodsID, int forgeLevel, int quality, int goodsNum, int binding, int site, string jewelList, string endTime, int addPropIndex, int bornIndex, int lucky, int strong, int ExcellenceProperty, int nAppendPropLev, int nEquipChangeLife)
		{
			GoodsData gd = new GoodsData
			{
				Id = id,
				GoodsID = goodsID,
				Using = 0,
				Forge_level = forgeLevel,
				Starttime = "1900-01-01 12:00:00",
				Endtime = endTime,
				Site = site,
				Quality = quality,
				Props = "",
				GCount = goodsNum,
				Binding = binding,
				Jewellist = jewelList,
				BagIndex = 0,
				AddPropIndex = addPropIndex,
				BornIndex = bornIndex,
				Lucky = lucky,
				Strong = strong,
				ExcellenceInfo = ExcellenceProperty,
				AppendPropLev = nAppendPropLev,
				ChangeLifeLevForEquip = nEquipChangeLife
			};
			this.AddOrnamentGoodsData(client, gd);
			return gd;
		}

		
		public void AddOrnamentGoodsData(GameClient client, GoodsData goodsData)
		{
			if (goodsData.Site == 9000)
			{
				if (null == client.ClientData.OrnamentGoodsDataList)
				{
					client.ClientData.OrnamentGoodsDataList = new List<GoodsData>();
				}
				lock (client.ClientData.OrnamentGoodsDataList)
				{
					client.ClientData.OrnamentGoodsDataList.Add(goodsData);
				}
				this.RefreshOrnamentProps(client);
			}
		}

		
		public bool OrnamentCanSaleBack(GameClient client, int GoodsID)
		{
			bool isDisable = GameFuncControlManager.IsGameFuncDisabled(GameFuncType.System2Dot2);
			bool result;
			if (isDisable)
			{
				result = false;
			}
			else
			{
				int nCategories = Global.GetGoodsCatetoriy(GoodsID);
				if (nCategories != 23)
				{
					result = true;
				}
				else if (client.ClientData.OrnamentGoodsDataList == null)
				{
					result = false;
				}
				else
				{
					List<GoodsData> ornamentGoodsDataList;
					lock (client.ClientData.OrnamentGoodsDataList)
					{
						ornamentGoodsDataList = new List<GoodsData>(client.ClientData.OrnamentGoodsDataList);
					}
					foreach (GoodsData item in ornamentGoodsDataList)
					{
						if (item.GoodsID == GoodsID)
						{
							return true;
						}
					}
					result = false;
				}
			}
			return result;
		}

		
		public bool OrnamentCanAdd(GameClient client, int GoodsID)
		{
			int nCategories = Global.GetGoodsCatetoriy(GoodsID);
			bool result;
			if (nCategories != 23)
			{
				result = false;
			}
			else if (client.ClientData.OrnamentGoodsDataList == null)
			{
				result = true;
			}
			else
			{
				List<GoodsData> ornamentGoodsDataList;
				lock (client.ClientData.OrnamentGoodsDataList)
				{
					ornamentGoodsDataList = new List<GoodsData>(client.ClientData.OrnamentGoodsDataList);
				}
				foreach (GoodsData item in ornamentGoodsDataList)
				{
					if (item.GoodsID == GoodsID)
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		
		public void RemoveOrnamentGoodsData(GameClient client, GoodsData goodsData)
		{
			if (null != client.ClientData.OrnamentGoodsDataList)
			{
				if (null != goodsData)
				{
					lock (client.ClientData.OrnamentGoodsDataList)
					{
						EquipPropItem item = GameManager.EquipPropsMgr.FindEquipPropItem(goodsData.GoodsID);
						if (null != item)
						{
							client.ClientData.PropsCacheManager.SetExtProps(new object[]
							{
								PropsSystemTypes.OrnamentGoodsProps,
								goodsData.GoodsID,
								PropsCacheManager.ConstExtProps
							});
						}
						client.ClientData.OrnamentGoodsDataList.Remove(goodsData);
					}
					this.RefreshOrnamentProps(client);
				}
			}
		}

		
		public void RefreshOrnamentProps(GameClient client)
		{
			lock (client.ClientData.OrnamentGoodsDataList)
			{
				foreach (GoodsData goodsData in client.ClientData.OrnamentGoodsDataList)
				{
                    EquipPropItem item = GameManager.EquipPropsMgr.FindEquipPropItem(goodsData.GoodsID);
					if (null != item)
					{
						client.ClientData.PropsCacheManager.SetExtProps(new object[]
						{
							PropsSystemTypes.OrnamentGoodsProps,
							goodsData.GoodsID,
							PropsCacheManager.ConstExtProps
						});
					}
				}
				foreach (GoodsData goodsData in client.ClientData.OrnamentGoodsDataList)
				{
                    EquipPropItem item = GameManager.EquipPropsMgr.FindEquipPropItem(goodsData.GoodsID);
					if (null != item)
					{
						double[] CalExtProps = new double[177];
						if (goodsData.Using <= 0)
						{
							for (int i = 0; i < CalExtProps.Length; i++)
							{
								CalExtProps[i] = item.ExtProps[i];
							}
						}
						else
						{
							OrnamentData itemData = null;
							if (!client.ClientData.OrnamentDataDict.TryGetValue(goodsData.BagIndex, out itemData))
							{
								continue;
							}
							for (int i = 0; i < CalExtProps.Length; i++)
							{
								CalExtProps[i] = ((double)itemData.Param1 * 0.2 + 0.8) * item.ExtProps[i];
							}
						}
						client.ClientData.PropsCacheManager.SetExtProps(new object[]
						{
							PropsSystemTypes.OrnamentGoodsProps,
							goodsData.GoodsID,
							CalExtProps
						});
					}
				}
				List<OrnamentGroupConfigData> tempOrnamentGroupConfig = null;
				lock (this.ConfigMutex)
				{
					tempOrnamentGroupConfig = this.OrnamentGroupConfig;
				}
				foreach (OrnamentGroupConfigData data in tempOrnamentGroupConfig)
				{
					OrnamentGroupConfigData groupData = data;
					client.ClientData.PropsCacheManager.SetExtProps(new object[]
					{
						PropsSystemTypes.OrnamentGroupProps,
						groupData.ID,
						PropsCacheManager.ConstExtProps
					});
				}
				foreach (OrnamentGroupConfigData data in tempOrnamentGroupConfig)
				{
					bool AddExtProps = true;
					OrnamentGroupConfigData groupData = data;
					for (int i = 0; i < groupData.GoodsIDList.Count; i++)
					{
						GoodsData goodsData = this.GetOrnamentGoodsDataByGoodsID(client, groupData.GoodsIDList[i]);
						if (goodsData == null || goodsData.Using <= 0)
						{
							AddExtProps = false;
							break;
						}
					}
					if (AddExtProps)
					{
						client.ClientData.PropsCacheManager.SetExtProps(new object[]
						{
							PropsSystemTypes.OrnamentGroupProps,
							groupData.ID,
							groupData.ExtProps
						});
					}
				}
			}
			GameManager.ClientMgr.NotifyUpdateEquipProps(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, client);
		}

		
		private bool CheckCanActiveChengJiuOrnament(GameClient client, OrnamentData data, OrnamentConfigData itemConfig)
		{
			bool result;
			switch (itemConfig.GoalType)
			{
			case OrnamentGoalType.OGT_Talent:
			case OrnamentGoalType.OGT_BHMatchJoin:
			case OrnamentGoalType.OGT_BHMatchGoldMVP:
			case OrnamentGoalType.OGT_BHMatchWin:
			case OrnamentGoalType.OGT_EscapeRoleKill:
				result = (data.Param1 >= itemConfig.GoalNum);
				break;
			case OrnamentGoalType.OGT_KingOfBattle:
			case OrnamentGoalType.OGT_YongZheZhanChang:
			case OrnamentGoalType.OGT_HuanYingSiYuan:
			case OrnamentGoalType.OGT_JingJiChallenge:
			case OrnamentGoalType.OGT_BHMatchGoldChampion:
			case OrnamentGoalType.OGT_KingOfBattleMVP:
			case OrnamentGoalType.OGT_YongZheZhanChangMVP:
			case OrnamentGoalType.OGT_TianTiPT:
			case OrnamentGoalType.OGT_TianTiDiamond:
			case OrnamentGoalType.OGT_CoupleArenaDuanWei:
			case OrnamentGoalType.OGT_KuaFuLueDuo_Defender:
			case OrnamentGoalType.OGT_KuaFuLueDuo_Attacker:
				result = (data.Param1 >= itemConfig.GoalNum);
				break;
			default:
				result = false;
				break;
			}
			return result;
		}

		
		public void processEvent(EventObject eventObject)
		{
			if (eventObject.getEventType() == 14)
			{
				GameClient client = (eventObject as PlayerInitGameEventObject).getPlayer();
				this.OnLogin(client);
			}
			else if (eventObject.getEventType() == 37)
			{
				OrnamentGoalEventObject evObj = eventObject as OrnamentGoalEventObject;
				try
				{
					bool isDisable = GameFuncControlManager.IsGameFuncDisabled(GameFuncType.System2Dot2);
					bool isOpen = GlobalNew.IsGongNengOpened(evObj.Client, GongNengIDs.Ornament, false);
					if (isOpen && !isDisable)
					{
						Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>> handler = null;
						if (this.evHandlerDict.TryGetValue(evObj.FuncType, out handler))
						{
							List<int> tmpGoalIdList = null;
							Dictionary<int, OrnamentConfigData> tmpGoalConfigDict = null;
							lock (this.ConfigMutex)
							{
								if (!this.Func2GoalId.TryGetValue(evObj.FuncType, out tmpGoalIdList) || tmpGoalIdList.Count <= 0)
								{
									return;
								}
								if ((tmpGoalConfigDict = this.OrnamentConfig) == null || tmpGoalConfigDict.Count <= 0)
								{
									return;
								}
							}
							handler(evObj, tmpGoalIdList, tmpGoalConfigDict);
						}
					}
				}
				catch (Exception ex)
				{
					LogManager.WriteLog(LogTypes.Error, "OrnamentManager.processEvent [OrnamentGoal]", ex, true);
				}
			}
		}

		
		private void _Handle_Talent(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						itemData.Param1 = evObj.Client.ClientData.MyTalentData.TotalCount;
					}
				}
			}
		}

		
		private void _Handle_GoalAddNum(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						OrnamentConfigData itemConfig = null;
						if (goalConfigDict.TryGetValue(goalId, out itemConfig))
						{
							if (itemData.Param1 < itemConfig.GoalNum)
							{
								itemData.Param1 += evObj.Arg1;
								if (!this.UpdateDb(evObj.Client.ClientData.RoleID, itemData, evObj.Client.ServerId))
								{
									itemData.Param1--;
								}
							}
						}
					}
				}
			}
		}

		
		private void _Handle_KingOfBattle(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						OrnamentConfigData itemConfig = null;
						if (goalConfigDict.TryGetValue(goalId, out itemConfig))
						{
							if (itemData.Param1 < itemConfig.GoalNum)
							{
								itemData.Param1++;
								if (!this.UpdateDb(evObj.Client.ClientData.RoleID, itemData, evObj.Client.ServerId))
								{
									itemData.Param1--;
								}
							}
						}
					}
				}
			}
		}

		
		private void _Handle_YongZheZhanChang(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						OrnamentConfigData itemConfig = null;
						if (goalConfigDict.TryGetValue(goalId, out itemConfig))
						{
							if (itemData.Param1 < itemConfig.GoalNum)
							{
								itemData.Param1++;
								if (!this.UpdateDb(evObj.Client.ClientData.RoleID, itemData, evObj.Client.ServerId))
								{
									itemData.Param1--;
								}
							}
						}
					}
				}
			}
		}

		
		private void _Handle_HuanYingSiYuan(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						OrnamentConfigData itemConfig = null;
						if (goalConfigDict.TryGetValue(goalId, out itemConfig))
						{
							if (itemData.Param1 < itemConfig.GoalNum)
							{
								itemData.Param1++;
								if (!this.UpdateDb(evObj.Client.ClientData.RoleID, itemData, evObj.Client.ServerId))
								{
									itemData.Param1--;
								}
							}
						}
					}
				}
			}
		}

		
		private void _Handle_JingJiChallenge(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						OrnamentConfigData itemConfig = null;
						if (goalConfigDict.TryGetValue(goalId, out itemConfig))
						{
							if (itemData.Param1 < itemConfig.GoalNum)
							{
								itemData.Param1++;
								if (!this.UpdateDb(evObj.Client.ClientData.RoleID, itemData, evObj.Client.ServerId))
								{
									itemData.Param1--;
								}
							}
						}
					}
				}
			}
		}

		
		private void _Handle_BHMatchGoldChampion(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						OrnamentConfigData itemConfig = null;
						if (goalConfigDict.TryGetValue(goalId, out itemConfig))
						{
							if (itemData.Param1 < itemConfig.GoalNum)
							{
								itemData.Param1++;
								if (!this.UpdateDb(evObj.Client.ClientData.RoleID, itemData, evObj.Client.ServerId))
								{
									itemData.Param1--;
								}
							}
						}
					}
				}
			}
		}

		
		private void _Handle_BHMatchJoin(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						List<int> RoleAnalysisData = BangHuiMatchManager.getInstance().GetBHMatchRoleAnalysisData(evObj.Client);
						if (null != RoleAnalysisData)
						{
							itemData.Param1 = RoleAnalysisData[11];
						}
					}
				}
			}
		}

		
		private void _Handle_EscapeKillRole(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						List<int> RoleAnalysisData = EscapeBattleManager.getInstance().GetEscapeBattleRoleAnalysisData(evObj.Client);
						if (null != RoleAnalysisData)
						{
							itemData.Param1 = RoleAnalysisData[2];
						}
					}
				}
			}
		}

		
		private void _Handle_BHMatchGoldMVP(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						List<int> RoleAnalysisData = BangHuiMatchManager.getInstance().GetBHMatchRoleAnalysisData(evObj.Client);
						if (null != RoleAnalysisData)
						{
							itemData.Param1 = RoleAnalysisData[2];
						}
					}
				}
			}
		}

		
		private void _Handle_BHMatchWin(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						List<int> RoleAnalysisData = BangHuiMatchManager.getInstance().GetBHMatchRoleAnalysisData(evObj.Client);
						if (null != RoleAnalysisData)
						{
							itemData.Param1 = RoleAnalysisData[10];
						}
					}
				}
			}
		}

		
		private void _Handle_KingOfBattleMVP(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						OrnamentConfigData itemConfig = null;
						if (goalConfigDict.TryGetValue(goalId, out itemConfig))
						{
							if (itemData.Param1 < itemConfig.GoalNum)
							{
								itemData.Param1++;
								if (!this.UpdateDb(evObj.Client.ClientData.RoleID, itemData, evObj.Client.ServerId))
								{
									itemData.Param1--;
								}
							}
						}
					}
				}
			}
		}

		
		private void _Handle_YongZheZhanChangMVP(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						OrnamentConfigData itemConfig = null;
						if (goalConfigDict.TryGetValue(goalId, out itemConfig))
						{
							if (itemData.Param1 < itemConfig.GoalNum)
							{
								itemData.Param1++;
								if (!this.UpdateDb(evObj.Client.ClientData.RoleID, itemData, evObj.Client.ServerId))
								{
									itemData.Param1--;
								}
							}
						}
					}
				}
			}
		}

		
		private void _Handle_TianTiPT(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						if (evObj.Client.ClientData.TianTiData.DuanWeiId >= 20)
						{
							OrnamentConfigData itemConfig = null;
							if (goalConfigDict.TryGetValue(goalId, out itemConfig))
							{
								if (itemData.Param1 < itemConfig.GoalNum)
								{
									itemData.Param1 = itemConfig.GoalNum;
									if (!this.UpdateDb(evObj.Client.ClientData.RoleID, itemData, evObj.Client.ServerId))
									{
										itemData.Param1 = 0;
									}
								}
							}
						}
					}
				}
			}
		}

		
		private void _Handle_TianTiDiamond(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						if (evObj.Client.ClientData.TianTiData.DuanWeiId >= 25)
						{
							OrnamentConfigData itemConfig = null;
							if (goalConfigDict.TryGetValue(goalId, out itemConfig))
							{
								if (itemData.Param1 < itemConfig.GoalNum)
								{
									itemData.Param1 = itemConfig.GoalNum;
									if (!this.UpdateDb(evObj.Client.ClientData.RoleID, itemData, evObj.Client.ServerId))
									{
										itemData.Param1 = 0;
									}
								}
							}
						}
					}
				}
			}
		}

		
		private void _Handle_CoupleArenaDuanWei(OrnamentGoalEventObject evObj, List<int> goalIdList, Dictionary<int, OrnamentConfigData> goalConfigDict)
		{
			if (evObj != null && evObj.Client != null)
			{
				Dictionary<int, OrnamentData> OrnamentDataDict = evObj.Client.ClientData.OrnamentDataDict;
				lock (OrnamentDataDict)
				{
					foreach (int goalId in goalIdList)
					{
						OrnamentData itemData = null;
						if (!OrnamentDataDict.TryGetValue(goalId, out itemData))
						{
							itemData = new OrnamentData();
							itemData.ID = goalId;
							OrnamentDataDict[goalId] = itemData;
						}
						CoupleArenaCoupleJingJiData coupleData = SingletonTemplate<CoupleArenaManager>.Instance().GetCachedCoupleData(evObj.Client.ClientData.RoleID);
						if (coupleData != null && coupleData.DuanWeiType >= 7)
						{
							OrnamentConfigData itemConfig = null;
							if (goalConfigDict.TryGetValue(goalId, out itemConfig))
							{
								if (itemData.Param1 < itemConfig.GoalNum)
								{
									itemData.Param1 = itemConfig.GoalNum;
									if (!this.UpdateDb(evObj.Client.ClientData.RoleID, itemData, evObj.Client.ServerId))
									{
										itemData.Param1 = 0;
									}
								}
							}
						}
					}
				}
			}
		}

		
		public bool ProcessOrnamentGetDataCmd(GameClient client, int nID, byte[] bytes, string[] cmdParams)
		{
			try
			{
				byte[] bytesData = DataHelper.ObjectToBytes<Dictionary<int, OrnamentData>>(client.ClientData.OrnamentDataDict);
				GameManager.ClientMgr.SendToClient(client, bytesData, nID);
				return true;
			}
			catch (Exception ex)
			{
				DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false, false);
			}
			return false;
		}

		
		public bool ProcessOrnamentGetGoodsListCmd(GameClient client, int nID, byte[] bytes, string[] cmdParams)
		{
			try
			{
				byte[] bytesData = DataHelper.ObjectToBytes<List<GoodsData>>(client.ClientData.OrnamentGoodsDataList);
				GameManager.ClientMgr.SendToClient(client, bytesData, nID);
				return true;
			}
			catch (Exception ex)
			{
				DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false, false);
			}
			return false;
		}

		
		public bool ProcessOrnamentSlotForgeCmd(GameClient client, int nID, byte[] bytes, string[] cmdParams)
		{
			try
			{
				int result = 0;
				int roleID = Convert.ToInt32(cmdParams[0]);
				int GoodsID = Convert.ToInt32(cmdParams[1]);
				if (GoodsID <= 0 || GoodsID >= 6)
				{
					result = -2;
					client.sendCmd(nID, string.Format("{0}:{1}:{2}:{3}", new object[]
					{
						result,
						roleID,
						GoodsID,
						0
					}), false);
					return true;
				}
				OrnamentData oData = null;
				if (!client.ClientData.OrnamentDataDict.TryGetValue(GoodsID, out oData))
				{
					result = -2;
					client.sendCmd(nID, string.Format("{0}:{1}:{2}:{3}", new object[]
					{
						result,
						roleID,
						GoodsID,
						0
					}), false);
					return true;
				}
				Dictionary<int, OrnamentSlotConfigData> tempOrnamentSlotLevUpConfig = null;
				lock (this.ConfigMutex)
				{
					tempOrnamentSlotLevUpConfig = this.OrnamentSlotLevUpConfig;
				}
				OrnamentSlotConfigData slotConfigData = null;
				if (!tempOrnamentSlotLevUpConfig.TryGetValue(oData.Param1, out slotConfigData))
				{
					result = -23;
					client.sendCmd(nID, string.Format("{0}:{1}:{2}:{3}", new object[]
					{
						result,
						roleID,
						GoodsID,
						0
					}), false);
					return true;
				}
				if (slotConfigData.Need <= 0)
				{
					result = -23;
					client.sendCmd(nID, string.Format("{0}:{1}:{2}:{3}", new object[]
					{
						result,
						roleID,
						GoodsID,
						0
					}), false);
					return true;
				}
				if (slotConfigData.Need > GameManager.ClientMgr.GetOrnamentCharmPointValue(client))
				{
					result = -32;
					client.sendCmd(nID, string.Format("{0}:{1}:{2}:{3}", new object[]
					{
						result,
						roleID,
						GoodsID,
						0
					}), false);
					return true;
				}
				GameManager.ClientMgr.ModifyOrnamentCharmPointValue(client, -slotConfigData.Need, "饰品", true, true, false);
				oData.Param1++;
				this.UpdateDb(client.ClientData.RoleID, oData, client.ServerId);
				this.TryActiveOrnamentSlot(client);
				this.RefreshOrnamentProps(client);
				client.sendCmd(nID, string.Format("{0}:{1}:{2}:{3}", new object[]
				{
					result,
					roleID,
					GoodsID,
					oData.Param1
				}), false);
				return true;
			}
			catch (Exception ex)
			{
				DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false, false);
			}
			return false;
		}

		
		public bool ProcessOrnamentActiveCmd(GameClient client, int nID, byte[] bytes, string[] cmdParams)
		{
			try
			{
				int result = 0;
				int roleID = Convert.ToInt32(cmdParams[0]);
				int GoodsID = Convert.ToInt32(cmdParams[1]);
				int GoodsDbID = Convert.ToInt32(cmdParams[2]);
				Dictionary<int, OrnamentConfigData> tempOrnamentConfig = null;
				lock (this.ConfigMutex)
				{
					tempOrnamentConfig = this.OrnamentConfig;
				}
				OrnamentConfigData oConfigData = null;
				if (!tempOrnamentConfig.TryGetValue(GoodsID, out oConfigData))
				{
					result = -2;
					client.sendCmd(nID, string.Format("{0}:{1}", result, roleID), false);
					return true;
				}
				if (!this.OrnamentCanAdd(client, GoodsID))
				{
					result = -12;
					client.sendCmd(nID, string.Format("{0}:{1}", result, roleID), false);
					return true;
				}
				if (oConfigData.Type == OrnamentType.OT_Active)
				{
					OrnamentData oData = null;
					if (!client.ClientData.OrnamentDataDict.TryGetValue(GoodsID, out oData))
					{
						result = -2;
						client.sendCmd(nID, string.Format("{0}:{1}", result, roleID), false);
						return true;
					}
					if (!this.CheckCanActiveChengJiuOrnament(client, oData, oConfigData))
					{
						result = -12;
						client.sendCmd(nID, string.Format("{0}:{1}", result, roleID), false);
						return true;
					}
					string strStartTime = "1900-01-01 12:00:00";
					string strEndTime = "1900-01-01 12:00:00";
					Global.AddGoodsDBCommand_Hook(Global._TCPManager.TcpOutPacketPool, client, GoodsID, 1, 0, "", 0, 1, 9000, "", true, 1, "饰品激活", true, strEndTime, 0, 0, 0, 0, 0, 0, 0, true, null, null, strStartTime, 0, true);
				}
				else if (oConfigData.Type == OrnamentType.OT_UseGoods)
				{
					GoodsData goodsData = Global.GetGoodsByDbID(client, GoodsDbID);
					if (goodsData == null || goodsData.Using > 0)
					{
						result = -12;
						client.sendCmd(nID, string.Format("{0}:{1}", result, roleID), false);
						return true;
					}
					if (!GameManager.ClientMgr.NotifyUseGoods(Global._TCPManager.MySocketListener, Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool, client, goodsData, 1, false, true))
					{
						result = -6;
						client.sendCmd(nID, string.Format("{0}:{1}", result, roleID), false);
						return true;
					}
					string strStartTime = "1900-01-01 12:00:00";
					string strEndTime = "1900-01-01 12:00:00";
					Global.AddGoodsDBCommand_Hook(Global._TCPManager.TcpOutPacketPool, client, goodsData.GoodsID, 1, 0, "", 0, goodsData.Binding, 9000, "", true, 1, "饰品激活", true, strEndTime, 0, 0, 0, 0, 0, 0, 0, true, null, null, strStartTime, 0, true);
				}
				if (oConfigData.GoalAward > 0)
				{
					GameManager.ClientMgr.ModifyOrnamentCharmPointValue(client, oConfigData.GoalAward, "饰品激活", true, true, false);
				}
				client.sendCmd(nID, string.Format("{0}:{1}", result, roleID), false);
				return true;
			}
			catch (Exception ex)
			{
				DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false, false);
			}
			return false;
		}

		
		public bool InitConfig()
		{
			List<int> tempOrnamentSlotOpenConfig = new List<int>();
			string StringSlotOpen = GameManager.systemParamsList.GetParamValueByName("OrnamentSiteOpen");
			if (!string.IsNullOrEmpty(StringSlotOpen))
			{
				string[] Field = StringSlotOpen.Split(new char[]
				{
					'|'
				});
				for (int i = 0; i < Field.Length; i++)
				{
					string[] StringPair = Field[i].Split(new char[]
					{
						','
					});
					if (StringPair.Length == 2)
					{
						tempOrnamentSlotOpenConfig.Add(Global.SafeConvertToInt32(StringPair[1]));
					}
				}
			}
			lock (this.ConfigMutex)
			{
				this.OrnamentSlotOpenConfig = tempOrnamentSlotOpenConfig;
			}
			return this.LoadOrnamentConfigFile() && this.LoadOrnamentSlotLevUpFile() && this.LoadOrnamentGroupFile();
		}

		
		public bool LoadOrnamentConfigFile()
		{
			try
			{
				GeneralCachingXmlMgr.RemoveCachingXml(Global.GameResPath("Config/Ornament.xml"));
				XElement xml = GeneralCachingXmlMgr.GetXElement(Global.GameResPath("Config/Ornament.xml"));
				if (null == xml)
				{
					return false;
				}
				Dictionary<OrnamentGoalType, List<int>> tempFunc2GoalId = new Dictionary<OrnamentGoalType, List<int>>();
				Dictionary<int, OrnamentConfigData> tempOrnamentConfig = new Dictionary<int, OrnamentConfigData>();
				IEnumerable<XElement> xmlItems = xml.Elements();
				foreach (XElement xmlItem in xmlItems)
				{
					if (null != xmlItem)
					{
						OrnamentConfigData data = new OrnamentConfigData();
						data.GoodsID = (int)Global.GetSafeAttributeLong(xmlItem, "GoodsID");
						data.Type = (OrnamentType)Global.GetSafeAttributeLong(xmlItem, "Type");
						data.RecoverPoints = (int)Global.GetSafeAttributeLong(xmlItem, "Recover");
						data.GoalType = (OrnamentGoalType)Global.GetSafeAttributeLong(xmlItem, "GoalType");
						data.GoalNum = (int)Global.GetSafeAttributeLong(xmlItem, "GoalNum");
						data.GoalAward = (int)Global.GetSafeAttributeLong(xmlItem, "GoalAward");
						if (!tempFunc2GoalId.ContainsKey(data.GoalType))
						{
							tempFunc2GoalId[data.GoalType] = new List<int>();
						}
						tempFunc2GoalId[data.GoalType].Add(data.GoodsID);
						tempOrnamentConfig[data.GoodsID] = data;
					}
				}
				lock (this.ConfigMutex)
				{
					this.Func2GoalId = tempFunc2GoalId;
					this.OrnamentConfig = tempOrnamentConfig;
				}
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Fatal, string.Format("{0}解析出现异常, {1}", "Config/Ornament.xml", ex.Message), null, true);
				return false;
			}
			return true;
		}

		
		public bool LoadOrnamentSlotLevUpFile()
		{
			try
			{
				GeneralCachingXmlMgr.RemoveCachingXml(Global.GameResPath("Config/OrnamentSite.xml"));
				XElement xml = GeneralCachingXmlMgr.GetXElement(Global.GameResPath("Config/OrnamentSite.xml"));
				if (null == xml)
				{
					return false;
				}
				Dictionary<int, OrnamentSlotConfigData> tempOrnamentSlotLevUpConfig = new Dictionary<int, OrnamentSlotConfigData>();
				IEnumerable<XElement> xmlItems = xml.Elements();
				foreach (XElement xmlItem in xmlItems)
				{
					if (null != xmlItem)
					{
						OrnamentSlotConfigData data = new OrnamentSlotConfigData();
						data.Level = (int)Global.GetSafeAttributeLong(xmlItem, "LevelID");
						data.Need = (int)Global.GetSafeAttributeLong(xmlItem, "Need");
						tempOrnamentSlotLevUpConfig[data.Level] = data;
					}
				}
				lock (this.ConfigMutex)
				{
					this.OrnamentSlotLevUpConfig = tempOrnamentSlotLevUpConfig;
				}
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Fatal, string.Format("{0}解析出现异常, {1}", "Config/OrnamentSite.xml", ex.Message), null, true);
				return false;
			}
			return true;
		}

		
		public bool LoadOrnamentGroupFile()
		{
			try
			{
				GeneralCachingXmlMgr.RemoveCachingXml(Global.GameResPath("Config/OrnamentGroup.xml"));
				XElement xml = GeneralCachingXmlMgr.GetXElement(Global.GameResPath("Config/OrnamentGroup.xml"));
				if (null == xml)
				{
					return false;
				}
				List<OrnamentGroupConfigData> tempOrnamentGroupConfig = new List<OrnamentGroupConfigData>();
				IEnumerable<XElement> xmlItems = xml.Elements();
				foreach (XElement xmlItem in xmlItems)
				{
					if (null != xmlItem)
					{
						OrnamentGroupConfigData data = new OrnamentGroupConfigData();
						data.ID = (int)Global.GetSafeAttributeLong(xmlItem, "ID");
						string StrGoodsID = Global.GetSafeAttributeStr(xmlItem, "OrnamentGoods");
						string[] Field = StrGoodsID.Split(new char[]
						{
							'|'
						});
						for (int i = 0; i < Field.Length; i++)
						{
							data.GoodsIDList.Add(Global.SafeConvertToInt32(Field[i]));
						}
						string TempValueString = Global.GetSafeAttributeStr(xmlItem, "GroupProperty");
						string[] ValueFileds = TempValueString.Split(new char[]
						{
							'|'
						});
						foreach (string value in ValueFileds)
						{
							string[] KvpFileds = value.Split(new char[]
							{
								','
							});
							if (KvpFileds.Length == 2)
							{
								ExtPropIndexes index = ConfigParser.GetPropIndexByPropName(KvpFileds[0]);
								if (index != ExtPropIndexes.Max)
								{
									data.ExtProps[(int)index] = Global.SafeConvertToDouble(KvpFileds[1]);
								}
							}
						}
						tempOrnamentGroupConfig.Add(data);
					}
				}
				lock (this.ConfigMutex)
				{
					this.OrnamentGroupConfig = tempOrnamentGroupConfig;
				}
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Fatal, string.Format("{0}解析出现异常, {1}", "Config/OrnamentGroup.xml", ex.Message), null, true);
				return false;
			}
			return true;
		}

		
		private const string Ornament_OrnamentFileName = "Config/Ornament.xml";

		
		private const string Ornament_OrnamentSiteFileName = "Config/OrnamentSite.xml";

		
		private const string Ornament_OrnamentGroupFileName = "Config/OrnamentGroup.xml";

		
		private Dictionary<OrnamentGoalType, Action<OrnamentGoalEventObject, List<int>, Dictionary<int, OrnamentConfigData>>> evHandlerDict = null;

		
		private Dictionary<OrnamentGoalType, List<int>> Func2GoalId = null;

		
		private object ConfigMutex = new object();

		
		protected List<int> OrnamentSlotOpenConfig = new List<int>();

		
		protected Dictionary<int, OrnamentConfigData> OrnamentConfig = null;

		
		protected Dictionary<int, OrnamentSlotConfigData> OrnamentSlotLevUpConfig = null;

		
		protected List<OrnamentGroupConfigData> OrnamentGroupConfig = null;

		
		private static OrnamentManager instance = new OrnamentManager();
	}
}
