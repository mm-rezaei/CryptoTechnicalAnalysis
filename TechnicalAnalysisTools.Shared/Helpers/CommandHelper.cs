using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.DataObjects;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.Helpers
{
    public static class CommandHelper
    {
        public static byte[] Serialize(CommandDataObject commandDataObject)
        {
            byte[] result;

            try
            {
                var commandBytes = new List<byte>();

                commandBytes.AddRange(BitConverter.GetBytes((int)commandDataObject.Command));

                commandBytes.AddRange(commandDataObject.CommandId.ToByteArray());

                if (commandDataObject.Parameter != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var formatter = new BinaryFormatter();

                        formatter.Serialize(memoryStream, commandDataObject.Parameter);

                        commandBytes.AddRange(memoryStream.ToArray());
                    }
                }

                result = commandBytes.ToArray();
            }
            catch
            {
                result = null;
            }

            return result;
        }

        public static CommandDataObject Deserialize(byte[] commandBytes)
        {
            CommandDataObject result = null;

            try
            {
                if (commandBytes.Length >= 4 + 16)
                {
                    var commandDataObject = new CommandDataObject();

                    commandDataObject.Command = (CommandTypes)Enum.ToObject(typeof(CommandTypes), BitConverter.ToInt32(commandBytes, 0));

                    commandDataObject.CommandId = new Guid(commandBytes.Skip(4).Take(16).ToArray());

                    if (commandBytes.Length > 4 + 16)
                    {
                        using (var memoryStream = new MemoryStream(commandBytes, 20, commandBytes.Length - 20))
                        {
                            var formatter = new BinaryFormatter();

                            commandDataObject.Parameter = formatter.Deserialize(memoryStream);
                        }
                    }

                    result = commandDataObject;
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }

        public static byte[] ImAlive()
        {
            return Serialize(new CommandDataObject() { Command = CommandTypes.ImAlive, Parameter = null });
        }

        public static byte[] SessionKey(byte[] sessionKey)
        {
            return Serialize(new CommandDataObject() { Command = CommandTypes.SessionKey, Parameter = sessionKey });
        }

        public static byte[] SuccessfulAuthenticate(UiClientTypes clientType)
        {
            return Serialize(new CommandDataObject() { Command = CommandTypes.SuccessfulAuthenticate, Parameter = clientType });
        }

        public static byte[] MenuItemChanged(CommandTypes menuItem, bool status)
        {
            var parameter = new object[2];

            parameter[0] = menuItem;
            parameter[1] = status;

            return Serialize(new CommandDataObject() { Command = CommandTypes.MenuItemChanged, Parameter = parameter });
        }

        public static byte[] ServerStatusPropertyChanged(string name, object value)
        {
            var parameter = new object[2];

            parameter[0] = name;
            parameter[1] = value;

            return Serialize(new CommandDataObject() { Command = CommandTypes.ServerStatusPropertyChanged, Parameter = parameter });
        }

        public static CommandDataObject RunAlarms(string script, string filename)
        {
            var parameter = new object[2];

            parameter[0] = script;
            parameter[1] = filename;

            return new CommandDataObject() { Command = CommandTypes.RunAlarms, Parameter = parameter };
        }

        public static CommandDataObject RunTemplateAlarm(string script, SymbolTypes[] symbols, string filename)
        {
            var parameter = new object[3];

            parameter[0] = script;
            parameter[1] = symbols;
            parameter[2] = filename;

            return new CommandDataObject() { Command = CommandTypes.RunTemplateAlarm, Parameter = parameter };
        }

        public static CommandDataObject EvaluateAlarm(Guid id, DateTime datetime)
        {
            var parameter = new object[2];

            parameter[0] = id;
            parameter[1] = datetime;

            return new CommandDataObject() { Command = CommandTypes.EvaluateAlarm, Parameter = parameter };
        }

        public static CommandDataObject TestNewStrategy(StrategyTestDataModel strategyTest)
        {
            var parameter = new object[4];

            parameter[0] = strategyTest;

            if(!string.IsNullOrWhiteSpace(strategyTest.Enter.Alarm))
            {
                try
                {
                    if(File.Exists(strategyTest.Enter.Alarm))
                    {
                        parameter[1] = File.ReadAllBytes(strategyTest.Enter.Alarm);
                    }
                }
                catch
                {
                    parameter[1] = null;
                }
            }

            if (!string.IsNullOrWhiteSpace(strategyTest.ExitTakeProfit.Alarm))
            {
                try
                {
                    if (File.Exists(strategyTest.ExitTakeProfit.Alarm))
                    {
                        parameter[2] = File.ReadAllBytes(strategyTest.ExitTakeProfit.Alarm);
                    }
                }
                catch
                {
                    parameter[2] = null;
                }
            }

            if (!string.IsNullOrWhiteSpace(strategyTest.ExitStopLoss.Alarm))
            {
                try
                {
                    if (File.Exists(strategyTest.ExitStopLoss.Alarm))
                    {
                        parameter[3] = File.ReadAllBytes(strategyTest.ExitStopLoss.Alarm);
                    }
                }
                catch
                {
                    parameter[3] = null;
                }
            }

            return new CommandDataObject() { Command = CommandTypes.TestNewStrategy, Parameter = parameter };
        }
    }
}
