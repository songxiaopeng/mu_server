﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using GameServer.Logic.Damon;
using GameServer.Server;
using GameServer.Tools;
using Server.Data;
using Server.Tools;

namespace GameServer.Logic.Goods
{
	// Token: 0x020002E1 RID: 737
	public class PetSkillManager : ICmdProcessorEx, ICmdProcessor, IManager
	{
		// Token: 0x06000BA6 RID: 2982 RVA: 0x000B6430 File Offset: 0x000B4630
		public static PetSkillManager getInstance()
		{
			return PetSkillManager.instance;
		}

		// Token: 0x06000BA7 RID: 2983 RVA: 0x000B6448 File Offset: 0x000B4648
		public bool initialize()
		{
			PetSkillManager.InitConfig();
			return true;
		}

		// Token: 0x06000BA8 RID: 2984 RVA: 0x000B6464 File Offset: 0x000B4664
		public bool startup()
		{
			TCPCmdDispatcher.getInstance().registerProcessorEx(1037, 2, 2, PetSkillManager.getInstance(), TCPCmdFlags.IsStringArrayParams);
			TCPCmdDispatcher.getInstance().registerProcessorEx(1038, 3, 3, PetSkillManager.getInstance(), TCPCmdFlags.IsStringArrayParams);
			TCPCmdDispatcher.getInstance().registerProcessorEx(1039, 1, 1, PetSkillManager.getInstance(), TCPCmdFlags.IsStringArrayParams);
			TCPCmdDispatcher.getInstance().registerProcessorEx(1065, 3, 3, PetSkillManager.getInstance(), TCPCmdFlags.IsStringArrayParams);
			return true;
		}

		// Token: 0x06000BA9 RID: 2985 RVA: 0x000B64D8 File Offset: 0x000B46D8
		public bool showdown()
		{
			return true;
		}

		// Token: 0x06000BAA RID: 2986 RVA: 0x000B64EC File Offset: 0x000B46EC
		public bool destroy()
		{
			return true;
		}

		// Token: 0x06000BAB RID: 2987 RVA: 0x000B6500 File Offset: 0x000B4700
		public bool processCmd(GameClient client, string[] cmdParams)
		{
			return true;
		}

		// Token: 0x06000BAC RID: 2988 RVA: 0x000B6514 File Offset: 0x000B4714
		public bool processCmdEx(GameClient client, int nID, byte[] bytes, string[] cmdParams)
		{
			bool result;
			switch (nID)
			{
			case 1037:
				result = this.ProcessCmdPetSkillUp(client, nID, bytes, cmdParams);
				break;
			case 1038:
				result = this.ProcessCmdPetSkillAwake(client, nID, bytes, cmdParams);
				break;
			case 1039:
				result = this.ProcessCmdPetSkillAwakeCost(client, nID, bytes, cmdParams);
				break;
			default:
				result = (nID != 1065 || this.ProcessCmdPetSkillInherit(client, nID, bytes, cmdParams));
				break;
			}
			return result;
		}

		// Token: 0x06000BAD RID: 2989 RVA: 0x000B6584 File Offset: 0x000B4784
		private bool ProcessCmdPetSkillUp(GameClient client, int nID, byte[] bytes, string[] cmdParams)
		{
			try
			{
				if (!CheckHelper.CheckCmdLength(client, nID, cmdParams, 2))
				{
					return false;
				}
				int petID = Convert.ToInt32(cmdParams[0]);
				int pit = Convert.ToInt32(cmdParams[1]);
				int resultType = (int)PetSkillManager.PetSkillUp(client, petID, pit);
				client.sendCmd<int>(1037, resultType, false);
				return true;
			}
			catch (Exception ex)
			{
				DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false, false);
			}
			return false;
		}

		// Token: 0x06000BAE RID: 2990 RVA: 0x000B6610 File Offset: 0x000B4810
		private bool ProcessCmdPetSkillAwake(GameClient client, int nID, byte[] bytes, string[] cmdParams)
		{
			try
			{
				if (!CheckHelper.CheckCmdLength(client, nID, cmdParams, 3))
				{
					return false;
				}
				int petID = Convert.ToInt32(cmdParams[0]);
				int lockPit = Convert.ToInt32(cmdParams[1]);
				int lockPit2 = Convert.ToInt32(cmdParams[2]);
				List<int> lockPitList = new List<int>();
				if (lockPit > 0)
				{
					lockPitList.Add(lockPit);
				}
				if (lockPit2 > 0)
				{
					lockPitList.Add(lockPit2);
				}
				string result = "";
				int resultType = (int)PetSkillManager.PetSkillAwake(client, petID, lockPitList, out result);
				client.sendCmd(1038, string.Format("{0}:{1}", resultType, result), false);
				return true;
			}
			catch (Exception ex)
			{
				DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false, false);
			}
			return false;
		}

		// Token: 0x06000BAF RID: 2991 RVA: 0x000B66F8 File Offset: 0x000B48F8
		private bool ProcessCmdPetSkillAwakeCost(GameClient client, int nID, byte[] bytes, string[] cmdParams)
		{
			try
			{
				if (!CheckHelper.CheckCmdLength(client, nID, cmdParams, 1))
				{
					return false;
				}
				int result = PetSkillManager.GetSkillAwakeCost(PetSkillManager.GetUpCount(client));
				client.sendCmd<int>(1039, result, false);
				return true;
			}
			catch (Exception ex)
			{
				DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false, false);
			}
			return false;
		}

		// Token: 0x06000BB0 RID: 2992 RVA: 0x000B676C File Offset: 0x000B496C
		private bool ProcessCmdPetSkillInherit(GameClient client, int nID, byte[] bytes, string[] cmdParams)
		{
			try
			{
				if (!CheckHelper.CheckCmdLength(client, nID, cmdParams, 3))
				{
					return false;
				}
				int srcPetID = Convert.ToInt32(cmdParams[0]);
				int tarPetID = Convert.ToInt32(cmdParams[1]);
				int userMoney = Convert.ToInt32(cmdParams[2]);
				string outProps = "";
				int result = (int)PetSkillManager.PetSkillInherit(client, srcPetID, tarPetID, userMoney, out outProps);
				client.sendCmd(1065, string.Format("{0}:{1}", result, outProps), false);
				return true;
			}
			catch (Exception ex)
			{
				DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false, false);
			}
			return false;
		}

		// Token: 0x06000BB1 RID: 2993 RVA: 0x000B6868 File Offset: 0x000B4A68
		private static EPetSkillState PetSkillInherit(GameClient client, int srcPetID, int tarPetID, int userMoney, out string outProps)
		{
			outProps = "";
			EPetSkillState result;
			if (GameFuncControlManager.IsGameFuncDisabled(GameFuncType.System2Dot6))
			{
				result = EPetSkillState.EnoOpen;
			}
			else if (!PetSkillManager.IsGongNengOpened(client))
			{
				result = EPetSkillState.EnoOpen;
			}
			else
			{
				GoodsData srcGoodsData = DamonMgr.GetDamonGoodsDataByDbID(client, srcPetID);
				GoodsData tarGoodsData = DamonMgr.GetDamonGoodsDataByDbID(client, tarPetID);
				if (null == srcGoodsData)
				{
					srcGoodsData = Global.GetGoodsByDbID(client, srcPetID);
				}
				else if (srcGoodsData.Site != 5000)
				{
					return EPetSkillState.EnoUsing;
				}
				if (null == tarGoodsData)
				{
					tarGoodsData = Global.GetGoodsByDbID(client, tarPetID);
				}
				else if (tarGoodsData.Site != 5000)
				{
					return EPetSkillState.EnoUsing;
				}
				if (srcGoodsData == null || srcGoodsData.GCount <= 0 || tarGoodsData == null || tarGoodsData.GCount <= 0)
				{
					result = EPetSkillState.EnoPet;
				}
				else
				{
					if (1 == userMoney)
					{
						if (client.ClientData.UserMoney < PetSkillManager.JingLingChuanChengXiaoHaoZhuanShi && !HuanLeDaiBiManager.GetInstance().HuanledaibiEnough(client, PetSkillManager.JingLingChuanChengXiaoHaoZhuanShi))
						{
							return EPetSkillState.EnoDiamond;
						}
					}
					else if (Global.GetTotalBindTongQianAndTongQianVal(client) < PetSkillManager.JingLingChuanChengXiaoHaoJinBi)
					{
						return EPetSkillState.EnoGold;
					}
					List<PetSkillInfo> srcSkillList = PetSkillManager.GetPetSkillInfo(srcGoodsData);
					List<PetSkillInfo> tarSkillList = PetSkillManager.GetPetSkillInfo(tarGoodsData);
					int pitLoop;
					for (pitLoop = 1; pitLoop < 4; pitLoop++)
					{
						PetSkillInfo srcSkill = srcSkillList.Find((PetSkillInfo _g) => _g.Pit == pitLoop);
						PetSkillInfo tarSkill = tarSkillList.Find((PetSkillInfo _g) => _g.Pit == pitLoop);
						if (srcSkill == null || null == tarSkill)
						{
							return EPetSkillState.EpitWrong;
						}
						if (srcSkill.PitIsOpen && !tarSkill.PitIsOpen)
						{
							return EPetSkillState.EpitWrong;
						}
					}
					if (1 == userMoney)
					{
						if (!GameManager.ClientMgr.SubUserMoney(Global._TCPManager.MySocketListener, Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool, client, PetSkillManager.JingLingChuanChengXiaoHaoZhuanShi, "精灵技能传承", true, true, false, DaiBiSySType.JingLingJiNengChuanCheng))
						{
							return EPetSkillState.EnoDiamond;
						}
					}
					else if (!Global.SubBindTongQianAndTongQian(client, PetSkillManager.JingLingChuanChengXiaoHaoJinBi, "精灵技能传承"))
					{
						return EPetSkillState.EnoGold;
					}
					int random = Global.GetRandomNumber(1, 101);
					if (random > PetSkillManager.JingLingChuanChengGoodsRate)
					{
						result = EPetSkillState.EnoInheritFail;
					}
					else
					{
						long returnMoHe = PetSkillManager.DelGoodsReturnLingJing(tarGoodsData);
						UpdateGoodsArgs tarGoodsArgs = new UpdateGoodsArgs
						{
							RoleID = client.ClientData.RoleID,
							DbID = tarPetID,
							WashProps = null
						};
						tarGoodsArgs.ElementhrtsProps = new List<int>();
						for (int loop = 0; loop < tarSkillList.Count; loop++)
						{
							PetSkillInfo info = (loop < srcSkillList.Count) ? srcSkillList[loop] : tarSkillList[loop];
							tarGoodsArgs.ElementhrtsProps.Add(info.PitIsOpen ? 1 : 0);
							tarGoodsArgs.ElementhrtsProps.Add(info.Level);
							tarGoodsArgs.ElementhrtsProps.Add((loop < srcSkillList.Count) ? info.SkillID : 0);
						}
						Global.UpdateGoodsProp(client, tarGoodsData, tarGoodsArgs, true);
						UpdateGoodsArgs srcGoodsArgs = new UpdateGoodsArgs
						{
							RoleID = client.ClientData.RoleID,
							DbID = srcPetID,
							WashProps = null
						};
						srcGoodsArgs.ElementhrtsProps = new List<int>();
						foreach (PetSkillInfo info in srcSkillList)
						{
                            srcGoodsArgs.ElementhrtsProps.Add(info.PitIsOpen ? 1 : 0);
							srcGoodsArgs.ElementhrtsProps.Add(1);
							srcGoodsArgs.ElementhrtsProps.Add(0);
						}
						Global.UpdateGoodsProp(client, srcGoodsData, srcGoodsArgs, true);
						GameManager.ClientMgr.ModifyMUMoHeValue(client, (int)returnMoHe, "精灵技能传承", true, true, false);
						if (srcGoodsData.Using > 0 || tarGoodsData.Using > 0)
						{
							PetSkillManager.UpdateRolePetSkill(client);
						}
						outProps = string.Format("{0}:{1}", string.Join<int>(",", srcGoodsArgs.ElementhrtsProps.ToArray()), string.Join<int>(",", tarGoodsArgs.ElementhrtsProps.ToArray()));
						result = EPetSkillState.Success;
					}
				}
			}
			return result;
		}

		// Token: 0x06000BB2 RID: 2994 RVA: 0x000B6D64 File Offset: 0x000B4F64
		private static EPetSkillState PetSkillUp(GameClient client, int petID, int pit)
		{
			EPetSkillState result;
			if (!PetSkillManager.IsGongNengOpened(client))
			{
				result = EPetSkillState.EnoOpen;
			}
			else
			{
				GoodsData goodsData = DamonMgr.GetDamonGoodsDataByDbID(client, petID);
				if (goodsData == null || goodsData.GCount <= 0)
				{
					result = EPetSkillState.EnoPet;
				}
				else if (goodsData.Site != 5000)
				{
					result = EPetSkillState.EnoUsing;
				}
				else if (pit < 1 || pit > 4)
				{
					result = EPetSkillState.EpitWrong;
				}
				else
				{
					List<PetSkillInfo> petSkillList = PetSkillManager.GetPetSkillInfo(goodsData);
					PetSkillInfo skillInfo = petSkillList.Find((PetSkillInfo _g) => _g.Pit == pit);
					if (!skillInfo.PitIsOpen)
					{
						result = EPetSkillState.EpitNoOpen;
					}
					else if (skillInfo.SkillID <= 0)
					{
						result = EPetSkillState.EpitSkillNull;
					}
					else
					{
						int maxLevel = PetSkillManager.GetPsUpMaxLevel();
						if (skillInfo.Level >= maxLevel)
						{
							result = EPetSkillState.ElevelMax;
						}
						else
						{
							int oldLevel = skillInfo.Level;
							int nextLevel = skillInfo.Level + 1;
							int lingJingNeed = (int)PetSkillManager.GetPsUpCost(nextLevel);
							long lingjingHave = (long)GameManager.ClientMgr.GetMUMoHeValue(client);
							if (lingjingHave < (long)lingJingNeed)
							{
								result = EPetSkillState.EnoLingJing;
							}
							else
							{
								GameManager.ClientMgr.ModifyMUMoHeValue(client, -lingJingNeed, "精灵技能升级", true, true, false);
								skillInfo.Level = nextLevel;
								UpdateGoodsArgs updateGoodsArgs = new UpdateGoodsArgs
								{
									RoleID = client.ClientData.RoleID,
									DbID = petID,
									WashProps = null
								};
								updateGoodsArgs.ElementhrtsProps = new List<int>();
								foreach (PetSkillInfo info in petSkillList)
								{
									updateGoodsArgs.ElementhrtsProps.Add(info.PitIsOpen ? 1 : 0);
									updateGoodsArgs.ElementhrtsProps.Add(info.Level);
									updateGoodsArgs.ElementhrtsProps.Add(info.SkillID);
								}
								Global.UpdateGoodsProp(client, goodsData, updateGoodsArgs, true);
								PetSkillManager.UpdateRolePetSkill(client);
								EventLogManager.AddPetSkillEvent(client, LogRecordType.PetSkill, EPetSkillLog.Up, new object[]
								{
									petID,
									goodsData.GoodsID,
									pit,
									oldLevel,
									nextLevel
								});
								result = EPetSkillState.Success;
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06000BB3 RID: 2995 RVA: 0x000B6FFC File Offset: 0x000B51FC
		public static List<PetSkillInfo> GetPetSkillInfo(GoodsData data)
		{
			List<PetSkillInfo> list = new List<PetSkillInfo>();
			if (data.ElementhrtsProps == null)
			{
				data.ElementhrtsProps = new List<int>
				{
					0,
					1,
					0,
					0,
					1,
					0,
					0,
					1,
					0,
					0,
					1,
					0
				};
			}
			int pit = 1;
			for (int i = 0; i < data.ElementhrtsProps.Count; i++)
			{
				PetSkillInfo info = new PetSkillInfo();
				info.PitIsOpen = (data.ElementhrtsProps[i++] > 0);
				if (!info.PitIsOpen)
				{
					int openLevel = PetSkillManager.GetPitOpenLevel(pit);
					if (data.Forge_level + 1 >= openLevel)
					{
						info.PitIsOpen = true;
					}
				}
				info.Pit = pit++;
				info.Level = data.ElementhrtsProps[i++];
				info.SkillID = data.ElementhrtsProps[i];
				list.Add(info);
			}
			return list;
		}

		// Token: 0x06000BB4 RID: 2996 RVA: 0x000B725C File Offset: 0x000B545C
		private static EPetSkillState PetSkillAwake(GameClient client, int petID, List<int> lockPitList, out string result)
		{
			result = "";
			EPetSkillState result2;
			if (!PetSkillManager.IsGongNengOpened(client))
			{
				result2 = EPetSkillState.EnoOpen;
			}
			else
			{
				GoodsData goodsData = DamonMgr.GetDamonGoodsDataByDbID(client, petID);
				if (goodsData == null || goodsData.GCount <= 0)
				{
					result2 = EPetSkillState.EnoPet;
				}
				else if (goodsData.Site != 5000)
				{
					result2 = EPetSkillState.EnoUsing;
				}
				else
				{
					List<PetSkillInfo> petSkillList = PetSkillManager.GetPetSkillInfo(goodsData);
					int diamondNeed = 0;
					if (lockPitList.Count > 0)
					{
						foreach (int lockPit in lockPitList)
						{
							if (lockPit > 4)
							{
								return EPetSkillState.EpitWrong;
							}
							if (!petSkillList[lockPit - 1].PitIsOpen)
							{
								return EPetSkillState.EpitNoOpen;
							}
						}
						diamondNeed = PetSkillManager.GetPitLockCost(lockPitList.Count);
						if (diamondNeed > 0 && client.ClientData.UserMoney < diamondNeed)
						{
							return EPetSkillState.EnoDiamond;
						}
					}
					int awakeCount = PetSkillManager.GetUpCount(client);
					int lingJingNeed = PetSkillManager.GetSkillAwakeCost(awakeCount);
					long lingjingHave = (long)GameManager.ClientMgr.GetMUMoHeValue(client);
					if (lingjingHave < (long)lingJingNeed)
					{
						result2 = EPetSkillState.EnoLingJing;
					}
					else
					{
						List<PetSkillInfo> openList = petSkillList.FindAll((PetSkillInfo _g) => _g.PitIsOpen);
						if (openList == null || openList.Count <= 0)
						{
							result2 = EPetSkillState.EpitNoOpen;
						}
						else
						{
							List<PetSkillInfo> canAwakeSkillList;
							if (lockPitList != null && lockPitList.Count > 0)
							{
								IEnumerable<PetSkillInfo> temp = from info in openList
								where info.PitIsOpen && lockPitList.IndexOf(info.Pit) < 0
								select info;
								if (!temp.Any<PetSkillInfo>())
								{
									return EPetSkillState.EnoPitAwake;
								}
								canAwakeSkillList = temp.ToList<PetSkillInfo>();
							}
							else
							{
								canAwakeSkillList = openList;
							}
							IEnumerable<PetSkillInfo> t = from info in canAwakeSkillList
							where info.PitIsOpen && info.SkillID <= 0
							select info;
							if (t.Any<PetSkillInfo>())
							{
								List<PetSkillInfo> list = t.ToList<PetSkillInfo>();
								canAwakeSkillList = list;
							}
							int skRand = Global.GetRandomNumber(0, canAwakeSkillList.Count);
							PetSkillInfo nowAwakeInfo = canAwakeSkillList[skRand];
							List<int> canAwakeSkillIDList = new List<int>();
							IEnumerable<KeyValuePair<int, PetSkillAwakeInfo>> tt = from p in PetSkillManager._psDic
							where !(from g in petSkillList
							select g.SkillID).Contains(p.Value.SkillID)
							select p;
							if (!tt.Any<KeyValuePair<int, PetSkillAwakeInfo>>())
							{
								result2 = EPetSkillState.EnoSkillAwake;
							}
							else
							{
								int nowAwakeSkillID = 0;
								int seed = tt.Sum((KeyValuePair<int, PetSkillAwakeInfo> _s) => _s.Value.Rate);
								int skillRand = Global.GetRandomNumber(0, seed);
								int sum = 0;
								foreach (KeyValuePair<int, PetSkillAwakeInfo> info3 in tt)
								{
									nowAwakeSkillID = info3.Key;
									int rate = info3.Value.Rate;
									sum += info3.Value.Rate;
									if (sum >= skillRand)
									{
										break;
									}
								}
								int oldSkillID = nowAwakeInfo.SkillID;
								nowAwakeInfo.SkillID = nowAwakeSkillID;
								if (diamondNeed > 0 && !GameManager.ClientMgr.SubUserMoney(client, diamondNeed, "精灵技能领悟", true, true, true, true, DaiBiSySType.None))
								{
									result2 = EPetSkillState.EnoDiamond;
								}
								else
								{
									GameManager.ClientMgr.ModifyMUMoHeValue(client, -lingJingNeed, "精灵技能领悟", true, true, false);
									PetSkillManager.ModifyUpCount(client, awakeCount + 1);
									UpdateGoodsArgs updateGoodsArgs = new UpdateGoodsArgs
									{
										RoleID = client.ClientData.RoleID,
										DbID = petID,
										WashProps = null
									};
									updateGoodsArgs.ElementhrtsProps = new List<int>();
									foreach (PetSkillInfo info2 in petSkillList)
									{
										updateGoodsArgs.ElementhrtsProps.Add(info2.PitIsOpen ? 1 : 0);
										updateGoodsArgs.ElementhrtsProps.Add(info2.Level);
										updateGoodsArgs.ElementhrtsProps.Add(info2.SkillID);
									}
									Global.UpdateGoodsProp(client, goodsData, updateGoodsArgs, true);
									result = string.Join<int>(",", updateGoodsArgs.ElementhrtsProps.ToArray());
									PetSkillManager.UpdateRolePetSkill(client);
									EventLogManager.AddPetSkillEvent(client, LogRecordType.PetSkill, EPetSkillLog.Awake, new object[]
									{
										petID,
										goodsData.GoodsID,
										nowAwakeInfo.Pit,
										oldSkillID,
										nowAwakeSkillID
									});
									result2 = EPetSkillState.Success;
								}
							}
						}
					}
				}
			}
			return result2;
		}

		// Token: 0x06000BB5 RID: 2997 RVA: 0x000B77B4 File Offset: 0x000B59B4
		public static int GetUpCount(GameClient client)
		{
			int count = 0;
			int dayOld = 0;
			List<int> data = Global.GetRoleParamsIntListFromDB(client, "PetSkillUpCount");
			if (data != null && data.Count > 0)
			{
				dayOld = data[0];
			}
			int day = Global.GetOffsetDayNow();
			if (dayOld == day)
			{
				count = data[1];
			}
			else
			{
				PetSkillManager.ModifyUpCount(client, count);
			}
			return count;
		}

		// Token: 0x06000BB6 RID: 2998 RVA: 0x000B7820 File Offset: 0x000B5A20
		public static void ModifyUpCount(GameClient client, int count)
		{
			List<int> dataList = new List<int>();
			dataList.AddRange(new int[]
			{
				Global.GetOffsetDayNow(),
				count
			});
			Global.SaveRoleParamsIntListToDB(client, dataList, "PetSkillUpCount", true);
		}

		// Token: 0x06000BB7 RID: 2999 RVA: 0x000B78D0 File Offset: 0x000B5AD0
		public static long DelGoodsReturnLingJing(GoodsData goodsData)
		{
			long sum = 0L;
			List<PetSkillInfo> petSkillList = PetSkillManager.GetPetSkillInfo(goodsData);
			IEnumerable<PetSkillInfo> temp = from info in petSkillList
			where info.PitIsOpen && info.Level > 0
			select info;
			long result;
			if (!temp.Any<PetSkillInfo>())
			{
				result = sum;
			}
			else
			{
				using (IEnumerator<PetSkillInfo> enumerator = temp.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						PetSkillInfo info = enumerator.Current;
						sum += (from levelInfo in PetSkillManager._psLevelUpDic
						where levelInfo.Key <= info.Level
						select levelInfo.Value).Sum();
					}
				}
				result = sum;
			}
			return result;
		}

		// Token: 0x06000BB8 RID: 3000 RVA: 0x000B7A10 File Offset: 0x000B5C10
		public static void UpdateRolePetSkill(GameClient client)
		{
			List<PassiveSkillData> resultList = new List<PassiveSkillData>();
			List<GoodsData> petList = client.ClientData.DamonGoodsDataList;
			GoodsData warPet = client.ClientData.DamonGoodsDataList.Find((GoodsData _g) => _g.Using > 0);
			if (warPet != null)
			{
				List<PetSkillInfo> allSkillList = new List<PetSkillInfo>();
				List<PetSkillInfo> petSkillList = PetSkillManager.GetPetSkillInfo(warPet);
				IEnumerable<PetSkillInfo> temp = from info in petSkillList
				where info.PitIsOpen && info.SkillID > 0
				select info;
				if (temp.Any<PetSkillInfo>())
				{
					foreach (PetSkillInfo t in temp)
					{
						SystemXmlItem systemMagic = null;
						if (GameManager.SystemMagicsMgr.SystemXmlItemDict.TryGetValue(t.SkillID, out systemMagic))
						{
							resultList.Add(new PassiveSkillData
							{
								skillId = t.SkillID,
								skillLevel = t.Level,
								triggerRate = (int)(systemMagic.GetDoubleValue("TriggerOdds") * 100.0),
								triggerType = systemMagic.GetIntValue("TriggerType", -1),
								coolDown = systemMagic.GetIntValue("CDTime", -1),
								triggerCD = systemMagic.GetIntValue("TriggerCD", -1)
							});
						}
					}
				}
			}
			client.passiveSkillModule.UpdateSkillList(resultList);
			JingLingQiYuanManager.getInstance().RefreshProps(client, true);
		}

		// Token: 0x06000BB9 RID: 3001 RVA: 0x000B7BD4 File Offset: 0x000B5DD4
		public static void InitConfig()
		{
			string str = GameManager.systemParamsList.GetParamValueByName("JingLingChuanChengGoodsRate");
			PetSkillManager.JingLingChuanChengGoodsRate = Global.SafeConvertToInt32(str);
			str = GameManager.systemParamsList.GetParamValueByName("JingLingChuanChengXiaoHaoJinBi");
			PetSkillManager.JingLingChuanChengXiaoHaoJinBi = Global.SafeConvertToInt32(str);
			str = GameManager.systemParamsList.GetParamValueByName("JingLingChuanChengXiaoHaoZhuanShi");
			PetSkillManager.JingLingChuanChengXiaoHaoZhuanShi = Global.SafeConvertToInt32(str);
			PetSkillManager.LoadPsInfo();
			PetSkillManager.LoadPsUpInfo();
			PetSkillManager.LoadPitOpenLevel();
			PetSkillManager.LoadPitLockCost();
		}

		// Token: 0x06000BBA RID: 3002 RVA: 0x000B7C4C File Offset: 0x000B5E4C
		private static void LoadPsInfo()
		{
			string fileName = Global.GameResPath("Config/PetSkill.xml");
			XElement xml = CheckHelper.LoadXml(fileName, true);
			if (null != xml)
			{
				try
				{
					PetSkillManager._psDic.Clear();
					IEnumerable<XElement> xmlItems = xml.Elements();
					foreach (XElement xmlItem in xmlItems)
					{
						if (xmlItem != null)
						{
							PetSkillAwakeInfo config = new PetSkillAwakeInfo();
							config.SkillID = Convert.ToInt32(Global.GetDefAttributeStr(xmlItem, "SkillID", "0"));
							config.RateMin = Convert.ToInt32(Global.GetDefAttributeStr(xmlItem, "StartValues", "0"));
							config.RateMax = Convert.ToInt32(Global.GetDefAttributeStr(xmlItem, "EndValues", "0"));
							config.Rate = config.RateMax - config.RateMin + 1;
							PetSkillManager._psDic.Add(config.SkillID, config);
						}
					}
				}
				catch (Exception ex)
				{
					LogManager.WriteLog(LogTypes.Fatal, string.Format("加载[{0}]时出错!!!", fileName), null, true);
				}
			}
		}

		// Token: 0x06000BBB RID: 3003 RVA: 0x000B7DA8 File Offset: 0x000B5FA8
		public static void LoadPsUpInfo()
		{
			string fileName = Global.GameResPath("Config/PetSkillLevelup.xml");
			XElement xml = CheckHelper.LoadXml(fileName, true);
			if (null != xml)
			{
				try
				{
					PetSkillManager._psLevelUpDic.Clear();
					IEnumerable<XElement> xmlItems = xml.Elements();
					foreach (XElement xmlItem in xmlItems)
					{
						if (xmlItem != null)
						{
							int level = Convert.ToInt32(Global.GetDefAttributeStr(xmlItem, "Level", "0"));
							long cost = Convert.ToInt64(Global.GetDefAttributeStr(xmlItem, "Cost", "0"));
							if (!PetSkillManager._psLevelUpDic.ContainsKey(level))
							{
								PetSkillManager._psLevelUpDic.Add(level, cost);
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

		// Token: 0x06000BBC RID: 3004 RVA: 0x000B7EC4 File Offset: 0x000B60C4
		public static void LoadPitOpenLevel()
		{
			try
			{
				PetSkillManager._pitOpenDic.Clear();
				string str = GameManager.systemParamsList.GetParamValueByName("PatSkillCostLevel");
				if (!string.IsNullOrEmpty(str))
				{
					string[] arr = str.Split(new char[]
					{
						'|'
					});
					foreach (string s in arr)
					{
						string[] r = s.Split(new char[]
						{
							','
						});
						int pit = int.Parse(r[0]);
						int level = int.Parse(r[1]);
						if (!PetSkillManager._pitOpenDic.ContainsKey(pit))
						{
							PetSkillManager._pitOpenDic.Add(pit, level);
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Fatal, string.Format("加载[{0}]时出错!!!", "PatSkillCostLevel"), null, true);
			}
		}

		// Token: 0x06000BBD RID: 3005 RVA: 0x000B7FC4 File Offset: 0x000B61C4
		public static void LoadPitLockCost()
		{
			try
			{
				PetSkillManager._pitLockDic.Clear();
				string str = GameManager.systemParamsList.GetParamValueByName("PatSkillCostZuanShi");
				if (!string.IsNullOrEmpty(str))
				{
					string[] arr = str.Split(new char[]
					{
						'|'
					});
					foreach (string s in arr)
					{
						string[] r = s.Split(new char[]
						{
							','
						});
						int pit = int.Parse(r[0]);
						int value = int.Parse(r[1]);
						if (!PetSkillManager._pitLockDic.ContainsKey(pit))
						{
							PetSkillManager._pitLockDic.Add(pit, value);
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Fatal, string.Format("加载[{0}]时出错!!!", "PatSkillCostLevel"), null, true);
			}
		}

		// Token: 0x06000BBE RID: 3006 RVA: 0x000B80C4 File Offset: 0x000B62C4
		public static PetSkillAwakeInfo GetPsInfo(int id)
		{
			PetSkillAwakeInfo result;
			if (PetSkillManager._psDic.ContainsKey(id))
			{
				result = PetSkillManager._psDic[id];
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000BBF RID: 3007 RVA: 0x000B80F8 File Offset: 0x000B62F8
		public static int GetPsUpMaxLevel()
		{
			int result;
			if (PetSkillManager._psLevelUpDic == null && PetSkillManager._psLevelUpDic.Count <= 0)
			{
				result = 0;
			}
			else
			{
				result = PetSkillManager._psLevelUpDic.Keys.Max();
			}
			return result;
		}

		// Token: 0x06000BC0 RID: 3008 RVA: 0x000B813C File Offset: 0x000B633C
		public static long GetPsUpCost(int nextLevel)
		{
			long result;
			if (PetSkillManager._psLevelUpDic.ContainsKey(nextLevel))
			{
				result = PetSkillManager._psLevelUpDic[nextLevel];
			}
			else
			{
				result = 0L;
			}
			return result;
		}

		// Token: 0x06000BC1 RID: 3009 RVA: 0x000B8170 File Offset: 0x000B6370
		public static int GetPitOpenLevel(int pit)
		{
			int result;
			if (PetSkillManager._pitOpenDic.ContainsKey(pit))
			{
				result = PetSkillManager._pitOpenDic[pit];
			}
			else
			{
				result = 0;
			}
			return result;
		}

		// Token: 0x06000BC2 RID: 3010 RVA: 0x000B81A4 File Offset: 0x000B63A4
		public static int GetPitLockCost(int count)
		{
			int result;
			if (PetSkillManager._pitLockDic.ContainsKey(count))
			{
				result = PetSkillManager._pitLockDic[count];
			}
			else
			{
				result = 0;
			}
			return result;
		}

		// Token: 0x06000BC3 RID: 3011 RVA: 0x000B81D8 File Offset: 0x000B63D8
		public static int GetSkillAwakeCost(int count)
		{
			int[] costList = GameManager.systemParamsList.GetParamValueIntArrayByName("PatSkillCostLingJing", ',');
			if (count >= costList.Length)
			{
				count = costList.Length - 1;
			}
			return costList[count];
		}

		// Token: 0x06000BC4 RID: 3012 RVA: 0x000B8210 File Offset: 0x000B6410
		public static bool IsGongNengOpened(GameClient client)
		{
			return !GameFuncControlManager.IsGameFuncDisabled(GameFuncType.System2Dot0) && GlobalNew.IsGongNengOpened(client, GongNengIDs.PetSkill, false) && GameManager.VersionSystemOpenMgr.IsVersionSystemOpen("PetSkill");
		}

		// Token: 0x040012F5 RID: 4853
		private const int PIT_MIN = 1;

		// Token: 0x040012F6 RID: 4854
		private const int PIT_MAX = 4;

		// Token: 0x040012F7 RID: 4855
		private const int UP_LEVEL_MAX = 5;

		// Token: 0x040012F8 RID: 4856
		private const int STATUE_COUNT = 8;

		// Token: 0x040012F9 RID: 4857
		private const int RANDOM_SEED_AWAKE = 100000;

		// Token: 0x040012FA RID: 4858
		public static int _gmRate = 1;

		// Token: 0x040012FB RID: 4859
		private static PetSkillManager instance = new PetSkillManager();

		// Token: 0x040012FC RID: 4860
		private static Dictionary<int, PetSkillAwakeInfo> _psDic = new Dictionary<int, PetSkillAwakeInfo>();

		// Token: 0x040012FD RID: 4861
		private static Dictionary<int, long> _psLevelUpDic = new Dictionary<int, long>();

		// Token: 0x040012FE RID: 4862
		private static Dictionary<int, int> _pitOpenDic = new Dictionary<int, int>();

		// Token: 0x040012FF RID: 4863
		private static Dictionary<int, int> _pitLockDic = new Dictionary<int, int>();

		// Token: 0x04001300 RID: 4864
		private static int JingLingChuanChengGoodsRate;

		// Token: 0x04001301 RID: 4865
		private static int JingLingChuanChengXiaoHaoJinBi;

		// Token: 0x04001302 RID: 4866
		private static int JingLingChuanChengXiaoHaoZhuanShi;
	}
}
