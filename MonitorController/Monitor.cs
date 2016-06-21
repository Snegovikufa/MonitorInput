using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MonitorInputController
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct PHYSICAL_MONITOR
    {
        public IntPtr hPhysicalMonitor;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string
            szPhysicalMonitorDescription;
    }

    enum MonitorOptions : uint
    {
        MONITOR_DEFAULTTONULL = 0x00000000,
        MONITOR_DEFAULTTOPRIMARY = 0x00000001,
        MONITOR_DEFAULTTONEAREST = 0x00000002
    }

    enum MC_VCP_CODE_TYPE : uint
    {
        MC_MOMENTARY = 0x00000000,
        MC_SET_PARAMETER = 0x00000001
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }

        public POINT(Point pt) : this(pt.X, pt.Y)
        {
        }

        public static implicit operator Point(POINT p)
        {
            return new Point(p.X, p.Y);
        }

        public static implicit operator POINT(Point p)
        {
            return new POINT(p.X, p.Y);
        }
    }

    public class MonitorController : IDisposable
    {
        private readonly IntPtr _firstMonitorHandle;
        private readonly uint _maxValue;

        private readonly uint _minValue;

        private readonly uint _physicalMonitorsCount;
        private uint _currentValue;
        private PHYSICAL_MONITOR[] _physicalMonitorArray;

        public MonitorController(int x)
        {
            IntPtr ptr = MonitorFromPoint(new POINT(x, 0), MonitorOptions.MONITOR_DEFAULTTONEAREST);
            if (!GetNumberOfPhysicalMonitorsFromHMONITOR(ptr, ref _physicalMonitorsCount))
            {
                throw new Exception("Cannot get monitor count!");
            }

            _physicalMonitorArray = new PHYSICAL_MONITOR[_physicalMonitorsCount];

            if (!GetPhysicalMonitorsFromHMONITOR(ptr, _physicalMonitorsCount, _physicalMonitorArray))
            {
                throw new Exception("Cannot get phisical monitor handle!");
            }

            _firstMonitorHandle = _physicalMonitorArray[0].hPhysicalMonitor;

            if (
                !GetMonitorBrightness(
                    _firstMonitorHandle,
                    ref _minValue,
                    ref _currentValue,
                    ref _maxValue))
            {
                throw new Exception("Cannot get monitor brightness!");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [DllImport("user32.dll", EntryPoint = "MonitorFromWindow")]
        public static extern IntPtr MonitorFromWindow([In] IntPtr hwnd, uint dwFlags);

        [DllImport("dxva2.dll", EntryPoint = "DestroyPhysicalMonitors")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyPhysicalMonitors(
            uint dwPhysicalMonitorArraySize,
            ref PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", EntryPoint = "GetNumberOfPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(
            IntPtr hMonitor,
            ref uint pdwNumberOfPhysicalMonitors);

        [DllImport("dxva2.dll", EntryPoint = "GetPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPhysicalMonitorsFromHMONITOR(
            IntPtr hMonitor,
            uint dwPhysicalMonitorArraySize,
            [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", EntryPoint = "GetMonitorBrightness")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorBrightness(
            IntPtr handle,
            ref uint minimumBrightness,
            ref uint currentBrightness,
            ref uint maxBrightness);

        [DllImport("dxva2.dll", EntryPoint = "SetMonitorBrightness")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetMonitorBrightness(IntPtr handle, uint newBrightness);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);

        [DllImport("dxva2.dll", SetLastError = true)]
        static extern IntPtr GetPhysicalMonitorsFromHMONITOR(
            IntPtr monitorHandle,
            uint monitorArraySize,
            int physicalMonitor);

        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetVCPFeature(IntPtr monitorHandle, uint vcpCode, uint source);

        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetVCPFeatureAndVCPFeatureReply(
            IntPtr monitorHandle,
            uint vcpCode,
            [Out] IntPtr source,
            ref uint currentValue,
            ref uint maximumValue);

        public void SetBrightness(int newValue)
        {
            newValue = Math.Min(newValue, Math.Max(0, newValue));
            _currentValue = (_maxValue - _minValue)*(uint)newValue/100u + _minValue;
            SetMonitorBrightness(_firstMonitorHandle, _currentValue);
        }

        public void SetMonitorInputSource(uint value)
        {
            SetVCPFeature(_firstMonitorHandle, 0x60, value);
        }

        public uint GetMonitorInputSource()
        {
            uint currentValue = 0u;
            uint maximum = 0u;

            GetVCPFeatureAndVCPFeatureReply(
                _firstMonitorHandle,
                0x60,
                new IntPtr(),
                ref currentValue,
                ref maximum);

            return currentValue;
        }

        public uint GetMonitorInputSourceCount()
        {
            uint currentValue = 0u;
            uint maximum = 0u;

            GetVCPFeatureAndVCPFeatureReply(
                _firstMonitorHandle,
                0x60,
                new IntPtr(),
                ref currentValue,
                ref maximum);

            return maximum;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_physicalMonitorsCount > 0)
                {
                    DestroyPhysicalMonitors(_physicalMonitorsCount, ref _physicalMonitorArray);
                }
            }
        }
    }
}