using System;
using System.Text.RegularExpressions;
using SuperSocket.SocketBase.Logging;
using TechnicalAnalysisTools.DataObjects;
using TechnicalAnalysisTools.Delegates;

namespace TechnicalAnalysisTools.Auxiliaries
{
    public class ServerLogAuxiliary : ILog
    {
        public ServerLogAuxiliary(string name)
        {
            InputName = name;
        }

        private string InputName;

        private string GetStandardString(object value)
        {
            string result;

            if (value == null)
            {
                result = "";
            }
            else
            {
                var message = value.ToString();

                if (string.IsNullOrWhiteSpace(message))
                {
                    result = "";
                }
                else
                {
                    result = message;
                }
            }

            return result;
        }

        private void LogMessage(object value, string action)
        {
            //
            var message = GetStandardString(value);

            //
            var guids = Regex.Matches(message, @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}"); //Match all substrings in findGuid

            for (int index = 0; index < guids.Count; index++)
            {
                message = message.Replace(guids[index].Value, "");
            }

            message = message.Replace(": /", ": ");

            //
            if (!string.IsNullOrWhiteSpace(message))
            {
                var log = new LogDataObject()
                {
                    LogTime = DateTime.UtcNow,
                    Action = action,
                    Message = message.Replace(Environment.NewLine, ", ")
                };

                LogReceived?.Invoke(log);
            }
        }

        public bool IsDebugEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsErrorEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsFatalEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsInfoEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsWarnEnabled
        {
            get
            {
                return true;
            }
        }

        public void Debug(object message)
        {
            var mainMessage = message;

            LogMessage(mainMessage, "DEBUG");
        }

        public void Debug(object message, Exception exception)
        {
            var mainMessage = (object)(message.ToString() + Environment.NewLine + exception.Message + exception.StackTrace);

            LogMessage(mainMessage, "DEBUG");
        }

        public void DebugFormat(string format, object arg0)
        {
            var mainMessage = (object)string.Format(format, arg0);

            LogMessage(mainMessage, "DEBUG");
        }

        public void DebugFormat(string format, params object[] args)
        {
            var mainMessage = (object)string.Format(format, args);

            LogMessage(mainMessage, "DEBUG");
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            var mainMessage = (object)string.Format(provider, format, args);

            LogMessage(mainMessage, "DEBUG");
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            var mainMessage = (object)string.Format(format, arg0, arg1);

            LogMessage(mainMessage, "DEBUG");
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            var mainMessage = (object)string.Format(format, arg0, arg1, arg2);

            LogMessage(mainMessage, "DEBUG");
        }

        public void Error(object message)
        {
            var mainMessage = message;

            LogMessage(mainMessage, "ERROR");
        }

        public void Error(object message, Exception exception)
        {
            var mainMessage = (object)(message.ToString() + Environment.NewLine + exception.Message + exception.StackTrace);

            LogMessage(mainMessage, "ERROR");
        }

        public void ErrorFormat(string format, object arg0)
        {
            var mainMessage = (object)string.Format(format, arg0);

            LogMessage(mainMessage, "ERROR");
        }

        public void ErrorFormat(string format, params object[] args)
        {
            var mainMessage = (object)string.Format(format, args);

            LogMessage(mainMessage, "ERROR");
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            var mainMessage = (object)string.Format(provider, format, args);

            LogMessage(mainMessage, "ERROR");
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            var mainMessage = (object)string.Format(format, arg0, arg1);

            LogMessage(mainMessage, "ERROR");
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            var mainMessage = (object)string.Format(format, arg0, arg2);

            LogMessage(mainMessage, "ERROR");
        }

        public void Fatal(object message)
        {
            var mainMessage = message;

            LogMessage(mainMessage, "FATAL");
        }

        public void Fatal(object message, Exception exception)
        {
            var mainMessage = (object)(message.ToString() + Environment.NewLine + exception.Message + exception.StackTrace);

            LogMessage(mainMessage, "FATAL");
        }

        public void FatalFormat(string format, object arg0)
        {
            var mainMessage = (object)string.Format(format, arg0);

            LogMessage(mainMessage, "FATAL");
        }

        public void FatalFormat(string format, params object[] args)
        {
            var mainMessage = (object)string.Format(format, args);

            LogMessage(mainMessage, "FATAL");
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            var mainMessage = (object)string.Format(provider, format, args);

            LogMessage(mainMessage, "FATAL");
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            var mainMessage = (object)string.Format(format, arg0, arg1);

            LogMessage(mainMessage, "FATAL");
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            var mainMessage = (object)string.Format(format, arg0, arg1, arg2);

            LogMessage(mainMessage, "FATAL");
        }

        public void Info(object message)
        {
            var mainMessage = message;

            LogMessage(mainMessage, "INFO");
        }

        public void Info(object message, Exception exception)
        {
            var mainMessage = (object)(message.ToString() + Environment.NewLine + exception.Message + exception.StackTrace);

            LogMessage(mainMessage, "INFO");
        }

        public void InfoFormat(string format, object arg0)
        {
            var mainMessage = (object)string.Format(format, arg0);

            LogMessage(mainMessage, "INFO");
        }

        public void InfoFormat(string format, params object[] args)
        {
            var mainMessage = (object)string.Format(format, args);

            LogMessage(mainMessage, "INFO");
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            var mainMessage = (object)string.Format(provider, format, args);

            LogMessage(mainMessage, "INFO");
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            var mainMessage = (object)string.Format(format, arg0, arg1);

            LogMessage(mainMessage, "INFO");
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            var mainMessage = (object)string.Format(format, arg0, arg1, arg2);

            LogMessage(mainMessage, "INFO");
        }

        public void Warn(object message)
        {
            var mainMessage = message;

            LogMessage(mainMessage, "WARN");
        }

        public void Warn(object message, Exception exception)
        {
            var mainMessage = (object)(message.ToString() + Environment.NewLine + exception.Message + exception.StackTrace);

            LogMessage(mainMessage, "WARN");
        }

        public void WarnFormat(string format, object arg0)
        {
            var mainMessage = (object)string.Format(format, arg0);

            LogMessage(mainMessage, "WARN");
        }

        public void WarnFormat(string format, params object[] args)
        {
            var mainMessage = (object)string.Format(format, args);

            LogMessage(mainMessage, "WARN");
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            var mainMessage = (object)string.Format(provider, format, args);

            LogMessage(mainMessage, "WARN");
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            var mainMessage = (object)string.Format(format, arg0, arg1);

            LogMessage(mainMessage, "WARN");
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            var mainMessage = (object)string.Format(format, arg0, arg1, arg2);

            LogMessage(mainMessage, "WARN");
        }

        public event LogReceivedHandler LogReceived;
    }
}
