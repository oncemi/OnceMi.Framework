using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OnceMi.Framework.Model.Dto
{
    public class SystemInfo
    {
        /// <summary>
        /// 操作系统的版本描述
        /// <para>
        /// ex: <br />
        /// Microsoft Windows 10.0.19041
        /// <br />
        /// Linux 4.4.0-19041-Microsoft #488-Microsoft Mon Sep 01 13:43:00 PST 2020
        /// </para>
        /// </summary>
        public string OSDescription { get; set; } = RuntimeInformation.OSDescription;

        /// <summary>
        /// 操作系统架构，可点击 <see cref="Architecture" /> 获取详细的信息
        /// <para>
        /// ex:<br />
        /// X86<br />
        /// X64<br />
        /// Arm<br />
        /// Arm64
        /// </para>
        /// </summary>
        public string OSArchitecture { get; set; } = RuntimeInformation.OSArchitecture.ToString();

        /// <summary>
        /// 开机时间 分钟
        /// </summary>
        public double OSTickMins
        {
            get
            {
                double val = Environment.TickCount / 1000.0 / 60.0;
                if (val < 0) val = -val;
                return val;
            }
        }

        /// <summary>
        /// 进程的架构，可点击 <see cref="Architecture" /> 获取详细的信息
        /// <para>
        /// ex:<br />
        /// X86<br />
        /// X64<br />
        /// Arm<br />
        /// Arm64
        /// </para>
        /// </summary>
        public string ProcessArchitecture { get; set; } = RuntimeInformation.ProcessArchitecture.ToString();

        public string ProcessPath { get; set; } = Environment.CurrentDirectory;

        public double TotalPhysicalMemory { get; set; }

        public double AvailablePhysicalMemory { get; set; }

        public double ProcessUsedMemory { get; set; } = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;

        public string FrameworkDescription => RuntimeInformation.FrameworkDescription;

        public List<SystemCpuHardwareInfo> CpuInfos { get; set; } = new List<SystemCpuHardwareInfo>();
    }

    public class SystemCpuHardwareInfo
    {
        public int Num { get; set; }

        public string Name { get; set; }

        public double MaxClockSpeed { get; set; }

        public uint NumberOfCores { get; set; }
    }
}
