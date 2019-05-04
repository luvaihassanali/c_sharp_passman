# c_sharp_passman
Windows Form App

1. Download/clone project
2. Navigate to project directory, right-click Form1.resx, click Properties, and check Unblock under Security 
3. Open project in Visual Studio and make sure Newtonsoft.Json reference is available, if not install from package manager
4. Build project then copy key.icon and data.json into bin/Debug or Release
5. Run

New entries can be created with New, deleted with Delete, and the Reset button will delete all entries (user will be prompted to validate). After an entry has been made, click the corresponding button to copy the password to your clipboard. You have 5 seconds to paste your password into the designated input before the clipboard is cleared

To reset, set data.json to:
```
{
  "entries": []
}
```

