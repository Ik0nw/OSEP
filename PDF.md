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


