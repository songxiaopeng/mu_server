﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using GameServer.Server;
using GameServer.Tools;
using Server.Tools;

namespace GameServer.Logic.UnionPalace
{
	
	public class UnionPalaceManager : ICmdProcessorEx, ICmdProcessor, IManager
	{
		
		public static UnionPalaceManager getInstance()
		{
			return UnionPalaceManager.instance;
		}

		
		public bool initialize()
		{
			UnionPalaceManager.InitConfig();
			return true;
		}

		
		public bool startup()
		{
			TCPCmdDispatcher.getInstance().registerProcessorEx(1035, 1, 1, UnionPalaceManager.getInstance(), TCPCmdFlags.IsStringArrayParams);
			TCPCmdDispatcher.getInstance().registerProcessorEx(1036, 1, 1, UnionPalaceManager.getInstance(), TCPCmdFlags.IsStringArrayParams);
			return true;
		}

		
		public bool showdown()
		{
			return true;
		}

		
		public bool destroy()
		{
			return true;
		}

		
		public bool processCmd(GameClient client, string[] cmdParams)
		{
			return true;
		}

		
		public bool processCmdEx(GameClient client, int nID, byte[] bytes, string[] cmdParams)
		{
			bool result;
			switch (nID)
			{
			case 1035:
				result = this.ProcessCmdUnionPalaceData(client, nID, bytes, cmdParams);
				break;
			case 1036:
				result = this.ProcessCmdUnionPalaceUp(client, nID, bytes, cmdParams);
				break;
			default:
				result = true;
				break;
			}
			return result;
		}

		
		private bool ProcessCmdUnionPalaceData(GameClient client, int nID, byte[] bytes, string[] cmdParams)
		{
			try
			{
				if (!CheckHelper.CheckCmdLengthAndRole(client, nID, cmdParams, 1))
				{
					return false;
				}
				UnionPalaceData data = UnionPalaceManager.UnionPalaceGetData(client, false);
				client.sendCmd<UnionPalaceData>(1035, data, false);
				return true;
			}
			catch (Exception ex)
			{
				DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false, false);
			}
			return false;
		}

		
		private bool ProcessCmdUnionPalaceUp(GameClient client, int nID, byte[] bytes, string[] cmdParams)
		{
			try
			{
				if (!CheckHelper.CheckCmdLength(client, nID, cmdParams, 1))
				{
					return false;
				}
				if (!UnionPalaceManager.IsGongNengOpened(client))
				{
					return true;
				}
				UnionPalaceData data = UnionPalaceManager.UnionPalaceUp(client);
				client.sendCmd<UnionPalaceData>(1036, data, false);
				return true;
			}
			catch (Exception ex)
			{
				DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false, false);
			}
			return false;
		}

		
		public static UnionPalaceData UnionPalaceGetData(GameClient client, bool isUpdataProps = false)
		{
            UnionPalaceData myUPData2;
			lock(client.ClientData.LockUnionPalace)
			{
                UnionPalaceData myUPData = client.ClientData.MyUnionPalaceData;
				UnionPalaceBasicInfo basicInfo = null;
				if (!UnionPalaceManager.IsGongNengOpened(client))
				{
					myUPData = new UnionPalaceData();
					myUPData.ResultType = -1;
					myUPData2 = myUPData;
				}
				else if (Global.GetBangHuiLevel(client) <= 0)
				{
					if (myUPData == null)
					{
						myUPData = new UnionPalaceData();
					}
					myUPData.ResultType = -2;
					if (isUpdataProps)
					{
						UnionPalaceManager.SetUnionPalaceProps(client, myUPData);
					}
					myUPData2 = myUPData;
				}
				else
				{
					if (myUPData == null)
					{
						myUPData = new UnionPalaceData();
						List<int> data = Global.GetRoleParamsIntListFromDB(client, "UnionPalace");
						if (data == null || data.Count <= 0)
						{
							myUPData.RoleID = client.ClientData.RoleID;
							myUPData.StatueID = 1;
							UnionPalaceManager.ModifyUnionPalaceData(client, myUPData);
						}
						else
						{
							myUPData.RoleID = client.ClientData.RoleID;
							myUPData.StatueID = data[0];
							myUPData.LifeAdd = data[1];
							myUPData.AttackAdd = data[2];
							myUPData.DefenseAdd = data[3];
							myUPData.AttackInjureAdd = data[4];
						}
						basicInfo = UnionPalaceManager.GetUPBasicInfoByID(myUPData.StatueID);
						myUPData.StatueType = basicInfo.StatueType;
						myUPData.StatueLevel = basicInfo.StatueLevel;
						client.ClientData.MyUnionPalaceData = myUPData;
						UnionPalaceManager.SetUnionPalaceProps(client, myUPData);
					}
					myUPData.ZhanGongNeed = UnionPalaceManager.GetUnionPalaceZG(client, 0);
					myUPData.UnionLevel = Global.GetBangHuiLevel(client);
					myUPData.BurstType = 0;
					if (basicInfo == null)
					{
						basicInfo = UnionPalaceManager.GetUPBasicInfoByID(myUPData.StatueID);
					}
					if (basicInfo.UnionLevel < 0)
					{
						myUPData.ResultType = 3;
						if (myUPData.UnionLevel < 9)
						{
							myUPData.ResultType = 4;
						}
					}
					else
					{
						myUPData.ResultType = 0;
						if (myUPData.UnionLevel < basicInfo.UnionLevel)
						{
							myUPData.ResultType = 5;
							if (myUPData.LifeAdd != 0 || myUPData.AttackInjureAdd != 0 || myUPData.DefenseAdd != 0 || myUPData.AttackAdd != 0 || myUPData.StatueType > 1)
							{
								myUPData.ResultType = 4;
							}
							else
							{
								int maxLevel = 0;
								IOrderedEnumerable<UnionPalaceBasicInfo> temp = from info in UnionPalaceManager._unionPalaceBasicList.Values
								where info.StatueID <= myUPData.StatueID && info.StatueLevel <= myUPData.StatueLevel && info.UnionLevel <= myUPData.UnionLevel && info.UnionLevel > 0
								orderby info.StatueID descending
								select info;
								if (temp.Any<UnionPalaceBasicInfo>())
								{
									maxLevel = temp.First<UnionPalaceBasicInfo>().StatueLevel;
								}
								if (basicInfo.StatueLevel > maxLevel + 1)
								{
									myUPData.ResultType = 4;
								}
							}
						}
					}
					if (isUpdataProps)
					{
						UnionPalaceManager.SetUnionPalaceProps(client, myUPData);
					}
					myUPData.ZhanGongLeft = client.ClientData.BangGong;
					myUPData2 = myUPData;
				}
			}
            return myUPData2;
		}

		
		public static void ModifyUnionPalaceData(GameClient client, UnionPalaceData data)
		{
			List<int> dataList = new List<int>();
			dataList.AddRange(new int[]
			{
				data.StatueID,
				data.LifeAdd,
				data.AttackAdd,
				data.DefenseAdd,
				data.AttackInjureAdd
			});
			Global.SaveRoleParamsIntListToDB(client, dataList, "UnionPalace", true);
		}

		
		public static UnionPalaceData UnionPalaceUp(GameClient client)
		{
			UnionPalaceData result;
			lock (client.ClientData.LockUnionPalace)
			{
				UnionPalaceData myUPData = UnionPalaceManager.UnionPalaceGetData(client, false);
				if (myUPData.ResultType < 0)
				{
					result = myUPData;
				}
				else if (myUPData.ResultType == 3 || myUPData.ResultType == -4)
				{
					myUPData.ResultType = -4;
					result = myUPData;
				}
				else if (!UnionPalaceManager.IsGongNengOpened(client))
				{
					myUPData.ResultType = -1;
					result = myUPData;
				}
				else
				{
					UnionPalaceBasicInfo basicInfo = UnionPalaceManager.GetUPBasicInfoByID(myUPData.StatueID);
					if (basicInfo.UnionLevel < 0)
					{
						myUPData.ResultType = -4;
						result = myUPData;
					}
					else
					{
						int bhLevel = Global.GetBangHuiLevel(client);
						if (basicInfo.UnionLevel > bhLevel)
						{
							myUPData.ResultType = -8;
							result = myUPData;
						}
						else if (bhLevel < myUPData.StatueLevel)
						{
							myUPData.ResultType = -7;
							result = myUPData;
						}
						else
						{
							int zhanGongNeed = UnionPalaceManager.GetUnionPalaceZG(client, 0);
							if (!GameManager.ClientMgr.SubUserBangGong(Global._TCPManager.MySocketListener, Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool, client, zhanGongNeed))
							{
								myUPData.ResultType = -3;
								result = myUPData;
							}
							else
							{
								UnionPalaceRateInfo rateInfo = UnionPalaceManager.GetUPRateInfoByID(basicInfo.StatueLevel);
								int[] addNums = null;
								int rate = 0;
								int r = Global.GetRandomNumber(0, 100);
								for (int i = 0; i < rateInfo.RateList.Count; i++)
								{
									rate += rateInfo.RateList[i];
									if (r <= rate)
									{
										addNums = rateInfo.AddNumList[rateInfo.RateList[i]].ToArray();
										myUPData.BurstType = i;
										break;
									}
								}
								List<int> logList = new List<int>();
								logList.Add(myUPData.StatueID);
								logList.Add(myUPData.LifeAdd);
								logList.Add(myUPData.AttackAdd);
								logList.Add(myUPData.DefenseAdd);
								logList.Add(myUPData.AttackInjureAdd);
								myUPData.LifeAdd += addNums[0] * UnionPalaceManager._gmRate;
								myUPData.LifeAdd = ((myUPData.LifeAdd > basicInfo.LifeMax) ? basicInfo.LifeMax : myUPData.LifeAdd);
								myUPData.AttackAdd += addNums[1] * UnionPalaceManager._gmRate;
								myUPData.AttackAdd = ((myUPData.AttackAdd > basicInfo.AttackMax) ? basicInfo.AttackMax : myUPData.AttackAdd);
								myUPData.DefenseAdd += addNums[2] * UnionPalaceManager._gmRate;
								myUPData.DefenseAdd = ((myUPData.DefenseAdd > basicInfo.DefenseMax) ? basicInfo.DefenseMax : myUPData.DefenseAdd);
								myUPData.AttackInjureAdd += addNums[3] * UnionPalaceManager._gmRate;
								myUPData.AttackInjureAdd = ((myUPData.AttackInjureAdd > basicInfo.AttackInjureMax) ? basicInfo.AttackInjureMax : myUPData.AttackInjureAdd);
								if (myUPData.LifeAdd < basicInfo.LifeMax || myUPData.DefenseAdd < basicInfo.DefenseMax || myUPData.AttackAdd < basicInfo.AttackMax || myUPData.AttackInjureAdd < basicInfo.AttackInjureMax)
								{
									myUPData.ResultType = 1;
								}
								else
								{
									myUPData.StatueID++;
									basicInfo = UnionPalaceManager.GetUPBasicInfoByID(myUPData.StatueID);
									myUPData.StatueType = basicInfo.StatueType;
									myUPData.StatueLevel = basicInfo.StatueLevel;
									myUPData.LifeAdd = 0;
									myUPData.AttackAdd = 0;
									myUPData.DefenseAdd = 0;
									myUPData.AttackInjureAdd = 0;
									myUPData.ResultType = 2;
									if (myUPData.StatueID > UnionPalaceManager._unionPalaceBasicList.Count || basicInfo.UnionLevel < 0)
									{
										myUPData.ResultType = 3;
									}
									else if (bhLevel < basicInfo.UnionLevel)
									{
										myUPData.ResultType = 5;
									}
									else if (bhLevel < myUPData.StatueLevel)
									{
										myUPData.ResultType = 4;
									}
								}
								int today = Global.GetOffsetDayNow();
								int upCount = UnionPalaceManager.GetUpCount(client, today);
								myUPData.ZhanGongNeed = UnionPalaceManager.GetUnionPalaceZG(client, upCount + 1);
								UnionPalaceManager.ModifyUpCount(client, upCount + 1);
								UnionPalaceManager.ModifyUnionPalaceData(client, myUPData);
								client.ClientData.MyUnionPalaceData = myUPData;
								UnionPalaceManager.SetUnionPalaceProps(client, myUPData);
								GameManager.ClientMgr.NotifyUpdateEquipProps(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, client);
								GameManager.ClientMgr.NotifyOthersLifeChanged(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, client, true, false, 7);
								myUPData.ZhanGongLeft = client.ClientData.BangGong;
								logList.Add(myUPData.StatueID);
								logList.Add(myUPData.LifeAdd);
								logList.Add(myUPData.AttackAdd);
								logList.Add(myUPData.DefenseAdd);
								logList.Add(myUPData.AttackInjureAdd);
								logList.Add(upCount);
								EventLogManager.AddUnionPalaceEvent(client, LogRecordType.UnionPalace, new object[]
								{
									logList.ToArray()
								});
								result = myUPData;
							}
						}
					}
				}
			}
			return result;
		}

		
		public static void initSetUnionPalaceProps(GameClient client, bool isUpdataProps = false)
		{
			lock (client.ClientData.LockUnionPalace)
			{
				client.ClientData.MyUnionPalaceData = null;
				UnionPalaceData UnionPalaceData = UnionPalaceManager.UnionPalaceGetData(client, isUpdataProps);
				GameManager.ClientMgr.NotifyUpdateEquipProps(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, client);
				GameManager.ClientMgr.NotifyOthersLifeChanged(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, client, true, false, 7);
			}
		}

		
		public static void SetUnionPalaceProps(GameClient client, UnionPalaceData myData)
		{
			lock (client.ClientData.LockUnionPalace)
			{
				int life = 0;
				int attack = 0;
				int defense = 0;
				int injure = 0;
				UnionPalaceBasicInfo basicInfo = UnionPalaceManager.GetUPBasicInfoByID(myData.StatueID);
				int bangHuiLevel = Global.GetBangHuiLevel(client);
				if (basicInfo != null && basicInfo.UnionLevel <= bangHuiLevel && myData.StatueLevel <= bangHuiLevel)
				{
					life = myData.LifeAdd;
					attack = myData.AttackAdd;
					defense = myData.DefenseAdd;
					injure = myData.AttackInjureAdd;
				}
				foreach (UnionPalaceBasicInfo d in UnionPalaceManager._unionPalaceBasicList.Values)
				{
					if (d.StatueID < myData.StatueID && d.UnionLevel <= bangHuiLevel && d.StatueLevel <= bangHuiLevel)
					{
						life += d.LifeMax;
						attack += d.AttackMax;
						defense += d.DefenseMax;
						injure += d.AttackInjureMax;
					}
				}
				double lifePercent = 0.0;
				int step = (myData.StatueID - 1) / 8;
				if (step > 0)
				{
					if (myData.ResultType == 4 || myData.ResultType == -7)
					{
						int maxID = 0;
						IOrderedEnumerable<UnionPalaceBasicInfo> temp = from info in UnionPalaceManager._unionPalaceBasicList.Values
						where info.StatueID <= myData.StatueID && info.StatueLevel <= myData.StatueLevel && info.UnionLevel <= myData.UnionLevel && info.UnionLevel > 0
						orderby info.StatueID descending
						select info;
						if (temp.Any<UnionPalaceBasicInfo>())
						{
							maxID = temp.First<UnionPalaceBasicInfo>().StatueID;
						}
						step = maxID / 8;
					}
					UnionPalaceSpecialInfo s = UnionPalaceManager.GetUPSpecialInfoByID(step);
					if (s != null && s.UnionLevel <= bangHuiLevel)
					{
						lifePercent = s.MaxLifePercent;
					}
				}
				EquipPropItem propItem = new EquipPropItem();
				propItem.ExtProps[13] = (double)life;
				propItem.ExtProps[45] = (double)attack;
				propItem.ExtProps[46] = (double)defense;
				propItem.ExtProps[27] = (double)injure;
				propItem.ExtProps[14] = lifePercent;
				client.ClientData.PropsCacheManager.SetExtProps(new object[]
				{
					19,
					propItem.ExtProps
				});
			}
		}

		
		public static int GetUpCount(GameClient client, int day)
		{
			int count = 0;
			int dayOld = 0;
			List<int> data = Global.GetRoleParamsIntListFromDB(client, "UnionPalaceUpCount");
			if (data != null && data.Count > 0)
			{
				dayOld = data[0];
			}
			if (dayOld == day)
			{
				count = data[1];
			}
			else
			{
				UnionPalaceManager.ModifyUpCount(client, count);
			}
			return count;
		}

		
		public static void ModifyUpCount(GameClient client, int count)
		{
			List<int> dataList = new List<int>();
			dataList.AddRange(new int[]
			{
				Global.GetOffsetDayNow(),
				count
			});
			Global.SaveRoleParamsIntListToDB(client, dataList, "UnionPalaceUpCount", true);
		}

		
		public static bool IsGongNengOpened(GameClient client)
		{
			return !GameFuncControlManager.IsGameFuncDisabled(GameFuncType.System2Dot0) && GlobalNew.IsGongNengOpened(client, GongNengIDs.ZhanMengShenDian, false) && GameManager.VersionSystemOpenMgr.IsVersionSystemOpen("UnionPalace");
		}

		
		public static void InitConfig()
		{
			UnionPalaceManager.LoadUnionPalaceBasicInfo();
			UnionPalaceManager.LoadUnionPalaceSpecialInfo();
			UnionPalaceManager.LoadUnionPalaceRateInfo();
		}

		
		private static void LoadUnionPalaceBasicInfo()
		{
			string fileName = Global.GameResPath("Config/ShenDianLevelUp.xml");
			XElement xml = CheckHelper.LoadXml(fileName, true);
			if (null != xml)
			{
				try
				{
					UnionPalaceManager._unionPalaceBasicList.Clear();
					IEnumerable<XElement> xmlItems = xml.Elements();
					foreach (XElement xmlItem in xmlItems)
					{
						if (xmlItem != null)
						{
							UnionPalaceBasicInfo config = new UnionPalaceBasicInfo();
							config.StatueID = Convert.ToInt32(Global.GetDefAttributeStr(xmlItem, "ID", "0"));
							config.StatueType = Convert.ToInt32(Global.GetDefAttributeStr(xmlItem, "Type", "0"));
							config.StatueLevel = Convert.ToInt32(Global.GetDefAttributeStr(xmlItem, "Level", "0"));
							config.UnionLevel = Convert.ToInt32(Global.GetDefAttributeStr(xmlItem, "NeedZhanMengLevel", "0"));
							string preStr = Global.GetDefAttributeStr(xmlItem, "NeedStatueLevel", "");
							if (!string.IsNullOrEmpty(preStr))
							{
								string[] arr = preStr.Split(new char[]
								{
									','
								});
								if (arr.Length == 2)
								{
									config.PreStatueType = Convert.ToInt32(arr[0]);
									config.PreStatueLevel = Convert.ToInt32(arr[1]);
								}
							}
							config.LifeMax = Convert.ToInt32(Global.GetDefAttributeStr(xmlItem, "MaxLifeV", "0"));
							config.AttackMax = Convert.ToInt32(Global.GetDefAttributeStr(xmlItem, "AddAttack", "0"));
							config.DefenseMax = Convert.ToInt32(Global.GetDefAttributeStr(xmlItem, "AddDefense", "0"));
							config.AttackInjureMax = Convert.ToInt32(Global.GetDefAttributeStr(xmlItem, "AddAttackInjure", "0"));
							UnionPalaceManager._unionPalaceBasicList.Add(config.StatueID, config);
						}
					}
				}
				catch (Exception ex)
				{
					LogManager.WriteLog(LogTypes.Fatal, string.Format("加载[{0}]时出错!!!", fileName), null, true);
				}
			}
		}

		
		public static void LoadUnionPalaceSpecialInfo()
		{
			string fileName = Global.GameResPath("Config/ShenDianExtra.xml");
			XElement xml = CheckHelper.LoadXml(fileName, true);
			if (null != xml)
			{
				try
				{
					UnionPalaceManager._unionPalaceSpecialList.Clear();
					IEnumerable<XElement> xmlItems = xml.Elements();
					foreach (XElement xmlItem in xmlItems)
					{
						if (xmlItem != null)
						{
							UnionPalaceSpecialInfo config = new UnionPalaceSpecialInfo();
							config.StatueLevel = Convert.ToInt32(Global.GetDefAttributeStr(xmlItem, "StatueLevel", "0"));
							config.UnionLevel = Convert.ToInt32(Global.GetDefAttributeStr(xmlItem, "ZhanMengLevel", "0"));
							config.MaxLifePercent = Convert.ToDouble(Global.GetDefAttributeStr(xmlItem, "MaxLifePercent", "0"));
							UnionPalaceManager._unionPalaceSpecialList.Add(config.StatueLevel, config);
						}
					}
				}
				catch (Exception ex)
				{
					LogManager.WriteLog(LogTypes.Fatal, string.Format("加载[{0}]时出错!!!", fileName), null, true);
				}
			}
		}

		
		public static void LoadUnionPalaceRateInfo()
		{
			string fileName = Global.GameResPath("Config/ShenDianScale.xml");
			XElement xml = CheckHelper.LoadXml(fileName, true);
			if (null != xml)
			{
				try
				{
					UnionPalaceManager._unionPalaceRateList.Clear();
					IEnumerable<XElement> xmlItems = xml.Elements();
					foreach (XElement xmlItem in xmlItems)
					{
						if (xmlItem != null)
						{
							UnionPalaceRateInfo config = new UnionPalaceRateInfo();
							config.StatueLevel = Convert.ToInt32(Global.GetDefAttributeStr(xmlItem, "Level", "0"));
							string addString = Convert.ToString(Global.GetDefAttributeStr(xmlItem, "Scale", ""));
							if (addString.Length > 0)
							{
								string[] addArr = addString.Split(new char[]
								{
									'|'
								});
								foreach (string str in addArr)
								{
									string[] oneArr = str.Split(new char[]
									{
										','
									});
									int rate = (int)(float.Parse(oneArr[0]) * 100f);
									config.RateList.Add(rate);
									List<int> valueList = new List<int>();
									for (int i = 1; i < oneArr.Length; i++)
									{
										valueList.Add(int.Parse(oneArr[i]));
									}
									config.AddNumList.Add(rate, valueList);
								}
								UnionPalaceManager._unionPalaceRateList.Add(config.StatueLevel, config);
							}
						}
					}
				}
				catch (Exception ex)
				{
					LogManager.WriteLog(LogTypes.Fatal, string.Format("加载[{0}]时出错!!!", fileName), null, true);
				}
			}
		}

		
		public static UnionPalaceBasicInfo GetUPBasicInfoByID(int id)
		{
			UnionPalaceBasicInfo result;
			if (UnionPalaceManager._unionPalaceBasicList.ContainsKey(id))
			{
				result = UnionPalaceManager._unionPalaceBasicList[id];
			}
			else
			{
				result = null;
			}
			return result;
		}

		
		public static UnionPalaceSpecialInfo GetUPSpecialInfoByID(int id)
		{
			UnionPalaceSpecialInfo result;
			if (UnionPalaceManager._unionPalaceSpecialList.ContainsKey(id))
			{
				result = UnionPalaceManager._unionPalaceSpecialList[id];
			}
			else
			{
				result = null;
			}
			return result;
		}

		
		public static UnionPalaceRateInfo GetUPRateInfoByID(int id)
		{
			UnionPalaceRateInfo result;
			if (UnionPalaceManager._unionPalaceRateList.ContainsKey(id))
			{
				result = UnionPalaceManager._unionPalaceRateList[id];
			}
			else
			{
				result = null;
			}
			return result;
		}

		
		public static int GetUnionPalaceZG(GameClient client, int upCount = 0)
		{
			if (upCount <= 0)
			{
				int today = Global.GetOffsetDayNow();
				upCount = UnionPalaceManager.GetUpCount(client, today);
			}
			int[] zhanGongList = GameManager.systemParamsList.GetParamValueIntArrayByName("ZhanMengShenDian", ',');
			if (upCount >= zhanGongList.Length)
			{
				upCount = zhanGongList.Length - 1;
			}
			return zhanGongList[upCount];
		}

		
		public static void SetUnionPalaceLevelByID(GameClient client, int id)
		{
			List<int> dataList = new List<int>();
			List<int> list = dataList;
			int[] array = new int[5];
			array[0] = id;
			list.AddRange(array);
			Global.SaveRoleParamsIntListToDB(client, dataList, "UnionPalace", true);
			client.ClientData.MyUnionPalaceData = null;
			UnionPalaceManager.initSetUnionPalaceProps(client, true);
		}

		
		public static void SetUnionPalaceCount(GameClient client, int count)
		{
			count = ((count < 0) ? 0 : count);
			UnionPalaceManager.ModifyUpCount(client, count);
			UnionPalaceData myUPData = client.ClientData.MyUnionPalaceData;
			myUPData.ZhanGongNeed = UnionPalaceManager.GetUnionPalaceZG(client, 0);
		}

		
		public static void SetUnionPalaceRate(GameClient client, int rate)
		{
			UnionPalaceManager._gmRate = rate;
		}

		
		private const int DEFAULT_MIN_ID = 1;

		
		private const int UP_LEVEL_MAX = 5;

		
		private const int STATUE_COUNT = 8;

		
		private const int UNION_PALACE_MAX_LEVEL = 9;

		
		private const int STATUE_MAX_LEVEL = 5;

		
		public static int _gmRate = 1;

		
		private static UnionPalaceManager instance = new UnionPalaceManager();

		
		private static Dictionary<int, UnionPalaceBasicInfo> _unionPalaceBasicList = new Dictionary<int, UnionPalaceBasicInfo>();

		
		private static Dictionary<int, UnionPalaceSpecialInfo> _unionPalaceSpecialList = new Dictionary<int, UnionPalaceSpecialInfo>();

		
		private static Dictionary<int, UnionPalaceRateInfo> _unionPalaceRateList = new Dictionary<int, UnionPalaceRateInfo>();
	}
}
