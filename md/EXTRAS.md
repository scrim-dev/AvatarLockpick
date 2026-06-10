# Extra Information

## ⚠️ Important Note
> **This tool is NOT associated with VRChat Inc. in any way.**
> 
> AvatarLockpick is completely safe to use as it:
> - Only scans your local VRChat cache files
> - Never modifies the game or its processes
> - Runs entirely as an external application
> - Does not interact with VRChat's runtime
> 
> Using this tool will NOT result in a ban as it operates within VRChat's Terms of Service by only reading locally cached files.
>
> Also beware that some avatars have locks that are scrambled or hidden so some avatars may not unlock fully. (I will try my best to figure out ways to still unlock these avatars)

## ⚠️ False Positives
You may have encountered a warning from Windows Defender or other anti-virus (AV) software when downloading or running my application. I understand this can be concerning, but please be assured: **my open-source applications are safe to use when downloaded directly from my official GitHub repositories.**

## Why is this happening?

There are a few key reasons why your anti-virus software might flag this file:

1.  **Unsigned Application:**
    *   Most commercial software is "digitally signed," which verifies the publisher and ensures the software hasn't been tampered with.
    *   Obtaining a code signing certificate is costly (often $100-$500+ per year), which isn't feasible for this project at this time.
    *   Because my application is unsigned, AV software often treats it with extra caution by default.

2.  **VRChat Cache Access:**
    *   A core function of this tool is to scan your local VRChat cache files, specifically for avatar JSON data.
    *   Some AV heuristics (behavioral analysis) can view access to application-specific user data as potentially suspicious, even though this tool **only** accesses VRChat data for its intended purpose and does not collect or transmit unrelated personal information.

3.  **Compressed/Packed .NET Assemblies:**
    *   The executable (.exe) is compressed (often referred to as "packed"). This helps reduce the overall file size and can simplify distribution by bundling necessary components into a single file, removing the need for an installer to manage multiple DLLs.
    *   Unfortunately, malware authors (LOSERS) often use similar packing techniques to hide their malicious code. As a result, AV software is often highly suspicious of packed executables by default, leading to a lot of false positives.

## Is it Safe?

**Yes, the file is safe *if* you've downloaded it directly from the official GitHub Releases page for this project.**

*   As this project is open-source, I encourage you to inspect the source code yourself to verify its functionality and safety.
*   Always ensure you're downloading from the official source:
    *   **Direct Link to this Repository's Releases:** [https://github.com/scrim-dev/AvatarLockpick/releases](https://github.com/scrim-dev/AvatarLockpick/releases) (Always go to the "Releases" tab for stable downloads)

## What Can You Do?

If you trust this application (and have verified its source), you can instruct your AV software to allow it:

1.  **Verify the Source:** CRITICAL STEP! Double-check that you downloaded the file *only* from the official GitHub Releases page linked above. **Do not download from unofficial sites, forums, or direct messages.**

2.  **Add an Exception/Exclusion:**
    *   **For Windows Defender:**
        1.  Open **Windows Security** (search for it in the Start Menu).
        2.  Go to **Virus & threat protection**.
        3.  Under "Virus & threat protection settings," click **Manage settings**.
        4.  Scroll down to **Exclusions** and click **Add or remove exclusions**.
        5.  Click **+ Add an exclusion** and choose **File** (to exclude the specific `.exe`) or **Folder** (if you want to exclude the folder where you keep the tool).
        6.  Navigate to and select the application's `.exe` file or folder.
    *   **For other Anti-Virus Software:** The process is similar. Look for settings related to "Exclusions," "Exceptions," "Allowed List," or "Trusted Files/Folders." Consult your AV software's documentation if needed.

3.  **(Optional) Report as False Positive:**
    *   Most AV vendors allow you to submit files for analysis and report them as false positives. This can help them improve their detection algorithms and potentially whitelist the application in future AV definition updates.
