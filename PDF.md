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
