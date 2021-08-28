using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OnceMi.Framework.Model.Dto
{
    public class SystemHardwareInfo
    {
        public string OSName { get; set; } = Environment.OSVersion.VersionString;

        public string OSBit { get; set; } = Environment.Is64BitOperatingSystem ? "x64" : "x86";

        public string ProcessBit { get; set; } = Environment.Is64BitProcess ? "x64" : "x86";

        public string ProcessPath { get; set; } = Environment.CurrentDirectory;

        public double TotalPhysicalMemory { get; set; }

        public double AvailablePhysicalMemory { get; set; }

        public double ProcessUsedMemory { get; set; } = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;

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
