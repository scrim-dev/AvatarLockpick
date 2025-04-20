using System.Runtime.InteropServices; // Added for DllImport

namespace AvatarLockpick.Revised.Utils
{
    public static class MessageBoxUtils
    {
        // Import the user32.dll MessageBox function
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

        // Define MessageBox constants (subset)
        // For uType parameter
        // Buttons
        private const uint MB_OK = 0x00000000U;
        private const uint MB_OKCANCEL = 0x00000001U;
        private const uint MB_YESNOCANCEL = 0x00000003U;
        private const uint MB_YESNO = 0x00000004U;

        // Icons
        private const uint MB_ICONERROR = 0x00000010U;
        private const uint MB_ICONQUESTION = 0x00000020U;
        private const uint MB_ICONWARNING = 0x00000030U;
        private const uint MB_ICONINFORMATION = 0x00000040U;

        // Optional: Default Button (e.g., MB_DEFBUTTON1, MB_DEFBUTTON2)

        // Return values (though we aren't capturing them in the helpers)
        // private const int IDOK = 1;
        // private const int IDCANCEL = 2;
        // private const int IDYES = 6;
        // private const int IDNO = 7;

        /// <summary>
        /// Displays a native Windows message box.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="caption">The title bar text.</param>
        /// <param name="buttonsAndIcon">Combined WinAPI flags for buttons and icon (e.g., MB_OK | MB_ICONINFORMATION).</param>
        /// <returns>Result code from the WinAPI call (e.g., IDOK, IDCANCEL).</returns>
        public static int Show(string text, string caption, uint buttonsAndIcon)
        {
            // Pass IntPtr.Zero for the owner window handle (no owner)
            return MessageBox(IntPtr.Zero, text, caption, buttonsAndIcon);
        }

        /// <summary>
        /// Displays a warning message box.
        /// </summary>
        /// <param name="text">The warning message text.</param>
        /// <param name="caption">The title bar text.</param>
        public static void ShowWarning(string text, string caption = "Warning")
        {
            Show(text, caption, MB_OK | MB_ICONWARNING);
        }

        /// <summary>
        /// Displays an error message box.
        /// </summary>
        /// <param name="text">The error message text.</param>
        /// <param name="caption">The title bar text.</param>
        public static void ShowError(string text, string caption = "Error")
        {
            Show(text, caption, MB_OK | MB_ICONERROR);
        }

        /// <summary>
        /// Displays an information message box.
        /// </summary>
        /// <param name="text">The information message text.</param>
        /// <param name="caption">The title bar text.</param>
        public static void ShowInfo(string text, string caption = "Information")
        {
            Show(text, caption, MB_OK | MB_ICONINFORMATION);
        }
    }
} 