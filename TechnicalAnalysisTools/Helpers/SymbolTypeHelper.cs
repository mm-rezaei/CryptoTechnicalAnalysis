using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Helpers
{
    public static class SymbolTypeHelper
    {
        private static SymbolTypes[] _SymbolTypesList;

        public static SymbolTypes[] SymbolTypesList
        {
            get
            {
                if (_SymbolTypesList == null)
                {
                    _SymbolTypesList = (SymbolTypes[])Enum.GetValues(typeof(SymbolTypes));

                    if (File.Exists(ServerAddressHelper.SymbolsListFile))
                    {
                        var lines = File.ReadAllLines(ServerAddressHelper.SymbolsListFile).Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).ToArray();

                        var symbolTypesList = new List<SymbolTypes>();

                        foreach (var line in lines)
                        {
                            SymbolTypes symbol;

                            if (Enum.TryParse(line, out symbol))
                            {
                                symbolTypesList.Add(symbol);
                            }
                        }

                        _SymbolTypesList = symbolTypesList.ToArray();
                    }
                }

                return _SymbolTypesList;
            }
        }
    }
}
