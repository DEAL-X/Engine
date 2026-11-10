using System;
using System.Collections.Generic;
using System.Text;

namespace MiMFa.Engine.Core
{
    public enum LogStatus
    {
        Error = -2,
        Warning = -1,
        Info = 0,
        Message = 1,
        Success = 2,
        Subject = 9
    }

    public class LogEventArgs : EventArgs
    {
        public LogStatus? Status { get; }
        public DateTime Time { get; }
        public string Log { get; }

        public LogEventArgs(string log = "", LogStatus? status = LogStatus.Info, DateTime? time = null)
        {
            Status = status;
            Time = time??DateTime.Now;
            Log = log;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            if (Status.HasValue)
                switch (Status.Value)
                {
                    case LogStatus.Error:
                    case LogStatus.Warning:
                    case LogStatus.Success:
                        return string.Join("\t", Time.ToString("yyyy/MM/dd HH:mm:ss"), (Status + ": ").ToUpper() + Log);
                    case LogStatus.Subject:
                        return string.Join("\t", Time.ToString("yyyy/MM/dd HH:mm:ss"), (Log+"").ToUpper());
                    default:
                        return string.Join("\t", Time.ToString("yyyy/MM/dd HH:mm:ss"), Log);
                }
            else return string.Join("\t", Time.ToString("yyyy/MM/dd HH:mm:ss"), Log);
        }
    }

    public delegate void LogEventHandler(Engine compiler, LogEventArgs e);
}
