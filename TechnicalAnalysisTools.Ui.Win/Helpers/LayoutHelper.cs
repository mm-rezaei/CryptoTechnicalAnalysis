using System.IO;
using System.Windows;
using DevExpress.Xpf.Grid;

namespace TechnicalAnalysisTools.Ui.Win.Helpers
{
    internal static class LayoutHelper
    {
        public static void SaveLayout(Window window, GridControl grid)
        {
            try
            {
                var filename = Path.Combine(ClientAddressHelper.LayoutDataFolder, window.Name + grid.Name + ".xml");

                if (File.Exists(filename))
                {
                    try
                    {
                        File.Delete(filename);
                    }
                    catch
                    {

                    }
                }

                grid.SaveLayoutToXml(filename);
            }
            catch
            {

            }
        }

        public static void LoadLayout(Window window, GridControl grid)
        {
            try
            {
                var filename = Path.Combine(ClientAddressHelper.LayoutDataFolder, window.Name + grid.Name + ".xml");

                if (File.Exists(filename))
                {
                    grid.RestoreLayoutFromXml(filename);
                }

                grid.ClearSorting();
            }
            catch
            {

            }
        }
    }
}
