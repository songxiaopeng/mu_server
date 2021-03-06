﻿using System;
using System.Collections.Generic;
using GameDBServer.Core;
using GameDBServer.DB;
using Server.Data;

namespace GameDBServer.Logic
{
	
	public class PreDeleteRoleMgr
	{
		
		public void LoadPreDeleteRoleFromDB(DBManager dbMgr)
		{
			DBQuery.QueryPreDeleteRoleDict(dbMgr, this._PreDeleteRoleDict);
		}

		
		public void AddPreDeleteRole(int rid, DateTime tm)
		{
			lock (this._PreDeleteRoleDict)
			{
				this._PreDeleteRoleDict[rid] = tm;
			}
		}

		
		public bool RemovePreDeleteRole(DBUserInfo dbUserInfo, DBRoleInfo dbRoleInfo)
		{
			bool result;
			lock (this._PreDeleteRoleDict)
			{
				string userID = dbUserInfo.UserID;
				int roleID = dbRoleInfo.RoleID;
				DBManager dbMgr = DBManager.getInstance();
				bool ret = false;
				bool hasrole = DBQuery.GetUserRole(dbMgr, userID, roleID);
				if (hasrole)
				{
					ret = DBWriter.UnPreRemoveRole(dbMgr, roleID);
				}
				if (!ret)
				{
					result = false;
				}
				else
				{
					this._PreDeleteRoleDict.Remove(roleID);
					lock (dbUserInfo)
					{
						int index = dbUserInfo.ListRoleIDs.IndexOf(roleID);
						if (index >= 0 && index < dbUserInfo.ListRoleIDs.Count)
						{
							dbUserInfo.ListRolePreRemoveTime[index] = "";
						}
					}
					result = true;
				}
			}
			return result;
		}

		
		public bool IfInPreDeleteState(int rid)
		{
			bool result;
			lock (this._PreDeleteRoleDict)
			{
				result = this._PreDeleteRoleDict.ContainsKey(rid);
			}
			return result;
		}

		
		public int CalcPreDeleteRoleLeftSeconds(string PreRemoveTime)
		{
			int PreDelLeftSeconds = -1;
			int result;
			if (string.IsNullOrEmpty(PreRemoveTime))
			{
				result = PreDelLeftSeconds;
			}
			else
			{
				DateTime PreRemoveTm = DateTime.Parse(PreRemoveTime);
				PreDelLeftSeconds = GameDBManager.PreDeleteRoleDelaySeconds - (int)(TimeUtil.NowDateTime() - PreRemoveTm).TotalSeconds;
				if (PreDelLeftSeconds < 0)
				{
					PreDelLeftSeconds = 0;
				}
				result = PreDelLeftSeconds;
			}
			return result;
		}

		
		public void UpdatePreDeleteRole()
		{
			lock (this._PreDeleteRoleDict)
			{
				DBManager dbMgr = DBManager.getInstance();
				List<PreDeleteRoleInfo> DeleteRoleInfoList = new List<PreDeleteRoleInfo>();
				foreach (KeyValuePair<int, DateTime> kvp in this._PreDeleteRoleDict)
				{
					if ((TimeUtil.NowDateTime() - kvp.Value).TotalSeconds >= (double)GameDBManager.PreDeleteRoleDelaySeconds)
					{
						int roleID = kvp.Key;
						PreDeleteRoleInfo DelInfo = new PreDeleteRoleInfo();
						DelInfo.RoleID = roleID;
						DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(ref roleID);
						if (null != dbRoleInfo)
						{
							DelInfo.UserID = dbRoleInfo.UserID;
							DelInfo.ZoneID = dbRoleInfo.ZoneID;
							bool ret = false;
							bool hasrole = DBQuery.GetUserRole(dbMgr, dbRoleInfo.UserID, roleID);
							if (hasrole)
							{
								ret = DBWriter.RemoveRole(dbMgr, roleID);
							}
							DBUserInfo dbUserInfo = dbMgr.GetDBUserInfo(dbRoleInfo.UserID);
							if (ret && null != dbUserInfo)
							{
								DeleteRoleInfoList.Add(DelInfo);
								this.HandleDeleteRole(dbUserInfo, dbRoleInfo);
							}
						}
					}
				}
				foreach (PreDeleteRoleInfo data in DeleteRoleInfoList)
				{
					string gmCmdData = string.Format("-deleterole {0} {1} {2}", data.UserID, data.RoleID, data.ZoneID);
					ChatMsgManager.AddGMCmdChatMsg(-1, gmCmdData);
					this._PreDeleteRoleDict.Remove(data.RoleID);
				}
			}
		}

		
		public void HandleDeleteRole(DBUserInfo dbUserInfo, DBRoleInfo dbRoleInfo)
		{
			DBManager dbMgr = DBManager.getInstance();
			string userID = dbUserInfo.UserID;
			int roleID = dbRoleInfo.RoleID;
			lock (dbUserInfo)
			{
				if (dbRoleInfo.Faction > 0 && dbRoleInfo.BHZhiWu == 1)
				{
					int nRoleID = -1;
					bool bGoOn = false;
					List<BangHuiMemberData> bangHuiMemberDataList = DBQuery.GetBangHuiMemberDataList(dbMgr, dbRoleInfo.Faction);
					List<BangHuiMgrItemData> bangHuiMgrItemDataList = DBQuery.GetBangHuiMgrItemItemDataList(dbMgr, dbRoleInfo.Faction);
					if (bangHuiMemberDataList != null && bangHuiMemberDataList.Count > 0)
					{
						if (bangHuiMgrItemDataList != null)
						{
							nRoleID = Global.GetDBRoleInfoByZhiWu(bangHuiMgrItemDataList, 2);
							if (nRoleID <= 0)
							{
								nRoleID = Global.GetDBRoleInfoByZhiWu(bangHuiMgrItemDataList, 3);
								if (nRoleID <= 0)
								{
									nRoleID = Global.GetDBRoleInfoByZhiWu(bangHuiMgrItemDataList, 4);
									if (nRoleID <= 0)
									{
										bGoOn = true;
									}
								}
							}
						}
						if (bGoOn)
						{
							for (int i = 0; i < bangHuiMemberDataList.Count; i++)
							{
								if (bangHuiMemberDataList[i].RoleID != roleID)
								{
									nRoleID = bangHuiMemberDataList[i].RoleID;
									break;
								}
							}
						}
						if (nRoleID > 0)
						{
							lock (Global.BangHuiMutex)
							{
								DBRoleInfo dbRole = dbMgr.GetDBRoleInfo(ref nRoleID);
								if (dbRole != null)
								{
									if (dbRole.Faction == dbRoleInfo.Faction)
									{
										dbRole.BHZhiWu = 1;
										DBWriter.UpdateBangHuiMemberZhiWu(dbMgr, dbRole.Faction, nRoleID, 1);
										DBWriter.UpdateBangHuiRoleID(dbMgr, nRoleID, dbRole.Faction);
										int serverLineID = dbRoleInfo.ServerLineID;
										string strCmdData = string.Format("0::0::0:-chbhzhiwu {0} {1} {2} {3}:0:0:-1", new object[]
										{
											dbRoleInfo.Faction,
											nRoleID,
											1,
											dbRoleInfo.RoleID
										});
										List<LineItem> itemList = LineManager.GetLineItemList();
										if (null != itemList)
										{
											for (int i = 0; i < itemList.Count; i++)
											{
												if (itemList[i].LineID != serverLineID)
												{
													ChatMsgManager.AddChatMsg(itemList[i].LineID, strCmdData);
												}
											}
										}
									}
								}
							}
						}
						BangHuiDestroyMgr.ClearBangHuiLingDi(dbMgr, dbRoleInfo.Faction);
					}
					else
					{
						BangHuiDestroyMgr.DoDestroyBangHui(dbMgr, dbRoleInfo.Faction);
					}
				}
				dbRoleInfo.Faction = 0;
				dbRoleInfo.BHName = "";
				dbRoleInfo.BHZhiWu = 0;
				dbRoleInfo.BangGong = 0;
				DBWriter.UpdateRoleBangHuiInfo(dbMgr, dbRoleInfo.RoleID, dbRoleInfo.Faction, dbRoleInfo.BHName, 0);
				int index = dbUserInfo.ListRoleIDs.IndexOf(roleID);
				if (index >= 0 && index < dbUserInfo.ListRoleIDs.Count)
				{
					dbUserInfo.ListRoleIDs.RemoveAt(index);
					dbUserInfo.ListRoleSexes.RemoveAt(index);
					dbUserInfo.ListRoleOccups.RemoveAt(index);
					dbUserInfo.ListRoleNames.RemoveAt(index);
					dbUserInfo.ListRoleLevels.RemoveAt(index);
					dbUserInfo.ListRoleZoneIDs.RemoveAt(index);
					dbUserInfo.ListRoleChangeLifeCount.RemoveAt(index);
					dbUserInfo.ListRolePreRemoveTime.RemoveAt(index);
				}
			}
		}

		
		private Dictionary<int, DateTime> _PreDeleteRoleDict = new Dictionary<int, DateTime>();
	}
}
