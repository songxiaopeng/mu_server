﻿using System;
using System.Collections.Generic;
using GameDBServer.Logic;

namespace GameDBServer.Server.CmdProcessor
{
	
	public class CmdRegisterTriggerManager : IManager
	{
		
		private CmdRegisterTriggerManager()
		{
		}

		
		public static CmdRegisterTriggerManager getInstance()
		{
			return CmdRegisterTriggerManager.instance;
		}

		
		private void TriggerProcessor(ICmdProcessor icp)
		{
		}

		
		public bool initialize()
		{
			this.TriggerProcessor(ZhanMengBuildUpLevelCmdProcessor.getInstance());
			this.TriggerProcessor(ZhanMengBuildGetBufferCmdProcessor.getInstance());
			this.TriggerProcessor(BaiTanLogAddCmdProcessor.getInstance());
			this.TriggerProcessor(BaiTanLogDetailCmdProcessor.getInstance());
			return true;
		}

		
		public bool startup()
		{
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

		
		private static CmdRegisterTriggerManager instance = new CmdRegisterTriggerManager();

		
		private List<ICmdProcessor> CmdProcessorList = new List<ICmdProcessor>();
	}
}
