﻿using System;
using ProtoBuf;

namespace GameDBServer.Logic.GoldAuction
{
	
	[ProtoContract]
	public class AuctionRoleData
	{
		
		public AuctionRoleData()
		{
			this.m_RoleID = 0;
			this.Value = 0L;
			this.strUserID = "";
			this.m_RoleName = "";
			this.ZoneID = -1;
			this.ServerId = -1;
		}

		
		public string getAttackerValue()
		{
			try
			{
				return string.Format("{0},{1},{2},{3},{4},{5}", new object[]
				{
					this.m_RoleID,
					this.Value,
					this.m_RoleName,
					this.ZoneID,
					this.strUserID,
					this.ServerId
				});
			}
			catch
			{
			}
			return "";
		}

		
		[ProtoMember(1)]
		public int m_RoleID;

		
		[ProtoMember(2)]
		public long Value;

		
		[ProtoMember(3)]
		public string m_RoleName;

		
		[ProtoMember(4)]
		public int ZoneID;

		
		[ProtoMember(5)]
		public string strUserID;

		
		[ProtoMember(6)]
		public int ServerId;
	}
}
