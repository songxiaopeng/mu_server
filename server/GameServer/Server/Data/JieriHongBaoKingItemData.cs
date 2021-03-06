﻿using System;
using ProtoBuf;

namespace Server.Data
{
	
	[ProtoContract]
	public class JieriHongBaoKingItemData
	{
		
		[ProtoMember(1)]
		public int RoleID;

		
		[ProtoMember(2)]
		public string Rolename;

		
		[ProtoMember(3)]
		public int TotalRecv;

		
		[ProtoMember(4)]
		public int Rank;

		
		[ProtoMember(5)]
		public int GetAwardTimes;

		
		[ProtoMember(6)]
		public int ZoneID;
	}
}
