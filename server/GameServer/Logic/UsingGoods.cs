﻿using System;
using System.Collections.Generic;
using Server.Data;
using Server.Tools;

namespace GameServer.Logic
{
	
	public class UsingGoods
	{
		
		public static int ProcessUsingGoodsVerify(GameClient client, int goodsID, int binding, out List<MagicActionItem> magicActionItemList, out int categoriy, int subNum)
		{
			magicActionItemList = null;
			categoriy = 0;
			int result;
			if (!GameManager.SystemMagicActionMgr.GoodsActionsDict.TryGetValue(goodsID, out magicActionItemList) || null == magicActionItemList)
			{
				SystemXmlItem goodsXmlItem;
				if (GameManager.SystemGoods.SystemXmlItemDict.TryGetValue(goodsID, out goodsXmlItem) && goodsXmlItem != null && goodsXmlItem.GetIntValue("BaoguoID", -1) > 0)
				{
					result = 1;
				}
				else
				{
					result = -3;
				}
			}
			else
			{
				SystemXmlItem systemGoodsItem = null;
				if (!GameManager.SystemGoods.SystemXmlItemDict.TryGetValue(goodsID, out systemGoodsItem))
				{
					result = -4;
				}
				else
				{
					categoriy = systemGoodsItem.GetIntValue("Categoriy", -1);
					if (categoriy >= 0 && categoriy < 49)
					{
						result = -5;
					}
					else
					{
						if (230 == categoriy)
						{
							for (int i = 0; i < magicActionItemList.Count; i++)
							{
								if (!MagicAction.ProcessAction(client, client, magicActionItemList[i].MagicActionID, magicActionItemList[i].MagicActionParams, -1, -1, 0, 1, -1, 0, binding, -1, goodsID, true, true, 1.0, subNum, 0.0))
								{
									return -1;
								}
							}
						}
						result = 0;
					}
				}
			}
			return result;
		}

		
		public static int ProcessUsingGoods(GameClient client, int goodsID, int binding, List<MagicActionItem> magicActionItemList, int categoriy, int subNum)
		{
			bool bItemAddVal = false;
			if (230 == categoriy)
			{
				bItemAddVal = true;
			}
			bool notify = true;
			if (categoriy == 230)
			{
				notify = true;
				client.ClientData.ClearAwardRecord(RoleAwardMsg.RandomBaoXiang);
				client.ClientData.RoleAwardMsgType = RoleAwardMsg.RandomBaoXiang;
			}
			for (int i = 0; i < magicActionItemList.Count; i++)
			{
				MagicAction.ProcessAction(client, client, magicActionItemList[i].MagicActionID, magicActionItemList[i].MagicActionParams, -1, -1, 0, 1, -1, 0, binding, -1, goodsID, bItemAddVal, false, 1.0, subNum, 0.0);
			}
			if (notify)
			{
				List<GoodsData> goodsList = client.ClientData.GetAwardRecord(RoleAwardMsg.RandomBaoXiang);
				if (goodsList != null && goodsList.Count > 0)
				{
					GameManager.ClientMgr.NotifyImportantMsgWithGoods(client, MsgWithGoodsType.GoodsAwards, ShowGameInfoTypes.OnlyChatBox, goodsList, "", null);
					client.ClientData.ClearAwardRecord(RoleAwardMsg.RandomBaoXiang);
				}
			}
			return 0;
		}

		
		public static bool IfProcessSeveralTimesAction(int goodsID, string toType)
		{
			List<MagicActionItem> magicActionItemList = UsingGoods.GetMagicActionListByGoodsID(goodsID);
			bool result;
			if (magicActionItemList == null)
			{
				result = false;
			}
			else
			{
				bool IsModifyYuanBao = false;
				for (int i = 0; i < magicActionItemList.Count; i++)
				{
					MagicActionItem item = magicActionItemList[i];
					if (item.MagicActionID <= MagicActionIDs.ActionSeveralTimesBegin || item.MagicActionID >= MagicActionIDs.ActionSeveralTimesEnd)
					{
						return false;
					}
					if (item.MagicActionID == MagicActionIDs.ADD_DJ)
					{
						IsModifyYuanBao = true;
					}
				}
				result = (!StringUtil.IsEqualIgnoreCase(toType, "UseYuanBao") || !IsModifyYuanBao);
			}
			return result;
		}

		
		public static List<MagicActionItem> GetMagicActionListByGoodsID(int goodsID)
		{
			List<MagicActionItem> magicActionItemList = null;
			List<MagicActionItem> result;
			if (!GameManager.SystemMagicActionMgr.GoodsActionsDict.TryGetValue(goodsID, out magicActionItemList))
			{
				result = null;
			}
			else
			{
				result = magicActionItemList;
			}
			return result;
		}
	}
}
