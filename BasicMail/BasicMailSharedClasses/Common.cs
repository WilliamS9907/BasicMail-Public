using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static BasicMailSharedPublicStructs.PublicStructs;

namespace BasicMailSharedClasses
{
    public static class Common
    {
        //Credit: Rasmus Faber via Stack Overflow:
        //https://stackoverflow.com/questions/818704/how-to-convert-securestring-to-system-string
        public static String SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);

                return Marshal.PtrToStringUni(valuePtr)!;
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        public static void SaveConfig(Configuration appConfig)
        {
            appConfig.Save(ConfigurationSaveMode.Full);

            ConfigurationManager.RefreshSection("appSettings");
        }

        public static ActionLogEntry GetActionLogEntry(String title, String stack,
                                                       String from = "", String to = "",
                                                       String cc = "", String bcc = "",
                                                       String subject = "", String body = "",
                                                       EmailStatus emailStatus = EmailStatus.Failed)
        {
            return new ActionLogEntry
            {
                title = title,
                from = from,
                to = to,
                cc = cc,
                bcc = bcc,
                subject = subject,
                body = body,
                status = emailStatus!,
                stack = stack
            };
        }

        public static async Task WriteActionLogToFileAsync(String logsPath, 
                                                           Guid appInstance,
                                                           ObservableCollection<ActionLogEntry> actionLog)
        {
            String actionLogPath = logsPath + "ActionLog_" + DateTime.Now.ToString("MM_dd_yyyy") + "_" + appInstance + ".json";

            await Task.Run(() =>
            {
                File.WriteAllText(actionLogPath, JsonConvert.SerializeObject(actionLog));
            });
        }
    }
}
