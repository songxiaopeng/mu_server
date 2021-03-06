﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameDBServer.DB;
using GameDBServer.Server;
using MySQLDriverCS;
using Server.Data;
using Server.Protocol;
using Server.Tools;

namespace GameDBServer.Logic
{
	
	internal class CFirstChargeMgr
	{
		
		
		
		public static SingleChargeData ChargeData
		{
			get
			{
				SingleChargeData chargeData;
				lock (CFirstChargeMgr.SingleChargeDataMutex)
				{
					chargeData = CFirstChargeMgr._ChargeData;
				}
				return chargeData;
			}
			set
			{
				lock (CFirstChargeMgr.SingleChargeDataMutex)
				{
					CFirstChargeMgr._ChargeData = value;
				}
			}
		}

		
		public static TCPProcessCmdResults FirstChargeConfig(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
		{
			tcpOutPacket = null;
			try
			{
				SingleChargeData chargeData = DataHelper.BytesToObject<SingleChargeData>(data, 0, count);
				CFirstChargeMgr.ChargeData = chargeData;
				byte[] retBytes = DataHelper.ObjectToBytes<int>(1);
				tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, retBytes, 0, retBytes.Length, nID);
			}
			catch (Exception ex)
			{
				DataHelper.WriteFormatExceptionLog(ex, "", false, false);
			}
			return TCPProcessCmdResults.RESULT_DATA;
		}

		
		private static bool HasGetFirstbindMoney(int money, string[] binddatalist)
		{
			bool result;
			if (null == binddatalist)
			{
				result = false;
			}
			else
			{
				int len = binddatalist.Length;
				for (int i = 0; i < len; i++)
				{
					if (binddatalist[i] == money.ToString())
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		
		private static List<int> MuiltchargeGetBindmoney(int addMoney, int nPlatfromID, string[] binddatalist, CFirstChargeMgr.ChargeType eChargeType, SingleChargeData chargeData)
		{
			List<int> templist = new List<int>();
			switch (eChargeType)
			{
			case CFirstChargeMgr.ChargeType.YingYongBao:
			{
				List<int> typelist = chargeData.singleData.Keys.ToList<int>();
				typelist.Sort();
				int tempmoney = 0;
				int i = typelist.Count - 1;
				while (i >= 0)
				{
					if (typelist[i] <= addMoney)
					{
						if (typelist[i] != chargeData.YueKaMoney || chargeData.ChargePlatType == 1)
						{
							if (!CFirstChargeMgr.HasGetFirstbindMoney(typelist[i], binddatalist))
							{
								if (tempmoney + typelist[i] <= addMoney)
								{
									templist.Add(typelist[i]);
									tempmoney += typelist[i];
								}
							}
						}
					}
					IL_CB:
					i--;
					continue;
					goto IL_CB;
				}
				break;
			}
			case CFirstChargeMgr.ChargeType.GangAoTai:
			{
				List<int> typelist = chargeData.singleData.Keys.ToList<int>();
				typelist.Sort();
				for (int i = typelist.Count - 1; i >= 0; i--)
				{
					if (typelist[i] <= addMoney)
					{
						if (chargeData.YueKaBangZuan == 1 || typelist[i] != chargeData.YueKaMoney)
						{
							if (!CFirstChargeMgr.HasGetFirstbindMoney(typelist[i], binddatalist))
							{
								templist.Add(typelist[i]);
								break;
							}
						}
					}
				}
				break;
			}
			default:
			{
				int nValue = 0;
				chargeData.singleData.TryGetValue(addMoney, out nValue);
				if (nValue > 0)
				{
					if (chargeData.YueKaBangZuan == 1 || addMoney != chargeData.YueKaMoney || chargeData.ChargePlatType == 1)
					{
						if (!CFirstChargeMgr.HasGetFirstbindMoney(addMoney, binddatalist))
						{
							templist.Add(addMoney);
						}
					}
				}
				break;
			}
			}
			return templist;
		}

		
		public static void SendToRolebindgold(DBManager dbMgr, string uid, int rid, int addMoney, SingleChargeData chargeData)
		{
			if (chargeData == null)
			{
				LogManager.WriteException(string.Concat(new object[]
				{
					"送绑钻失败，配置表信息为空 uid=",
					uid,
					" money=",
					addMoney
				}));
			}
			else
			{
				string data = CFirstChargeMgr.GetFirstChargeInfo(dbMgr, uid);
				string strPlat = (uid.Length >= 4) ? uid.Substring(0, 4) : "";
				int nPlatformID = 1;
				if (strPlat == "APPS")
				{
					nPlatformID = 2;
				}
				CFirstChargeMgr.ChargeType type = CFirstChargeMgr.ChargeType.Normal;
				string strYYB = (uid.Length >= 3) ? uid.Substring(0, 3) : "";
				if (strYYB == "YYB")
				{
					type = CFirstChargeMgr.ChargeType.YingYongBao;
				}
				else if (strYYB == "GAT" || strYYB == "430")
				{
					type = CFirstChargeMgr.ChargeType.GangAoTai;
				}
				string[] datalist = null;
				if (!string.IsNullOrEmpty(data))
				{
					datalist = data.Split(new char[]
					{
						','
					});
				}
				List<int> listAddMoney = CFirstChargeMgr.MuiltchargeGetBindmoney(addMoney, nPlatformID, datalist, type, chargeData);
				if (listAddMoney != null)
				{
					for (int i = 0; i < listAddMoney.Count; i++)
					{
						if (!string.IsNullOrEmpty(data))
						{
							data = data + "," + listAddMoney[i];
						}
						else
						{
							data = string.Concat(listAddMoney[i]);
						}
					}
					if (!CFirstChargeMgr.UpdateFirstCharge(dbMgr, uid, data, 0))
					{
						LogManager.WriteException(string.Concat(new object[]
						{
							"送绑钻失败，保存数据库失败 uid=",
							uid,
							" money=",
							addMoney
						}));
					}
					else
					{
						for (int i = 0; i < listAddMoney.Count; i++)
						{
							int bindMoney = chargeData.singleData[listAddMoney[i]];
							string gmCmdData = string.Format("-updateBindgold {0} {1} {2} {3}", new object[]
							{
								uid,
								rid,
								bindMoney,
								data
							});
							ChatMsgManager.AddGMCmdChatMsg(-1, gmCmdData);
						}
					}
				}
			}
		}

		
		public static string GetFirstChargeInfo(DBManager dbMgr, string uid)
		{
			string resoult = "-1";
			MySQLConnection conn = null;
			try
			{
				conn = dbMgr.DBConns.PopDBConnection();
				string cmdText = string.Format("SELECT charge_info FROM t_firstcharge WHERE uid = '{0}'", uid);
				MySQLCommand cmd = new MySQLCommand(cmdText, conn);
				MySQLDataReader reader = cmd.ExecuteReaderEx();
				try
				{
					if (reader.Read())
					{
						resoult = reader["charge_info"].ToString();
						if (string.IsNullOrEmpty(resoult))
						{
							resoult = "-1";
						}
					}
				}
				catch (Exception ex)
				{
					LogManager.WriteException("GetFirstChargeInfo excepton=" + ex.ToString());
					resoult = "-2";
				}
				GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);
				cmd.Dispose();
				cmd = null;
			}
			finally
			{
				if (null != conn)
				{
					dbMgr.DBConns.PushDBConnection(conn);
				}
			}
			return resoult;
		}

		
		public static bool UpdateFirstCharge(DBManager dbMgr, string userId, string chargeinfo, int notget = 0)
		{
			bool ret = false;
			using (MyDbConnection3 conn = new MyDbConnection3(false))
			{
				string cmdText = string.Format("REPLACE  INTO t_firstcharge (uid, charge_info, notget) VALUES('{0}', '{1}', '{2}')", userId, chargeinfo, notget);
				ret = conn.ExecuteNonQueryBool(cmdText, 0);
			}
			return ret;
		}

		
		public static TCPProcessCmdResults ProcessQueryUserFirstCharge(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
		{
			tcpOutPacket = null;
			string cmdData = null;
			try
			{
				cmdData = new UTF8Encoding().GetString(data, 0, count);
			}
			catch (Exception)
			{
				LogManager.WriteLog(LogTypes.Error, string.Format("解析指令字符串错误, CMD={0}", (TCPGameServerCmds)nID), null, true);
				tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", 30767);
				return TCPProcessCmdResults.RESULT_DATA;
			}
			try
			{
				string[] fields = cmdData.Split(new char[]
				{
					':'
				});
				if (fields.Length != 1)
				{
					LogManager.WriteLog(LogTypes.Error, string.Format("指令参数个数错误, CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData), null, true);
					tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", 30767);
					return TCPProcessCmdResults.RESULT_DATA;
				}
				string uid = fields[0];
				DBUserInfo info = dbMgr.GetDBUserInfo(uid);
				if (null == info)
				{
					LogManager.WriteLog(LogTypes.Error, string.Format("发起请求的账号不存在，CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, uid), null, true);
					tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", 30767);
					return TCPProcessCmdResults.RESULT_DATA;
				}
				DBUserInfo userInfo = dbMgr.GetDBUserInfo(uid);
				string ret = CFirstChargeMgr.GetFirstChargeInfo(dbMgr, uid);
				if (ret != "-2")
				{
					tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, ret, nID);
				}
				else
				{
					tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", 30767);
				}
				return TCPProcessCmdResults.RESULT_DATA;
			}
			catch (Exception e)
			{
				LogManager.WriteException("ProcessSaveUserFirstCharge:" + e.ToString());
			}
			return TCPProcessCmdResults.RESULT_DATA;
		}

		
		private static SingleChargeData _ChargeData = null;

		
		private static object SingleChargeDataMutex = new object();

		
		private enum ChargeType
		{
			
			Normal,
			
			YingYongBao,
			
			GangAoTai
		}
	}
}
