﻿using System;
using System.Text;
using GameDBServer.DB;
using Server.Data;
using Server.Tools;

namespace GameDBServer.Server.CmdProcessor
{
	
	public class ZhanMengBuildGetBufferCmdProcessor : ICmdProcessor
	{
		
		private ZhanMengBuildGetBufferCmdProcessor()
		{
			TCPCmdDispatcher.getInstance().registerProcessor(602, this);
		}

		
		public static ZhanMengBuildGetBufferCmdProcessor getInstance()
		{
			return ZhanMengBuildGetBufferCmdProcessor.instance;
		}

		
		public void processCmd(GameServerClient client, int nID, byte[] cmdParams, int count)
		{
			string cmdData = null;
			try
			{
				cmdData = new UTF8Encoding().GetString(cmdParams, 0, count);
			}
			catch (Exception)
			{
				LogManager.WriteLog(LogTypes.Error, string.Format("解析指令字符串错误, CMD={0}", (TCPGameServerCmds)nID), null, true);
				client.sendCmd(30767, "0");
				return;
			}
			string[] fields = cmdData.Split(new char[]
			{
				':'
			});
			if (fields.Length != 5)
			{
				LogManager.WriteLog(LogTypes.Error, string.Format("指令参数个数错误, CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData), null, true);
				client.sendCmd(30767, "0");
			}
			else
			{
				int roleID = Convert.ToInt32(fields[0]);
				int bhid = Convert.ToInt32(fields[1]);
				int buildType = Convert.ToInt32(fields[2]);
				int convertCost = Convert.ToInt32(fields[3]);
				int toLevel = Convert.ToInt32(fields[4]);
				DBManager dbMgr = DBManager.getInstance();
				DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(ref roleID);
				if (null == dbRoleInfo)
				{
					LogManager.WriteLog(LogTypes.Error, string.Format("发起请求的角色不存在，CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID), null, true);
					client.sendCmd(30767, "0");
				}
				else
				{
					BangHuiDetailData bangHuiDetailData = DBQuery.QueryBangHuiInfoByID(dbMgr, bhid);
					if (null == bangHuiDetailData)
					{
						string strcmd = string.Format("{0}", -1000);
						client.sendCmd(nID, strcmd);
					}
					else
					{
						switch (buildType)
						{
						case 1:
							if (toLevel > bangHuiDetailData.QiLevel)
							{
								toLevel = -1;
							}
							break;
						case 2:
							if (toLevel > bangHuiDetailData.JiTan)
							{
								toLevel = -1;
							}
							break;
						case 3:
							if (toLevel > bangHuiDetailData.JunXie)
							{
								toLevel = -1;
							}
							break;
						case 4:
							if (toLevel > bangHuiDetailData.GuangHuan)
							{
								toLevel = -1;
							}
							break;
						default:
							toLevel = -1;
							break;
						}
						if (toLevel < 0)
						{
							string strcmd = string.Format("{0}", -1110);
							client.sendCmd(nID, strcmd);
						}
						else if (dbRoleInfo.BangGong < Math.Abs(convertCost))
						{
							string strcmd = string.Format("{0}", -1110);
							client.sendCmd(nID, strcmd);
						}
						else if (!DBWriter.UpdateRoleBangGong(dbMgr, roleID, dbRoleInfo.BGDayID1, dbRoleInfo.BGMoney, dbRoleInfo.BGDayID2, dbRoleInfo.BGGoods, dbRoleInfo.BangGong - convertCost))
						{
							string strcmd = string.Format("{0}", -1110);
							client.sendCmd(nID, strcmd);
						}
						else
						{
							dbRoleInfo.BangGong -= Math.Abs(convertCost);
							string strcmd = string.Format("{0}", 1);
							client.sendCmd(nID, strcmd);
						}
					}
				}
			}
		}

		
		private static ZhanMengBuildGetBufferCmdProcessor instance = new ZhanMengBuildGetBufferCmdProcessor();
	}
}
