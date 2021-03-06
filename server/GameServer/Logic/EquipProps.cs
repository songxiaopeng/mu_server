﻿using System;
using System.Collections.Generic;
using Server.Tools;

namespace GameServer.Logic
{
	
	public class EquipProps
	{
		
		public void ParseEquipProps(SystemXmlItem systemGoods, out EquipPropItem equipPropItem)
		{
			equipPropItem = null;
			string props = systemGoods.GetStringValue("EquipProps");
			string[] fields = props.Split(new char[]
			{
				','
			});
			if (fields.Length != 177)
			{
				LogManager.WriteLog(LogTypes.Fatal, string.Format("解析物品属性失败: EquipID={0},EquipProps属性期望个数{1}，实际个数{2}", systemGoods.GetIntValue("ID", -1), 177, fields.Length), null, true);
			}
			double[] arryDoubles = null;
			try
			{
				arryDoubles = Global.StringArray2DoubleArray(fields);
			}
			catch (Exception)
			{
				LogManager.WriteLog(LogTypes.Error, string.Format("转换物品属性数组: EquipID={0}", systemGoods.GetIntValue("ID", -1)), null, true);
				return;
			}
			equipPropItem = new EquipPropItem();
			int i = 0;
			while (i < 177 && i < arryDoubles.Length)
			{
				equipPropItem.ExtProps[i] = arryDoubles[i];
				i++;
			}
		}

		
		public void ParseEquipProps(string props, out EquipPropItem equipPropItem)
		{
			equipPropItem = null;
			string[] fields = props.Split(new char[]
			{
				','
			});
			if (fields.Length != 177)
			{
				LogManager.WriteLog(LogTypes.Error, string.Format("解析物品属性失败", new object[0]), null, true);
			}
			else
			{
				double[] arryDoubles = null;
				try
				{
					arryDoubles = Global.StringArray2DoubleArray(fields);
				}
				catch (Exception)
				{
					LogManager.WriteLog(LogTypes.Error, string.Format("转换物品属性数组", new object[0]), null, true);
					return;
				}
				equipPropItem = new EquipPropItem();
				for (int i = 0; i < 177; i++)
				{
					equipPropItem.ExtProps[i] = arryDoubles[i];
				}
			}
		}

		
		public string EquipPropsToString(double[] ExtProps)
		{
			string strProps = "";
			string result;
			if (ExtProps == null)
			{
				result = strProps;
			}
			else
			{
				for (int i = 0; i < ExtProps.Length; i++)
				{
					if (i == 0)
					{
						strProps += ExtProps[i];
					}
					else
					{
						strProps += ",";
						strProps += ExtProps[i];
					}
				}
				result = strProps;
			}
			return result;
		}

		
		public EquipPropItem FindEquipPropItem(int equipID)
		{
			EquipPropItem equipPropItem = null;
			lock (this._EquipPropsDict)
			{
				if (this._EquipPropsDict.TryGetValue(equipID, out equipPropItem))
				{
					return equipPropItem;
				}
			}
			SystemXmlItem systemGoods = null;
			EquipPropItem result;
			if (!GameManager.SystemGoods.SystemXmlItemDict.TryGetValue(equipID, out systemGoods))
			{
				result = null;
			}
			else
			{
				this.ParseEquipProps(systemGoods, out equipPropItem);
				if (null != equipPropItem)
				{
					lock (this._EquipPropsDict)
					{
						this._EquipPropsDict[equipID] = equipPropItem;
					}
				}
				result = equipPropItem;
			}
			return result;
		}

		
		public void ClearCachedEquipPropItem()
		{
			lock (this._EquipPropsDict)
			{
				this._EquipPropsDict.Clear();
			}
		}

		
		private Dictionary<int, EquipPropItem> _EquipPropsDict = new Dictionary<int, EquipPropItem>();
	}
}
