﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.ElectronicCommerce.Core
{
	public static class SnowflakeHelper
	{
		public static long Next()
		{
			SnowflakeTool snowflakeTool = new SnowflakeTool(1);
			return snowflakeTool.NextId();
		}

		private class SnowflakeTool
		{
			//机器ID
			private static long nodeId;
			private static long twepoch = 687888001020L; //唯一时间，这是一个避免重复的随机量，自行设定不要大于当前时间戳
			private static long sequence = 0L;
			private static int workerIdBits = 4; //机器码字节数。4个字节用来保存机器码(定义为Long类型会出现，最大偏移64位，所以左移64位没有意义)
			public static long maxWorkerId = -1L ^ -1L << workerIdBits; //最大机器ID
			private static int sequenceBits = 10; //计数器字节数，10个字节用来保存计数码
			private static int workerIdShift = sequenceBits; //机器码数据左移位数，就是后面计数器占用的位数
			private static int timestampLeftShift = sequenceBits + workerIdBits; //时间戳左移动位数就是机器码和计数器总字节数
			public static long sequenceMask = -1L ^ -1L << sequenceBits; //一微秒内可以产生计数，如果达到该值则等到下一微妙在进行生成
			private long lastTimestamp = -1L;

			/// <summary>
			/// 机器码
			/// </summary>
			/// <param name="workerId"></param>
			public SnowflakeTool(long workerId)
			{
				if (workerId > maxWorkerId || workerId < 0)
					throw new Exception(string.Format("节点id 不能大于 {0} 或者 小于 0 ", workerId));
				SnowflakeTool.nodeId = workerId;

			}

			public long NextId()
			{
				lock (this)
				{
					long timestamp = TimeGen();
					if (this.lastTimestamp == timestamp)
					{ //同一微妙中生成ID
						SnowflakeTool.sequence = (SnowflakeTool.sequence + 1) & SnowflakeTool.sequenceMask; //用&运算计算该微秒内产生的计数是否已经到达上限
						if (SnowflakeTool.sequence == 0)
						{
							//一微妙内产生的ID计数已达上限，等待下一微妙
							timestamp = TillNextMillis(this.lastTimestamp);
						}
					}
					else
					{ //不同微秒生成ID
						SnowflakeTool.sequence = 0; //计数清0
					}
					if (timestamp < lastTimestamp)
					{ //如果当前时间戳比上一次生成ID时时间戳还小，抛出异常，因为不能保证现在生成的ID之前没有生成过
						throw new Exception(string.Format("Clock moved backwards.  Refusing to generate id for {0} milliseconds",
							this.lastTimestamp - timestamp));
					}
					this.lastTimestamp = timestamp; //把当前时间戳保存为最后生成ID的时间戳
					long nextId = (timestamp - twepoch << timestampLeftShift) | SnowflakeTool.nodeId << SnowflakeTool.workerIdShift | SnowflakeTool.sequence;
					return nextId;
				}
			}

			/// <summary>
			/// 获取下一微秒时间戳
			/// </summary>
			/// <param name="lastTimestamp"></param>
			/// <returns></returns>
			private long TillNextMillis(long lastTimestamp)
			{
				long timestamp = TimeGen();
				while (timestamp <= lastTimestamp)
				{
					timestamp = TimeGen();
				}
				return timestamp;
			}

			/// <summary>
			/// 生成当前时间戳
			/// </summary>
			/// <returns></returns>
			private long TimeGen()
			{
				return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
			}
		}
	}

	
}
