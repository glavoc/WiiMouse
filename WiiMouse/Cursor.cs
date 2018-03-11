using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using WiimoteLib;
using System.Configuration;

namespace WiiMouse
{
    public static class Cursor
    {

        private static PointF m_FirstSensorPos;
        private static PointF m_SecondSensorPos;
        private static PointF m_MidSensorPos;

        private static ButtonState m_LatestButtonState;


        /// <summary>
        /// Calculates the Cursor Position on Screen by using the Midpoint of the 2 Leds in the sensor bar
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static void SetPosition(WiimoteChangedEventArgs args)
        {
            double minXPos = 0;
            double maxXPos = System.Windows.SystemParameters.PrimaryScreenWidth;
            double maxWidth = maxXPos - minXPos;
            double x;
            double minYPos = 0;
            double maxYPos = System.Windows.SystemParameters.PrimaryScreenHeight;
            double maxHeight = maxYPos - minYPos;
            double y;

            PointF relativePosition = new PointF();
            if (args.WiimoteState.IRState.IRSensors[0].Found && args.WiimoteState.IRState.IRSensors[1].Found)
            {
                relativePosition = args.WiimoteState.IRState.Midpoint;
            }
            else if (args.WiimoteState.IRState.IRSensors[0].Found)
            {
                relativePosition.X = m_MidSensorPos.X + (args.WiimoteState.IRState.IRSensors[0].Position.X - m_FirstSensorPos.X);
                relativePosition.Y = m_MidSensorPos.Y + (args.WiimoteState.IRState.IRSensors[0].Position.Y - m_FirstSensorPos.Y);
            }
            else if (args.WiimoteState.IRState.IRSensors[1].Found)
            {
                relativePosition.X = m_MidSensorPos.X + (args.WiimoteState.IRState.IRSensors[1].Position.X - m_SecondSensorPos.X);
                relativePosition.Y = m_MidSensorPos.Y + (args.WiimoteState.IRState.IRSensors[1].Position.Y - m_SecondSensorPos.Y);
            }

            //Remember for next run
            m_FirstSensorPos = args.WiimoteState.IRState.IRSensors[0].Position;
            m_SecondSensorPos = args.WiimoteState.IRState.IRSensors[1].Position;
            m_MidSensorPos = relativePosition;

            x = Convert.ToInt32((float)maxWidth * (1.0F - relativePosition.X)) + minXPos;
            y = Convert.ToInt32((float)maxHeight * relativePosition.Y) + minYPos;
            if (x < 0)
            {
                x = 0;
            }
            else if (x > System.Windows.SystemParameters.PrimaryScreenWidth)
            {
                x = System.Windows.SystemParameters.PrimaryScreenWidth;
            }
            if (y < 0)
            {
                y = 0;
            }
            else if (y > System.Windows.SystemParameters.PrimaryScreenHeight)
            {
                y = System.Windows.SystemParameters.PrimaryScreenHeight;
            }

            WiimoteLib.Point point = new WiimoteLib.Point();
            point.X = (int) x;
            point.Y = (int) y;
            Win32.SetCursorPos((int)x, (int)y); //set mouse position to wii point

        }
    }


    public static class Button
    {
        #region WiiMote Button Constants
        const int nWII_BUTTON_A = 0x0008;
        const int nWII_BUTTON_B = 0x0004;
        const int nWII_EVENT_DOWN = 1;
        const int nWII_EVENT_UP = 2;
        #endregion WiiMote Button Constants

        #region WiiMote Button Translations
        static string strButtonA = ConfigurationManager.AppSettings["WiiMote_A"];
        static string strButtonB = ConfigurationManager.AppSettings["WiiMote_B"];
        #endregion WiiMote Button Translations

        #region struct
        // Some helpful structs ...:
        public struct ButtonState
        {
            public bool PushFlag;
        }

        public struct WiiMoteButtonState
        {
            public ButtonState ButtonA;
            public ButtonState ButtonB;
        }
        #endregion struct

        // Instantiation of 'myButtons':
        static WiiMoteButtonState myButtons = new WiiMoteButtonState();

        static bool bRepeatFlag = false;   // To drag or not to drag ...
        static DateTime nLastPushTime;
        //---------------------------------------------------------------------------------------------
        #region ReadButtonAssignment
        // Here we ask for the button mapping of the WiiMote wrt the Mouse functions
        // Get the info from the XML-configuration file (App.Config):

        private static void ReadButtonAssignment()
        {
            strButtonA = ConfigurationManager.AppSettings["WiiMote_A"];
            strButtonB = ConfigurationManager.AppSettings["WiiMote_B"];
        }
        #endregion ReadButtonAssignment
        //---------------------------------------------------------------------------------------------
        #region AnalyzeButton
        // Read and interpret the WiiMote data, acquired via Bluetooth;  basic work is done in
        // the library (Brian Peek-MVP C#/Coding4Fun). We are using the its interfaces:

        public static void AnalyzeButton(WiimoteState ws)
        {
            if (ws.ButtonState.A)
                ButtonEvent(nWII_BUTTON_A, nWII_EVENT_DOWN, ref myButtons.ButtonA); // EventDown == '1'
            else
                ButtonEvent(nWII_BUTTON_A, nWII_EVENT_UP, ref myButtons.ButtonA); // EventDown == '2'

            if (ws.ButtonState.B)
                ButtonEvent(nWII_BUTTON_B, nWII_EVENT_DOWN, ref myButtons.ButtonB); // EventDown == '1'
            else
                ButtonEvent(nWII_BUTTON_B, nWII_EVENT_UP, ref myButtons.ButtonB); // EventDown == '2'
        }
        #endregion AnalyzeButton

        //---------------------------------------------------------------------------------------------
        #region ButtonEvent
        // ButtonEvent prepares the actual event performing, providing the event method with
        // the necessary input: which_button, what_event_type and whether it's a click or
        // dragging event.

        private static void ButtonEvent(int Button, int EventType, ref ButtonState ButtonState)
        {
            if (((EventType == nWII_EVENT_UP) && ButtonState.PushFlag) ||
                ((EventType == nWII_EVENT_DOWN) && !(ButtonState.PushFlag)))
            {
                switch (Button)             // Which button are we dealing with ...
                {
                    case (nWII_BUTTON_A):
                        {
                            bRepeatFlag = false;
                            PerformEvent(strButtonA, EventType, bRepeatFlag);
                            break;
                        }
                    case (nWII_BUTTON_B):
                        {
                            bRepeatFlag = false;
                            PerformEvent(strButtonB, EventType, bRepeatFlag);
                            break;
                        }
                    default: break;
                }
            }
            // Administer the PushFlag:
            if (EventType == nWII_EVENT_DOWN) ButtonState.PushFlag = true;
            if (EventType == nWII_EVENT_UP) ButtonState.PushFlag = false;
        }
        #endregion ButtonEvent
        //---------------------------------------------------------------------------------------------
        #region PerformEvent
        // Method performing the Mouse activities, based on the WIN32-API calls (P/Invoke):

        private static void PerformEvent(string ButtonAssignType, int EventType, bool RepeatFlag)
        {
            if ((EventType == nWII_EVENT_DOWN) && (RepeatFlag == false))
                nLastPushTime = DateTime.Now; ;

            //tbxLogging.Clear();     // Debug only ...
            //tbxLogging.AppendText("\r\nLast-Push-Time: " + nLastPushTime.ToString());

            // Do the Mouse Event ...:
            if (ButtonAssignType == "MOUSE_LEFT")
                switch (EventType)
                {
                    case nWII_EVENT_DOWN:
                        Win32.mouse_event(Win32.MouseEventType.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        break;
                    case nWII_EVENT_UP:
                        Win32.mouse_event(Win32.MouseEventType.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        break;
                    default: break;
                }

            if (ButtonAssignType == "MOUSE_RIGHT")
                switch (EventType)
                {
                    case nWII_EVENT_DOWN:
                        Win32.mouse_event(Win32.MouseEventType.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                        break;
                    case nWII_EVENT_UP:
                        Win32.mouse_event(Win32.MouseEventType.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        break;
                    default: break;
                }
        }
        #endregion PerformEvent

    }
}

#region WIN32-API Class
public class Win32
{
    // The following WIN32-API moves the mouse/cursor:
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    public static extern void mouse_event(MouseEventType dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
    public enum MouseEventType : int
    {
        MOUSEEVENTF_LEFTDOWN = 0x02,
        MOUSEEVENTF_LEFTUP = 0x04,
        MOUSEEVENTF_RIGHTDOWN = 0x08,
        MOUSEEVENTF_RIGHTUP = 0x10
    }
}
#endregion WIN32-API Class