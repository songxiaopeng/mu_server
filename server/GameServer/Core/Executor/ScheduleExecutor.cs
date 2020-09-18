﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace GameServer.Core.Executor
{
	
	public class ScheduleExecutor
	{
		
		public ScheduleExecutor(int maxThreadNum)
		{
			this.maxThreadNum = maxThreadNum;
			this.threadQueue = new List<Thread>();
			this.workerQueue = new List<Worker>();
			this.TaskQueue = new LinkedList<TaskWrapper>();
			for (int i = 0; i < maxThreadNum; i++)
			{
				Worker worker = new Worker(this);
				Thread thread = new Thread(new ThreadStart(worker.work));
				worker.CurrentThread = thread;
				this.workerQueue.Add(worker);
				this.threadQueue.Add(thread);
			}
		}

		
		public void start()
		{
			lock (this.threadQueue)
			{
				foreach (Thread thread in this.threadQueue)
				{
					thread.Start();
				}
			}
		}

		
		public void stop()
		{
			lock (this.threadQueue)
			{
				foreach (Thread thread in this.threadQueue)
				{
					thread.Abort();
				}
				this.threadQueue.Clear();
			}
			lock (this.workerQueue)
			{
				this.workerQueue.Clear();
			}
		}

		
		public bool execute(ScheduleTask task)
		{
			TaskWrapper wrapper = new TaskWrapper(task, -1L, -1L);
			this.addTask(wrapper);
			return true;
		}

		
		public PeriodicTaskHandle scheduleExecute(ScheduleTask task, long delay)
		{
			return this.scheduleExecute(task, delay, -1L);
		}

		
		public PeriodicTaskHandle scheduleExecute(ScheduleTask task, long delay, long periodic)
		{
			TaskWrapper wrapper = new TaskWrapper(task, delay, periodic);
			PeriodicTaskHandle handle = new PeriodicTaskHandleImpl(wrapper, this);
			this.addTask(wrapper);
			return handle;
		}

		
		internal TaskWrapper GetPreiodictTask(long ticks)
		{
			TaskWrapper result;
			lock (this.PreiodictTaskList)
			{
				if (this.PreiodictTaskList.Count == 0)
				{
					result = null;
				}
				else if (this.PreiodictTaskList[0].StartTime > ticks)
				{
					result = null;
				}
				else
				{
					TaskWrapper taskWrapper = this.PreiodictTaskList[0];
					this.PreiodictTaskList.RemoveAt(0);
					result = taskWrapper;
				}
			}
			return result;
		}

		
		internal TaskWrapper getTask()
		{
			lock (this.TaskQueue)
			{
				try
				{
					if (this.TaskQueue.Count <= 0)
					{
						return null;
					}
					TaskWrapper currentTask = this.TaskQueue.First.Value;
					this.TaskQueue.RemoveFirst();
					if (currentTask.canExecute)
					{
						return currentTask;
					}
					return null;
				}
				catch (Exception)
				{
				}
			}
			return null;
		}

		
		internal int GetTaskCount()
		{
			int count;
			lock (this.TaskQueue)
			{
				count = this.TaskQueue.Count;
			}
			return count;
		}

		
		internal void addTask(TaskWrapper taskWrapper)
		{
			if (taskWrapper.Periodic > 0L)
			{
				lock (this.PreiodictTaskList)
				{
					this.PreiodictTaskList.BinaryInsertAsc(taskWrapper, taskWrapper);
				}
			}
			else
			{
				lock (this.TaskQueue)
				{
					this.TaskQueue.AddLast(taskWrapper);
					taskWrapper.canExecute = true;
				}
			}
		}

		
		internal void removeTask(TaskWrapper taskWrapper)
		{
			lock (this.TaskQueue)
			{
				this.TaskQueue.Remove(taskWrapper);
				taskWrapper.canExecute = false;
			}
		}

		
		private List<Worker> workerQueue = null;

		
		private List<Thread> threadQueue = null;

		
		private LinkedList<TaskWrapper> TaskQueue = null;

		
		private List<TaskWrapper> PreiodictTaskList = new List<TaskWrapper>();

		
		private int maxThreadNum = 0;
	}
}
