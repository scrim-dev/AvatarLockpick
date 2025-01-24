using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AvatarLockpick.Utils
{
    internal static class MsgBoxUtils
    {
        private const string APP_NAME = "AvatarLockpick";

        public static DialogResult ShowError(string message, string title = "Error")
        {
            return MessageBox.Show(message, $"{APP_NAME} - {title}", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static DialogResult ShowWarning(string message, string title = "Warning")
        {
            return MessageBox.Show(message, $"{APP_NAME} - {title}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static DialogResult ShowInfo(string message, string title = "Information")
        {
            return MessageBox.Show(message, $"{APP_NAME} - {title}", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /*public static DialogResult ShowQuestion(string message, string title = "Question")
        {
            return MessageBox.Show(message, $"{APP_NAME} - {title}", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }*/

        public static DialogResult ShowCustom(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(message, $"{APP_NAME} - {title}", buttons, icon);
        }
    }
}
