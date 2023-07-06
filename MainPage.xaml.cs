using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace UWPAPPSampleCallingCameraDLL
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        [DllImport("Mfplat.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern int MFStartup(uint Version, int dwFlags);

        [DllImport("mfsensorgroup.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern HRESULT MFCreateSensorActivityMonitor(
            [In] IMFSensorActivitiesReportCallback pCallback,
            [MarshalAs(UnmanagedType.Interface), Out] out IMFSensorActivityMonitor ppActivityMonitor
        );

        public MainPage()
        {
            this.InitializeComponent();
        }

        // Define the IMFSensorActivityMonitor interface
        [ComImport]
        [Guid("D0CEF145-B3F4-4340-A2E5-7A5080CA05CB")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        private interface IMFSensorActivityMonitor
        {
            // Define the methods of IMFSensorActivityMonitor here
            [MethodImpl(MethodImplOptions.InternalCall)]
            HRESULT Start();
            [MethodImpl(MethodImplOptions.InternalCall)]
            HRESULT Stop();
        }

        // Implement your own IMFSensorActivitiesReportCallback class
        // that provides the necessary callback functionality
        private class MySensorActivitiesReportCallback : IMFSensorActivitiesReportCallback
        {
            public HRESULT OnActivitiesReport(IMFSensorActivitiesReport sensorActivitiesReport)
            {
                uint totalReportCount = 0;

                // Early exit if we did not have any reports
                sensorActivitiesReport.GetCount(out totalReportCount);
                if (totalReportCount == 0)
                {
                    Console.WriteLine("\nNo reports\n");
                    return HRESULT.S_OK;
                }

                Console.WriteLine("\n\nPrinting all reports[totalcount=%lu] \n", totalReportCount);

                for (uint idx = 0; idx < totalReportCount; idx++)
                {
                    uint count = 0;
                    IMFSensorActivityReport activityReport;
                    sensorActivitiesReport.GetActivityReport(idx, out activityReport);
                    activityReport.GetProcessCount(out count);

                    Console.WriteLine("\n\t [processcount=%lu] \n", count);
                    for (uint i = 0; i < count; i++)
                    {
                        IMFSensorProcessActivity processActivity;
                        activityReport.GetProcessActivity(i, out processActivity);
                        bool isInUse;
                        processActivity.GetStreamingState(out isInUse);
                    }
                }
                return HRESULT.S_OK;
            }
        }

        // Define the IMFSensorActivitiesReportCallback interface
        [ComImport]
        [Guid("DE5072EE-DBE3-46DC-8A87-B6F631194751")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        private interface IMFSensorActivitiesReportCallback
        {
            HRESULT OnActivitiesReport(IMFSensorActivitiesReport sensorActivitiesReport);
        }

        // Define the IMFSensorActivitiesReport interface
        [ComImport]
        [Guid("683F7A5E-4A19-43CD-B1A9-DBF4AB3F7777")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        private interface IMFSensorActivitiesReport
        {
            HRESULT GetCount(out uint pcCount);

            HRESULT GetActivityReport(uint Index, out IMFSensorActivityReport sensorActivityReport);

            HRESULT GetActivityReportByDeviceName(string SymbolicName, out IMFSensorActivityReport sensorActivityReport);
        }

        // Define the IMFSensorActivityReport interface
        [ComImport]
        [Guid("3E8C4BE1-A8C2-4528-90DE-2851BDE5FEAD")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        private interface IMFSensorActivityReport
        {
            HRESULT GetFriendlyName([MarshalAs(UnmanagedType.LPWStr)] string FriendlyName, uint cchFriendlyName, out uint pcchWritten);

            HRESULT GetSymbolicLink([MarshalAs(UnmanagedType.LPWStr)] string SymbolicLink, uint cchSymbolicLink, out uint pcchWritten);

            HRESULT GetProcessCount(out uint pcCount);

            HRESULT GetProcessActivity(uint Index, out IMFSensorProcessActivity ppProcessActivity);
        }

        // Define the IMFSensorProcessActivity interface
        [ComImport]
        [Guid("39DC7F4A-B141-4719-813C-A7F46162A2B8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        private interface IMFSensorProcessActivity
        {
            HRESULT GetProcessId(out uint pPID);

            HRESULT GetStreamingState(out bool pfStreaming);

            HRESULT GetStreamingMode(out MFSensorDeviceMode pMode);

            HRESULT GetReportTime(out FILETIME pft);
        }

        // Define the MFSensorDeviceMode enum
        private enum MFSensorDeviceMode
        {
            MFSensorDeviceMode_Controller = 0,
            MFSensorDeviceMode_Shared = (MFSensorDeviceMode_Controller + 1)
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        // Define the HRESULT struct for COM interop
        [StructLayout(LayoutKind.Sequential)]
        private struct HRESULT
        {
            public int Value;

            public static readonly HRESULT S_OK = new HRESULT { Value = 0 };
        }

        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                // Call MFStartup to initialize Media Foundation
                int result = MFStartup(0x20070, 0); // Pass the appropriate version and flags

                if (result == 0)
                {
                    Console.WriteLine("Media Foundation initialized successfully!");

                    // Perform operations with Media Foundation here

                    // Call MFShutdown to clean up Media Foundation
                    // MFShutdown();
                }
                else
                {
                    Console.WriteLine("Failed to initialize Media Foundation!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }


            IMFSensorActivitiesReportCallback callback = null;
            IMFSensorActivityMonitor activityMonitor = null;

            try
            {
                // Create an instance of your IMFSensorActivitiesReportCallback implementation
                callback = new MySensorActivitiesReportCallback();
                HRESULT result = MFCreateSensorActivityMonitor(callback, out activityMonitor);

                if (result.Equals(HRESULT.S_OK) && activityMonitor != null)
                {
                    Console.WriteLine("Sensor activity monitor created successfully!");

                    await Task.Run(() =>
                    {
                        activityMonitor.Start();
                    });

                    // activityMonitor.Start();


                    await Task.Delay(1000000);
                }
                else
                {
                    Console.WriteLine("Failed to create sensor activity monitor!");
                }
            }
            catch (COMException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Release the callback object if it was created
                if (callback != null)
                {
                    Marshal.ReleaseComObject(callback);
                }
                if (activityMonitor != null)
                {
                    Marshal.ReleaseComObject(activityMonitor);
                }
            }
        }
    }
}
