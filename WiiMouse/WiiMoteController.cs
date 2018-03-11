using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiimoteLib;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using System.Runtime.InteropServices;

namespace WiiMouse
{

    /// <summary>
    /// Main Class for connecting and disconnecting to the WiiMote
    /// </summary>
    internal class WiiMoteController :IDisposable
    {
        Wiimote m_WiiMote;
        private delegate WiimoteLib.Point MouseCursorPosition(WiimoteChangedEventArgs args);
        public static bool m_IsConnected = false;
        private int m_ConnectionRetries = 0;


        public void InitiateConnect( )
        {
            try
            {
                m_WiiMote = new Wiimote();
                m_WiiMote.Connect();
                m_WiiMote.WiimoteChanged += Wiimote_WiimoteChanged;
                m_ConnectionRetries++;
                m_WiiMote.SetReportType(InputReport.IRAccel, IRSensitivity.Maximum, true);
                m_WiiMote.SetLEDs(true, false, false, false);
                M_IsConnected = true;
            }
            catch
            {
                M_IsConnected = false;
            }
        }

        private void Wiimote_WiimoteChanged(object sender, WiimoteChangedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(UpdateWiimoteChanged, e);
        }
        
        private void UpdateWiimoteChanged(object args)
        {
            var eventArgs = args as WiimoteChangedEventArgs;
            if (args == null)
            {
                return;
            }

            WiimoteState ws = eventArgs.WiimoteState;
            if (ws == null)
            {
                return;
            }

            Cursor.SetPosition(eventArgs);
            Button.AnalyzeButton(ws); //button controller

        }

        public void Dispose()
        {
            m_WiiMote.Disconnect();
            m_WiiMote.Dispose();
            m_WiiMote = null;
        }

        public bool M_IsConnected
        {
            get { return m_IsConnected; }
            set { m_IsConnected = value; }
        }
    }
}
