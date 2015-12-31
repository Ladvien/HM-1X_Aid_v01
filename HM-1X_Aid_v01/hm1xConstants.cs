using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HM_1X_Aid_v01
{
    public class hm1xConstants
    {


        public static string[] hm1xCommandsString = {"None", "AT", "AT+ADC", "AT+ADDR","AT+ADVI","AT+ADTY","AT+ANCS","AT+ALLO","AT+AD","AT+BEFC","AT+AFTC","AT+BATC","AT+BATT",
                                   "AT+BIT7","AT+BAUD","AT+COMI","AT+COMA","AT+COLA","AT+COUP","AT+CHAR", "AT+CLEAR","AT+CONNL","AT+CO","AT+COL",
                                   "AT+CYC", "AT+DISC", "AT+DISI", "AT+CONN", "AT+DELO", "AT+ERASE", "AT+FLAG", "AT+FILT", "AT+FIOW", "AT+GAIN",
                                   "AT+HELP", "AT+IMME", "AT+IBEA", "AT+BEA0", "AT+BEA1", "AT+BEA2", "AT+BEA3", "AT+MARJ", "AT+MINO", "AT+MEAS",
                                   "AT+MODE", "AT+NOTI", "AT+NOTP", "AT+NAME", "AT+PCTL", "AT+PARI", "AT+PIO1", "AT+PASS", "AT+PIN", "AT+POWE",
                                   "AT+PWRM", "AT+RELI", "AT+RENEW", "AT+RESTART", "AT+ROLE", "AT+RSSI", "AT+RADD", "AT+RAT", "AT+STOP", "AT+START",
                                   "AT+SLEEP", "AT+SAVE", "AT+SENS", "AT+SHOW", "AT+TEHU", "AT+TEMP", "AT+TCON", "AT+TYPE", "AT+UUID", "AT+UART","AT+VERS", "ERROR"};

        public enum hm1xEnumCommands : int
        {
            None = 0, CheckStatus, ADC, MACAddress, AdvertizingInterval, AdvertizingType, ANCS, WhiteListSwitch, WhitelistMACAddress, PIOStateAfterPowerOn, PIOStateAfterConnection,
            BatteryMonitor, BatteryInformation, BitFormat, BaudRate, MinLinkLayerInterval, MaxLinkLayerInterval, LinkLayerSlaveLatency, UpdateConnectionParameter, Characteristic,
            ClearLastConnected, TryLastConnected, TryConnectionAddress, PIOState, PIOCollectionRate, StartDiscovery, StartiBeaconDiscovery, ConnectToDiscoveredDevice, iBeaconMode, RemoveBondInfo,
            AdvertizingFlag, HM1XConnectionFilter, FlowControlSwitch, RXGain, Help, WorkType, iBeaconModeSwitch, iBeaconUUID0, iBeaconUUID1, iBeaconUUID2, iBeaconUUID3,
            iBeaconMajorVersion, iBeaconMinorVersion, iBeaconMeasuredPower, WorkMode, ConnectionNotification, ConnNotificationMode, Name, OutputDriver, Parity, ConnectionLEDMode, SetPIOTemp, Pin,
            PowerLevel, SleepType, ReliableAdvertizing, Renew, Reset, Role, RSSI, LastConnectedAddress, SensorWorkInterval, StopBits, StartWork, Sleep, SaveConnectedAddress, SensorType,
            DiscoveryParameter, TemperatureSensor, ICTemperature, RemoteDeviceTimeout, BondType, Service, WakeThroughUART, Version, ERROR
        }

        public enum hm1xDeviceType : int { Unknown = 0, HM10 = 1, HM11 = 2, HM15 = 3 };

        public static string[] hm1xCommandsExplained =
        {                                       // Enumerated Command
            "None",                             // None 
            "Check Status",                     // CheckStatus
            "ADC Info",                         // ADC
            "MAC Address",                      // MacAddress
            "Advertizing Interval",
            "Advertizing Type",
            "ANCS",
            "Whitelist",
            "Whitelist MAC Addresses",
            "PIO State After Power On",
            "PIO State After Connection",
            "Battery Monitor",
            "Battery Information",
            "Bit Format",
            "Baud Rate",
            "Minimum Link Layer Interval",
            "Maximum Link Layer Interval",
            "Link Layer Slave Latency",
            "Update Connection Parameter",
            "Characteristic",
            "Clear Last Connected",
            "Try Last Connected",
            "Try Connection Address",
            "Pin IO State",
            "Pin IO Collection Rate",
            "Start Discovery",
            "Start iBeacon Discovery",
            "Connect to Discovered Device",
            "iBeacon Mode",
            "Remove Bond Information",
            "Advertizing Flag",
            "Connect to HM Only (deprecated)",
            "Flow Control Switch",
            "RX Gain",
            "Help",
            "Work Type",
            "iBeacon Mode Switch",
            "iBeacon UUID 0",
            "iBeacon UUID 1",
            "iBeacon UUID 2",
            "iBeacon UUID 3",
            "iBeacon Major Version",
            "iBeacon Minor Version",
            "iBeacon Measured Power",
            "Work Mode",
            "Connection Notification",
            "Connection Notification Mode",
            "Name",
            "Output Driver",
            "Parity",
            "Connection LED Mode",
            "Set Pin IO Temp",
            "Password",
            "Power Level",
            "Sleep Type",
            "Reliable Advertizing",
            "Renew",
            "Reset",
            "Role",
            "RSSI",
            "Last Connected Address",
            "Sensor Work Interval",
            "Stop Bits",
            "Start Work",
            "Sleep",
            "Save Connection Address",
            "Sensor Type",
            "Discovery Parameter",
            "Temperature Sensor",
            "IC Temperature",
            "Remove Device Timeout",
            "Bond Type",
            "Service",
            "Wake Through UART",
            "Version",
            "ERROR"
        };

        public string getCommandStringFromEnum(hm1xConstants.hm1xEnumCommands enumeratedSelection)
        {
            string returnString = hm1xCommandsString[(int)enumeratedSelection];
            return returnString;
        }
    }
    
    public class hm1xSettings
    {
        public void getSettingsHM10(ComboBox settingsExplained, List<string> atCommandList, hm1xConstants.hm1xEnumCommands command,
            TextBox parameterOne, TextBox parameterTwo, Label parameterOneLbl, Label parameterTwoLbl, hm1xConstants.hm1xDeviceType deviceType, Button confirmButton)
        {

            // Make sure we reset the confirm button to be enabled.
            confirmButton.Enabled = true;
            // Make sure previous settings for the combo box are cleared.
            settingsExplained.Items.Clear();

            switch (command)
            {
                //"AT"
                case hm1xConstants.hm1xEnumCommands.CheckStatus:
                    settingsExplained.Enabled = false;
                    break;
                //"None"
                case hm1xConstants.hm1xEnumCommands.None:
                    atCommandList.Add("");
                    settingsExplained.Items.Add("None");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    settingsExplained.Enabled = false;
                    break;
                //"AT+ADC"
                case hm1xConstants.hm1xEnumCommands.ADC:
                    if (deviceType == hm1xConstants.hm1xDeviceType.HM10)
                    {
                        settingsExplained.Items.Add("Pin 4");
                        atCommandList.Add("4?");
                        settingsExplained.Items.Add("Pin 5");
                        atCommandList.Add("5?");
                        settingsExplained.Items.Add("Pin 6");
                        atCommandList.Add("6?");
                        settingsExplained.Items.Add("Pin 7");
                        atCommandList.Add("7?");
                        settingsExplained.Items.Add("Pin 8");
                        atCommandList.Add("8?");
                        settingsExplained.Items.Add("Pin 9");
                        atCommandList.Add("9?");
                        settingsExplained.Items.Add("Pin A");
                        atCommandList.Add("A?");
                        settingsExplained.Items.Add("Pin B");
                        atCommandList.Add("B?");
                        settingsExplained.Enabled = true;
                    }
                    else
                    {
                        atCommandList.Add("");
                        settingsExplained.Items.Add(deviceType.ToString() + " does not support this feature");
                        confirmButton.Enabled = false;
                        settingsExplained.Enabled = false;
                    }
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+ADDR"
                case hm1xConstants.hm1xEnumCommands.MACAddress:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get this device's MAC Address");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+ADVI"
                case hm1xConstants.hm1xEnumCommands.AdvertizingInterval:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Status");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("100.00ms");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("152.50ms");
                    atCommandList.Add("2");
                    settingsExplained.Items.Add("211.25");
                    atCommandList.Add("3");
                    settingsExplained.Items.Add("318.75ms");
                    atCommandList.Add("4");
                    settingsExplained.Items.Add("417.50ms");
                    atCommandList.Add("5");
                    settingsExplained.Items.Add("546.25ms");
                    atCommandList.Add("6");
                    settingsExplained.Items.Add("760.00ms");
                    atCommandList.Add("7");
                    settingsExplained.Items.Add("852.50s");
                    atCommandList.Add("8");
                    settingsExplained.Items.Add("1022.50ms");
                    atCommandList.Add("8");
                    settingsExplained.Items.Add("1285.00ms");
                    atCommandList.Add("9");
                    settingsExplained.Items.Add("2000.00ms");
                    atCommandList.Add("A");
                    settingsExplained.Items.Add("3000.00ms");
                    atCommandList.Add("B");
                    settingsExplained.Items.Add("4000.00ms");
                    atCommandList.Add("C");
                    settingsExplained.Items.Add("5000.00ms");
                    atCommandList.Add("D");
                    settingsExplained.Items.Add("6000.00ms");
                    atCommandList.Add("E");
                    settingsExplained.Items.Add("7000.00ms");
                    atCommandList.Add("F");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+ADTY"
                case hm1xConstants.hm1xEnumCommands.AdvertizingType:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Status");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("Advertising, Scan-Response, Connectable");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Only permit last device within 1.28 seconds");
                    atCommandList.Add("2");
                    settingsExplained.Items.Add("Allow advertizing and Scan-Response");
                    atCommandList.Add("3");
                    settingsExplained.Items.Add("Only allow advertizing");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+ANCS"
                case hm1xConstants.hm1xEnumCommands.ANCS:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Status");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("Turn ANCS Off");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Turn ANCS On");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+ALLO"
                case hm1xConstants.hm1xEnumCommands.WhiteListSwitch:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Status");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("Turn OFF Whitelist");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Turn ON Whitelist");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+AD"
                case hm1xConstants.hm1xEnumCommands.WhitelistMACAddress:
                    atCommandList.Add("1??");
                    settingsExplained.Items.Add("Get Status Address 1");
                    atCommandList.Add("2??");
                    settingsExplained.Items.Add("Get Status Address 2");
                    atCommandList.Add("3??");
                    settingsExplained.Items.Add("Get Status Address 3");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Set Whitelist MAC Address 1");
                    atCommandList.Add("2");
                    settingsExplained.Items.Add("Set Whitelist MAC Address 2");
                    atCommandList.Add("3");
                    settingsExplained.Items.Add("Set Whitelist MAC Address 2");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+BEFC"
                case hm1xConstants.hm1xEnumCommands.PIOStateAfterPowerOn:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get PIO State After Powered");
                    atCommandList.Add("");
                    settingsExplained.Items.Add("Set PIO After Powered");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+AFTC"
                case hm1xConstants.hm1xEnumCommands.PIOStateAfterConnection:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get PIO State After Connected");
                    atCommandList.Add("");
                    settingsExplained.Items.Add("Set PIO After Connected");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+BATC"
                case hm1xConstants.hm1xEnumCommands.BatteryMonitor:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Battery Monitor State");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("Turn Battery Monitor Off");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Turn Battery Monitor On");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+BATT",
                case hm1xConstants.hm1xEnumCommands.BatteryInformation:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Battery Monitor Information");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+BIT7",
                case hm1xConstants.hm1xEnumCommands.BitFormat:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get 7-Bit Setting");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("Set to 7-Bit NOT Compatible");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Set to 7-Bit Compatible");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+BAUD",
                case hm1xConstants.hm1xEnumCommands.BaudRate:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Baud Rate");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("Set Baud to 9600");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Set Baud to 19200");
                    atCommandList.Add("2");
                    settingsExplained.Items.Add("Set Baud to 38400");
                    atCommandList.Add("3");
                    settingsExplained.Items.Add("Set Baud to 57600");
                    atCommandList.Add("4");
                    settingsExplained.Items.Add("Set Baud to 115200");
                    atCommandList.Add("5");
                    settingsExplained.Items.Add("Set Baud to 4800");
                    atCommandList.Add("6");
                    settingsExplained.Items.Add("Set Baud to 2400");
                    atCommandList.Add("7");
                    settingsExplained.Items.Add("Set Baud to 1200");
                    atCommandList.Add("8");
                    settingsExplained.Items.Add("Set Baud to 230400");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+COMI",
                    // Will Add after upgrade.
                //"AT+COMA",
                    // Will Add after upgrade.
                //"AT+COLA",
                    // Will Add after upgrade.
                //"AT+COUP",
                    // Will Add after upgrade.
                //"AT+CHAR", 
                case hm1xConstants.hm1xEnumCommands.Characteristic:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Current Characteristic Value");
                    atCommandList.Add("");
                    settingsExplained.Items.Add("Set Characteristic Value");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+CLEAR",
                case hm1xConstants.hm1xEnumCommands.ClearLastConnected:
                    atCommandList.Add("");
                    settingsExplained.Items.Add("Clear last connected device bond");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+CONNL",
                case hm1xConstants.hm1xEnumCommands.TryLastConnected:
                    atCommandList.Add("");
                    settingsExplained.Items.Add("Try to connect to last connected device");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+CO",
                case hm1xConstants.hm1xEnumCommands.TryConnectionAddress:
                    atCommandList.Add("N");
                    settingsExplained.Items.Add("Bluetooth Low Energy Address");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Dual Module Address");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+COL",
                case hm1xConstants.hm1xEnumCommands.PIOState:
                    atCommandList.Add("??");
                    settingsExplained.Items.Add("Get Pin IO State");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+CYC", 
                case hm1xConstants.hm1xEnumCommands.PIOCollectionRate:
                    atCommandList.Add("??");
                    settingsExplained.Items.Add("Get Pin IO Collection Rate");
                    atCommandList.Add("");
                    settingsExplained.Items.Add("Set Pin IO Collection Rate");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+DISC", 
                case hm1xConstants.hm1xEnumCommands.StartDiscovery:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Start Discovery");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+DISI", 
                ///////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////Must Finish After Upgrade//////////////////////////////////////

                // Requires v539
                case hm1xConstants.hm1xEnumCommands.StartiBeaconDiscovery:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Start Discovery");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;

                ///////////////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////////////
                //"AT+CONN", 
                case hm1xConstants.hm1xEnumCommands.ConnectToDiscoveredDevice:
                    atCommandList.Add("");
                    settingsExplained.Items.Add("Connect to Device Number...");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+DELO", 
                case hm1xConstants.hm1xEnumCommands.iBeaconMode:
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Allowed to broadcast and scan");
                    atCommandList.Add("2");
                    settingsExplained.Items.Add("Only allow broadcast");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+ERASE", 
                case hm1xConstants.hm1xEnumCommands.RemoveBondInfo:
                    atCommandList.Add("");
                    settingsExplained.Items.Add("Remove bond info");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+FLAG", 
                case hm1xConstants.hm1xEnumCommands.AdvertizingFlag:
                    atCommandList.Add("");
                    settingsExplained.Items.Add("Set Advertizing Byte");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+FILT", 
                case hm1xConstants.hm1xEnumCommands.HM1XConnectionFilter:
                    settingsExplained.Enabled = false;
                     /*
                    atCommandList.Add("");
                    settingsExplained.Items.Add("Deprecated since v530");
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Filter Setting");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Find all BLE Devices");
                    atCommandList.Add("2");
                    settingsExplained.Items.Add("Find only HM Modules");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;*/
                    break;
                //"AT+FIOW", 
                case hm1xConstants.hm1xEnumCommands.FlowControlSwitch:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get flow control setting");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("Set flow control ON");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Set flow control OFF");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+GAIN",
                case hm1xConstants.hm1xEnumCommands.RXGain:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get RX Gain setting");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("Set RX Gain ON");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Set RX Gain OFF");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+HELP
                case hm1xConstants.hm1xEnumCommands.Help:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get JNHuamao Website");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+IMME
                case hm1xConstants.hm1xEnumCommands.WorkType:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Work Type");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("Set Enter Serial Mode with Start Command");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Set Enter Serial Mode Immediately");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+IBEA
                //"AT+BEA0
                //"AT+BEA1
                //"AT+BEA2
                //"AT+BEA3
                //"AT+MARJ
                //"AT+MINO
                //"AT+MEAS
                //"AT+MODE
                case hm1xConstants.hm1xEnumCommands.WorkMode:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Transmission Mode");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("Set Module to Transmission Mode");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Set PIO Collection and Transmission Mode");
                    atCommandList.Add("2");
                    settingsExplained.Items.Add("Set to Remote Control Mode");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+NOTI
                case hm1xConstants.hm1xEnumCommands.ConnectionNotification:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Notification on Connection Mode");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("Set to NOT notifty on connection");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Set to notifty on connection");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+NOTP
                case hm1xConstants.hm1xEnumCommands.ConnNotificationMode:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Notify with Address Setting.");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("Set to Notify without Address");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Set With Addresss");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+NAME
                case hm1xConstants.hm1xEnumCommands.Name:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Name of Module");
                    atCommandList.Add("");
                    settingsExplained.Items.Add("Set Name of Module");;
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+PCTL
                case hm1xConstants.hm1xEnumCommands.OutputDriver:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Output Power Driver Setting");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("Set Output Power Driver NORMAL");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Set Output Power Driver HIGH");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+PARI
                case hm1xConstants.hm1xEnumCommands.Parity:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Current Parity setting");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("Set Parity to None");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Set Parity to EVEN");
                    atCommandList.Add("2");
                    settingsExplained.Items.Add("Set Parity to ODD");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+PIO
                case hm1xConstants.hm1xEnumCommands.ConnectionLEDMode:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get LED Connection Behavior");
                    atCommandList.Add("0");
                    settingsExplained.Items.Add("Unconnected - Blink 500ms");
                    atCommandList.Add("1");
                    settingsExplained.Items.Add("Unconnected - Low, Connected - High");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+PASS
                //"AT+PIN
                //"AT+POWE
                //"AT+PWRM
                //"AT+RELI
                //"AT+RENEW
                //"AT+RESTART
                //"AT+ROLE
                //"AT+RSSI
                //"AT+RADD
                case hm1xConstants.hm1xEnumCommands.LastConnectedAddress:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get MAC Address of Last Connected Device");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
                //"AT+RAT

                //"AT+STOP
                //"AT+START
                //"AT+SLEEP
                //"AT+SAVE
                //"AT+SENS
                //"AT+SHOW
                //"AT+TEHU
                //"AT+TEMP
                //"AT+TCON
                //"AT+TYPE
                //"AT+UUID
                //"AT+UART
                //"AT+VERS"
                case hm1xConstants.hm1xEnumCommands.Version:
                    atCommandList.Add("?");
                    settingsExplained.Items.Add("Get Version");
                    settingsExplained.Enabled = true;
                    settingsExplained.SelectedIndex = 0;
                    break;
            }
        }

        public int getResponseTimeNeeded(hm1xConstants.hm1xEnumCommands selectedEnumeration)
        {
            int millisecondsReturned = 0;

            switch (selectedEnumeration)
            {
                case hm1xConstants.hm1xEnumCommands.TryConnectionAddress:
                    millisecondsReturned = 12000;
                    break;
                case hm1xConstants.hm1xEnumCommands.PIOState:
                    millisecondsReturned = 450;
                    break;
                case hm1xConstants.hm1xEnumCommands.ConnectToDiscoveredDevice:
                    millisecondsReturned = 120000;
                    break;
                case hm1xConstants.hm1xEnumCommands.iBeaconMode:
                    millisecondsReturned = 12000;
                    break;
                default:
                    millisecondsReturned = 300;
                    break;
            }
            return millisecondsReturned;
        }

        public void adjustParametersAndOtherSettings(hm1xConstants.hm1xEnumCommands selectedEnumeration, int settingsSelection, TextBox parameterOne, TextBox parameterTwo)
        {
            /// Keep it DRY.  The PowerOn and AfterConnection are the same case.
            if(selectedEnumeration == hm1xConstants.hm1xEnumCommands.PIOStateAfterConnection) { selectedEnumeration = hm1xConstants.hm1xEnumCommands.PIOStateAfterPowerOn; }

            switch (selectedEnumeration)
            {
                case hm1xConstants.hm1xEnumCommands.WhitelistMACAddress:
                    if (settingsSelection > 2)
                    {
                        parameterOne.Text = conformMACAddress(parameterOne.Text);
                    }
                    else
                    {
                        // Must clear the parameter box or it will intefer with other commands.S
                        parameterOne.Text = "";
                    }
                    break;
                case hm1xConstants.hm1xEnumCommands.PIOStateAfterPowerOn:

                    // Check to make sure we got a combobox index.
                    if (settingsSelection > 0)
                    {
                        parameterOne.Text = parameterOne.Text.ToUpper();
                        // Turn all the "bits" into a charArray to check each of their validity.
                        char[] trueBitCharArray = parameterOne.Text.ToCharArray();
                        for (int i = 0; i < trueBitCharArray.Count(); i++)
                        {
                            if (trueBitCharArray[i] > '1') { trueBitCharArray[i] = '1'; } else if (trueBitCharArray[i] < '0') { trueBitCharArray[i] = '0'; }
                        }

                        parameterOne.Text = new string(trueBitCharArray);

                        // Check to make sure it is not more or less than 12 characters.
                        int numberOfBits = parameterOne.Text.Count();
                        if (numberOfBits < 13)
                        {
                            for (int i = 0; i < (12 - numberOfBits); i++)
                            {
                                parameterOne.Text += "0";
                            }
                        }
                        else if (numberOfBits > 12)
                        {
                            parameterOne.Text = parameterOne.Text.Remove(12, numberOfBits - 12);
                        }
                        
                        // Convert string bits into string hex.
                        parameterOne.Text = BinaryStringToHexString(parameterOne.Text);
                        
                        // Remove the upper nibble.
                        parameterOne.Text = parameterOne.Text.Remove(0, 1);

                        // Convert back to array to check the range of first char.
                        char[] checkFirstCharRangeCharArray = parameterOne.Text.ToCharArray();
                        if(checkFirstCharRangeCharArray[0] > '3') { checkFirstCharRangeCharArray[0] = '3'; }
                        parameterOne.Text = new string(checkFirstCharRangeCharArray);

                    }
                    else
                    {
                        // Must clear the parameter box or it will intefer with other commands.S
                        parameterOne.Text = "";
                    }
                    break;
                case hm1xConstants.hm1xEnumCommands.Characteristic:
                    if (settingsSelection > 0)
                    {
                        parameterOne.Text = parameterOne.Text.ToUpper();
                        // If there are less than four chars, pad with '0';
                        int characteristicCharCount = parameterOne.Text.Count();
                        if (characteristicCharCount < 4)
                        {
                            for (int i = 0; i < (4 - characteristicCharCount); i++)
                            {
                                parameterOne.Text += "0";
                            }
                        }
                        else if (characteristicCharCount > 4)  // If there are more than four characters, trim it.
                        {
                            parameterOne.Text = parameterOne.Text.Remove(4, characteristicCharCount - 4);
                        }

                        // Make sure all four characters are in HEX range.
                        char[] hexCheckCharArray = parameterOne.Text.ToCharArray();
                        for(int i = 0; i < 4; i++)
                        {
                            if(hexCheckCharArray[i] > 'F') { hexCheckCharArray[i] = 'F'; }
                            else if (hexCheckCharArray[i] < '0') { hexCheckCharArray[i] = '0'; }
                        }

                        // Make sure the first and last characters are valid.
                        if(hexCheckCharArray[3] > 'E') { hexCheckCharArray[3] = 'E'; }
                        if (hexCheckCharArray[3] < '1') { hexCheckCharArray[3] = '1'; }
                        parameterOne.Text = "0x" + new string(hexCheckCharArray);

                    }
                    else
                    {
                        // Must clear the parameter box or it will intefer with other commands.S
                        parameterOne.Text = "";
                    }
                    break;
                case hm1xConstants.hm1xEnumCommands.TryConnectionAddress:
                    parameterOne.Text = conformMACAddress(parameterOne.Text);
                    break;
                case hm1xConstants.hm1xEnumCommands.PIOCollectionRate:
                    if (settingsSelection > 0)
                    {
                        parameterOne.Text = parameterOne.Text.ToUpper();
                        // If there are less than four chars, pad with '0';
                        int characteristicCharCount = parameterOne.Text.Count();
                        Console.WriteLine(characteristicCharCount);
                        if (characteristicCharCount < 2)
                        {
                            for (int i = 0; i < (2 - characteristicCharCount); i++)
                            {
                                parameterOne.Text += "0";
                            }
                        }
                        else if (characteristicCharCount > 2)  // If there are more than four characters, trim it.
                        {
                            parameterOne.Text = parameterOne.Text.Remove(2, characteristicCharCount - 2);
                        }

                        // Make sure all four characters are in HEX range.
                        char[] charArrayPIOCollectionRate = parameterOne.Text.ToCharArray();
                        for (int i = 0; i < 2; i++)
                        {
                            if (charArrayPIOCollectionRate[i] > '9') { charArrayPIOCollectionRate[i] = '9'; }
                            else if (charArrayPIOCollectionRate[i] < '0') { charArrayPIOCollectionRate[i] = '0'; }
                        }
                        parameterOne.Text = new string(charArrayPIOCollectionRate);
                    }
                    else
                    {
                        // Must clear the parameter box or it will intefer with other commands.S
                        parameterOne.Text = "";
                    }
                    break;
                case hm1xConstants.hm1xEnumCommands.ConnectToDiscoveredDevice:
                    parameterOne.Text = parameterOne.Text.ToUpper();
                    // If there are less than four chars, pad with '0';
                    int charCount = parameterOne.Text.Count();
                    Console.WriteLine(charCount);

                    // Make sure all four characters are in HEX range.
                    char[] charArray = parameterOne.Text.ToCharArray();
                    if (charCount > 2)  // If there are more than four characters, trim it.
                    {
                        parameterOne.Text = parameterOne.Text.Remove(2, charCount - 2);
                    }
                    else if (charCount == 1) // If there is only one char, let's make it 1-9.
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            if (charArray[i] > '9') { charArray[i] = '9'; }
                            else if (charArray[i] < '0') { charArray[i] = '0'; }
                        }
                        parameterOne.Text = new string(charArray);
                    } else if (charCount == 0)// Else, default to "0"
                    {
                        parameterOne.Text = "0";
                    }
                    else  // This number is just right (has to be two or negative digits)
                    {
                        parameterOne.Text = new string(charArray);
                    }
                    break;
                case hm1xConstants.hm1xEnumCommands.AdvertizingFlag:
                    parameterOne.Text = parameterOne.Text.ToUpper();
                    // If there are less than four chars, pad with '0';
                    int advFlagCharCount = parameterOne.Text.Count();
                    if (advFlagCharCount < 2)
                    {
                        for (int i = 0; i < (1 - advFlagCharCount); i++)
                        {
                            parameterOne.Text += "0";
                        }
                    }
                    else if (advFlagCharCount > 2)  // If there are more than four characters, trim it.
                    {
                        parameterOne.Text = parameterOne.Text.Remove(2, advFlagCharCount - 2);
                    }

                    // Make sure all four characters are in HEX range.
                    char[] advFlagCheckCharArray = parameterOne.Text.ToCharArray();
                    for (int i = 0; i < 2; i++)
                    {
                        if (advFlagCheckCharArray[i] > 'F') { advFlagCheckCharArray[i] = 'F'; }
                        else if (advFlagCheckCharArray[i] < '0') { advFlagCheckCharArray[i] = '0'; }
                    }

                    parameterOne.Text = new string(advFlagCheckCharArray);
                    break;
                case hm1xConstants.hm1xEnumCommands.Name:
                    if(settingsSelection > 0)
                    {
                        int nameLength = parameterOne.Text.Length;
                        if(nameLength > 12)
                        {
                            parameterOne.Text = parameterOne.Text.Remove(12, nameLength - 12);
                            Console.WriteLine(parameterOne.Text);
                        } else if (nameLength < 1)
                        {
                            parameterOne.Text = "ALABTU >:)";
                        }
                    }
                    break;

            }
        }

        // Borrowed from SO.
        // http://stackoverflow.com/questions/5612306/converting-long-string-of-binary-to-hex-c-sharp
        public static string BinaryStringToHexString(string binary)
        {
            StringBuilder result = new StringBuilder(binary.Length / 8 + 1);

            // TODO: check all 1's or 0's... Will throw otherwise

            int mod4Len = binary.Length % 8;
            if (mod4Len != 0)
            {
                // pad to length multiple of 8
                binary = binary.PadLeft(((binary.Length / 8) + 1) * 8, '0');
            }

            for (int i = 0; i < binary.Length; i += 8)
            {
                string eightBits = binary.Substring(i, 8);
                result.AppendFormat("{0:X2}", Convert.ToByte(eightBits, 2));
            }

            return result.ToString();
        }

        private string conformMACAddress(string address)
        {
            string returnString = address;
            returnString = returnString.ToUpper();
            int macAddressCharCount = returnString.Count();
            if (macAddressCharCount < 12)
            {
                for (int i = 0; i < (12 - macAddressCharCount); i++)
                {
                    returnString += "0";
                }
            }
            else if (macAddressCharCount > 12)
            {
                returnString = returnString.Remove(12, macAddressCharCount - 12);
            }

            return returnString;
        }

    } // End hm1xSettings

} // End Namespace
