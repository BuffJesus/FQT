using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FableQuestTool.Services;

/// <summary>
/// Launches Fable with FSE (FableScriptExtender.dll) injected.
/// This enables custom Lua quest scripts to run in the game.
/// </summary>
public static class FseLauncher
{
    #region Native Windows API

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
        IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

    // Process creation for suspended start
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool CreateProcess(
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint ResumeThread(IntPtr hThread);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct STARTUPINFO
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
    private const uint MEM_COMMIT = 0x1000;
    private const uint MEM_RESERVE = 0x2000;
    private const uint MEM_RELEASE = 0x8000;
    private const uint PAGE_READWRITE = 0x04;
    private const uint INFINITE = 0xFFFFFFFF;
    private const uint CREATE_SUSPENDED = 0x00000004;

    #endregion

    /// <summary>
    /// Launches Fable with FSE injected.
    /// </summary>
    /// <param name="fablePath">Path to the Fable installation directory</param>
    /// <param name="message">Output message with result or error details</param>
    /// <returns>True if launch successful, false otherwise</returns>
    public static bool Launch(string fablePath, out string message)
    {
        message = string.Empty;

        // Validate paths
        string fableExePath = Path.Combine(fablePath, "Fable.exe");
        string fseDllPath = Path.Combine(fablePath, "FableScriptExtender.dll");

        if (!File.Exists(fableExePath))
        {
            message = $"Fable.exe not found at: {fableExePath}";
            return false;
        }

        if (!File.Exists(fseDllPath))
        {
            message = $"FableScriptExtender.dll not found at: {fseDllPath}\n\n" +
                     "Please ensure FableScriptExtender.dll is in your Fable installation directory.\n" +
                     "You can obtain it from the FSE releases or build it from source.";
            return false;
        }

        try
        {
            // Create process suspended
            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(si);

            if (!CreateProcess(
                fableExePath,
                null,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                CREATE_SUSPENDED,
                IntPtr.Zero,
                fablePath,
                ref si,
                out PROCESS_INFORMATION pi))
            {
                int error = Marshal.GetLastWin32Error();
                message = $"Failed to start Fable.exe (Error code: {error})";
                return false;
            }

            try
            {
                // Inject the DLL
                if (!InjectDll(pi.hProcess, fseDllPath, out string injectError))
                {
                    // If injection fails, terminate the process
                    Process.GetProcessById(pi.dwProcessId)?.Kill();
                    message = $"DLL injection failed: {injectError}";
                    return false;
                }

                // Resume the main thread
                ResumeThread(pi.hThread);

                message = "Fable launched with FSE successfully!";
                return true;
            }
            finally
            {
                // Clean up handles
                CloseHandle(pi.hThread);
                CloseHandle(pi.hProcess);
            }
        }
        catch (Exception ex)
        {
            message = $"Failed to launch Fable with FSE: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// Injects a DLL into a target process.
    /// </summary>
    private static bool InjectDll(IntPtr hProcess, string dllPath, out string error)
    {
        error = string.Empty;
        IntPtr allocatedMemory = IntPtr.Zero;

        try
        {
            // Get LoadLibraryA address from kernel32.dll
            IntPtr kernel32 = GetModuleHandle("kernel32.dll");
            if (kernel32 == IntPtr.Zero)
            {
                error = "Failed to get kernel32.dll handle";
                return false;
            }

            IntPtr loadLibraryAddr = GetProcAddress(kernel32, "LoadLibraryA");
            if (loadLibraryAddr == IntPtr.Zero)
            {
                error = "Failed to get LoadLibraryA address";
                return false;
            }

            // Allocate memory in target process for the DLL path
            byte[] dllPathBytes = Encoding.ASCII.GetBytes(dllPath + '\0');
            allocatedMemory = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)dllPathBytes.Length, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            if (allocatedMemory == IntPtr.Zero)
            {
                error = $"Failed to allocate memory in target process (Error: {Marshal.GetLastWin32Error()})";
                return false;
            }

            // Write DLL path to allocated memory
            if (!WriteProcessMemory(hProcess, allocatedMemory, dllPathBytes, (uint)dllPathBytes.Length, out _))
            {
                error = $"Failed to write DLL path to target process (Error: {Marshal.GetLastWin32Error()})";
                return false;
            }

            // Create remote thread to call LoadLibraryA with our DLL path
            IntPtr threadHandle = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr, allocatedMemory, 0, out _);

            if (threadHandle == IntPtr.Zero)
            {
                error = $"Failed to create remote thread (Error: {Marshal.GetLastWin32Error()})";
                return false;
            }

            // Wait for the thread to complete
            WaitForSingleObject(threadHandle, INFINITE);
            CloseHandle(threadHandle);

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
        finally
        {
            // Free the allocated memory
            if (allocatedMemory != IntPtr.Zero)
            {
                VirtualFreeEx(hProcess, allocatedMemory, 0, MEM_RELEASE);
            }
        }
    }

    /// <summary>
    /// Checks if FSE components are properly installed.
    /// </summary>
    public static bool IsFseReady(string fablePath, out string missingComponent)
    {
        missingComponent = string.Empty;

        string fseDllPath = Path.Combine(fablePath, "FableScriptExtender.dll");
        if (!File.Exists(fseDllPath))
        {
            missingComponent = "FableScriptExtender.dll";
            return false;
        }

        string fseFolder = Path.Combine(fablePath, "FSE");
        string questsLua = Path.Combine(fseFolder, "quests.lua");
        if (!File.Exists(questsLua))
        {
            missingComponent = "FSE/quests.lua";
            return false;
        }

        string masterLua = Path.Combine(fseFolder, "Master", "FSE_Master.lua");
        if (!File.Exists(masterLua))
        {
            missingComponent = "FSE/Master/FSE_Master.lua";
            return false;
        }

        return true;
    }
}
