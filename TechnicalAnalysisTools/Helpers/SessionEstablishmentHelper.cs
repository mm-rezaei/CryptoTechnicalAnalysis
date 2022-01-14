using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TechnicalAnalysisTools.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Helpers
{
    public static class SessionEstablishmentHelper
    {
        public static void FillSessionEstablishment(SessionEstablishmentDataModel sessionEstablishment)
        {
            sessionEstablishment.Address = "127.0.0.1";
            sessionEstablishment.Port = 9191;
            sessionEstablishment.DatabaseSupport = false;
            sessionEstablishment.Clients.Clear();

            try
            {
                if (File.Exists(ServerAddressHelper.SessionEstablishmentFile))
                {
                    var lines = File.ReadAllLines(ServerAddressHelper.SessionEstablishmentFile).Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();

                    if (lines.Length >= 7)
                    {
                        var addressKey = "address=";
                        var portKey = "port=";
                        var databaseSupportKey = "databasesupport=";
                        var clientTypeKey = "clienttype=";
                        var usernameKey = "username=";
                        var passwordKey = "password=";
                        var isenabled = "isenabled=";

                        if (lines[0].StartsWith(addressKey))
                        {
                            sessionEstablishment.Address = lines[0].Substring(addressKey.Length).Trim();
                        }

                        if (lines[1].StartsWith(portKey))
                        {
                            sessionEstablishment.Port = int.Parse(lines[1].Substring(portKey.Length).Trim());
                        }

                        if (lines[2].StartsWith(databaseSupportKey))
                        {
                            var value = lines[2].Substring(databaseSupportKey.Length).Trim().ToLower();

                            value = char.ToUpper(value[0]).ToString() + value.Substring(1);

                            sessionEstablishment.DatabaseSupport = Convert.ToBoolean(value);
                        }

                        var lineIndex = 3;

                        while (lineIndex < lines.Length)
                        {
                            //
                            var clientItem = new SessionEstablishmentItemDataModel();

                            if (lines[lineIndex].StartsWith(clientTypeKey))
                            {
                                var value = lines[lineIndex].Substring(clientTypeKey.Length).Trim().ToLower();

                                value = char.ToUpper(value[0]).ToString() + value.Substring(1);

                                clientItem.ClientType = (UiClientTypes)Enum.Parse(typeof(UiClientTypes), value);
                            }
                            else
                            {
                                clientItem = null;
                            }

                            if (lines[lineIndex + 1].StartsWith(usernameKey))
                            {
                                clientItem.Username = lines[lineIndex + 1].Substring(usernameKey.Length).Trim().ToLower();
                            }
                            else
                            {
                                clientItem = null;
                            }

                            if (lines[lineIndex + 2].StartsWith(passwordKey))
                            {
                                clientItem.Password = lines[lineIndex + 2].Substring(passwordKey.Length).Trim();
                            }
                            else
                            {
                                clientItem = null;
                            }

                            if (lines[lineIndex + 3].StartsWith(isenabled))
                            {
                                var value = lines[lineIndex + 3].Substring(isenabled.Length).Trim().ToLower();

                                value = char.ToUpper(value[0]).ToString() + value.Substring(1);

                                clientItem.IsEnabled = Convert.ToBoolean(value);
                            }
                            else
                            {
                                clientItem = null;
                            }

                            if (clientItem == null)
                            {
                                throw new Exception("SessionEstablishment file is corrupted.");
                            }
                            else
                            {
                                sessionEstablishment.Clients.Add(clientItem);
                            }

                            //
                            lineIndex += 4;
                        }
                    }
                }
            }
            catch
            {
                sessionEstablishment.Address = "127.0.0.1";
                sessionEstablishment.Port = 9191;
                sessionEstablishment.DatabaseSupport = false;
                sessionEstablishment.Clients.Clear();
            }
        }

        public static void FillSessionEstablishmentClients(SessionEstablishmentDataModel sessionEstablishment)
        {
            //
            var address = sessionEstablishment.Address;
            var port = sessionEstablishment.Port;
            var databaseSupport = sessionEstablishment.DatabaseSupport;

            var enabledClients = new Dictionary<string, bool>();

            foreach (var client in sessionEstablishment.Clients)
            {
                enabledClients.Add(client.Username, client.IsEnabled);
            }

            //
            FillSessionEstablishment(sessionEstablishment);

            //
            sessionEstablishment.Address = address;
            sessionEstablishment.Port = port;
            sessionEstablishment.DatabaseSupport = databaseSupport;

            foreach (var client in sessionEstablishment.Clients)
            {
                if (enabledClients.ContainsKey(client.Username))
                {
                    client.IsEnabled = enabledClients[client.Username];
                }
            }
        }
    }
}
