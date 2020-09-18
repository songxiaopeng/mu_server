﻿using System;
using System.Collections.Generic;
using System.Globalization;
using GameDBServer.DB;
using Server.Data;

namespace GameDBServer.Logic.Ten
{
	
	internal class TenManager
	{
		
		public static void initTen(string[] fields)
		{
			TenManager._tenAwards = new Dictionary<int, TenAwardData>();
			if (fields != null && fields.Length > 0)
			{
				foreach (string item in fields)
				{
					if (item != null)
					{
						string[] arr = item.Split(new char[]
						{
							':'
						});
						TenAwardData config = new TenAwardData();
						config.AwardID = Convert.ToInt32(arr[0]);
						config.DbKey = arr[1];
						config.OnlyNum = Convert.ToInt32(arr[2]);
						config.DayMaxNum = Convert.ToInt32(arr[3]);
						config.MailTitle = arr[5];
						config.MailContent = arr[6];
						config.MailUser = arr[7];
						config.BeginTime = DateTime.ParseExact(arr[8], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
						config.EndTime = DateTime.ParseExact(arr[9], "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
						config.RoleLevel = Convert.ToInt32(arr[10]);
						string awards = arr[4];
						if (awards.Length > 0)
						{
							config.AwardGoods = new List<GoodsData>();
							string[] awardsArr = awards.Split(new char[]
							{
								'|'
							});
							foreach (string award in awardsArr)
							{
								string[] oneArr = award.Split(new char[]
								{
									','
								});
								GoodsData d = new GoodsData();
								d.GoodsID = Convert.ToInt32(oneArr[0]);
								d.GCount = Convert.ToInt32(oneArr[1]);
								d.Binding = Convert.ToInt32(oneArr[2]);
								config.AwardGoods.Add(d);
							}
						}
						TenManager._tenAwards.Add(config.AwardID, config);
					}
				}
				TenManager._isInitTen = true;
			}
		}

		
		private static TenAwardData getTenAward(int awardID)
		{
			TenAwardData result;
			if (TenManager._tenAwards.ContainsKey(awardID))
			{
				result = TenManager._tenAwards[awardID];
			}
			else
			{
				result = null;
			}
			return result;
		}

		
		public static void ScanLastGroup(DBManager dbMgr)
		{
			long nowTicks = DateTime.Now.Ticks / 10000L;
			if (nowTicks - TenManager.LastScanTicks >= 30000L && TenManager._isInitTen)
			{
				TenManager.LastScanTicks = nowTicks;
				List<TenAwardData> groupList = DBQuery.ScanNewGroupTenFromTable(dbMgr);
				if (groupList != null && groupList.Count > 0 && TenManager._tenAwards.Count > 0 && TenManager._isInitTen)
				{
					foreach (TenAwardData item in groupList)
					{
						bool isSucc = DBWriter.UpdateTenState(dbMgr, item.DbID, 1);
						if (isSucc)
						{
							int result = TenManager.SendAward(dbMgr, item.UserID, item.RoleID, item.AwardID);
							DBWriter.UpdateTenState(dbMgr, item.DbID, result);
						}
					}
				}
			}
		}

		
		public static int SendAward(DBManager dbMgr, string userID, int roleID, int awardID)
		{
			TenAwardData awardData = TenManager.getTenAward(awardID);
			int result;
			if (awardData == null)
			{
				result = -6;
			}
			else
			{
				DateTime now = DateTime.Now;
				if (now < awardData.BeginTime || now > awardData.EndTime)
				{
					result = -9;
				}
				else
				{
					DBRoleInfo roleData = DBManager.getInstance().GetDBRoleInfo(ref roleID);
					if (roleData == null)
					{
						result = -3;
					}
					else if (roleData.ChangeLifeCount * 100 + roleData.Level < awardData.RoleLevel)
					{
						result = -10;
					}
					else
					{
						if (awardData.OnlyNum > 0)
						{
							int totalNum = DBQuery.TenOnlyNum(dbMgr, userID, awardID);
							if (totalNum > 0)
							{
								return -5;
							}
						}
						if (awardData.DayMaxNum > 0)
						{
							int totalNum = DBQuery.TenDayNum(dbMgr, userID, awardID);
							if (totalNum >= awardData.DayMaxNum)
							{
								return -5;
							}
						}
						string mailGoodsString = "";
						if (null != awardData.AwardGoods)
						{
							foreach (GoodsData goods in awardData.AwardGoods)
							{
								int useCount = goods.GCount;
								mailGoodsString += string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}_{10}_{11}_{12}_{13}_{14}_{15}", new object[]
								{
									goods.GoodsID,
									goods.Forge_level,
									goods.Quality,
									goods.Props,
									useCount,
									0,
									0,
									goods.Jewellist,
									goods.AddPropIndex,
									goods.Binding,
									goods.BornIndex,
									goods.Lucky,
									goods.Strong,
									goods.ExcellenceInfo,
									goods.AppendPropLev,
									goods.ChangeLifeLevForEquip
								});
								if (mailGoodsString.Length > 0)
								{
									mailGoodsString += "|";
								}
							}
						}
						string[] fields = new string[]
						{
							"-1",
							awardData.MailUser,
							roleID.ToString(),
							"",
							awardData.MailTitle.ToString(),
							awardData.MailContent.ToString(),
							"0",
							"0",
							"0",
							mailGoodsString
						};
						int addGoodsCount = 0;
						int mailID = Global.AddMail(dbMgr, fields, out addGoodsCount);
						if (mailID > 0)
						{
							string gmCmd = string.Format("{0}|{1}", roleID.ToString(), mailID);
							string gmCmdData = string.Format("-notifymail {0}", gmCmd);
							ChatMsgManager.AddGMCmdChatMsg(-1, gmCmdData);
							result = mailID;
						}
						else
						{
							result = -8;
						}
					}
				}
			}
			return result;
		}

		
		private static Dictionary<int, TenAwardData> _tenAwards = new Dictionary<int, TenAwardData>();

		
		private static bool _isInitTen = false;

		
		private static long LastScanTicks = DateTime.Now.Ticks / 10000L;

		
		public enum TenResultType
		{
			
			Default,
			
			Success,
			
			EnoPara = -1,
			
			EnoRole = -3,
			
			EIp = -4,
			
			ECountMax = -5,
			
			EAware = -6,
			
			EBag = -7,
			
			Fail = -8,
			
			ETimeOut = -9,
			
			ELevelLimit = -10
		}
	}
}
