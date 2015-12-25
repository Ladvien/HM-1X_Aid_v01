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
                                   "AT+BIT","AT+BAUD","AT+COMI","AT+COMA","AT+COLA","AT+COUP","AT+CHAR", "AT+CLEAR","AT+CONL","AT+CO","AT+COL",
                                   "AT+CYC", "AT+DISC", "AT+DISI", "AT+CONN", "AT+DELO", "AT+ERASE", "AT+FLAG", "AT+FILT", "AT+FIOW", "AT+GAIN",
                                   "AT+HELP", "AT+IMME", "AT+IBEA", "AT+BEA0", "AT+BEA1", "AT+BEA2", "AT+BEA3", "AT+MARJ", "AT+MINO", "AT+MEAS",
                                   "AT+MODE", "AT+NOTI", "AT+NOTP", "AT+NAME", "AT+PCTL", "AT+PARI", "AT+PIO", "AT+PASS", "AT+PIN", "AT+POWE",
                                   "AT+PWRM", "AT+RELI", "AT+RENEW", "AT+RESTART", "AT+ROLE", "AT+RSSI", "AT+RADD", "AT+RAT", "AT+STOP", "AT+START",
                                   "AT+SLEEP", "AT+SAVE", "AT+SENS", "AT+SHOW", "AT+TEHU", "AT+TEMP", "AT+TCON", "AT+TYPE", "AT+UUID", "AT+UART","AT+VERS" };

        public enum hm1xEnumCommands : int
        {
            None = 0, CheckStatus, ADC, MACAddress, AdvertizingInterval, AdvertizingType, ANCS, WhiteListSwitch, WhitelistMACAddress, PIOStateAfterPowerOn, PIOStateAfterConnection,
            BatteryMonitor, BatteryInformation, BitFormat, BaudRate, MinLinkLayerInterval, MaxLinkLayerInterval, LinkLayerSlaveLatency, UpdateConnectionParameter, Characteristic,
            ClearLastConnected, TryLastConnected, TryConnectionAddress, PIOState, PIOCollectionRate, StartDiscovery, StartiBeaconDiscovery, ConnectToDiscoveredDevice, iBeaconMode, RemoveBondInfo,
            AdvertizingFlag, HM1XConnectionFilter, FlowControlSwitch, RXGain, Help, WorkType, iBeaconModeSwitch, iBeaconUUID0, iBeaconUUID1, iBeaconUUID2, iBeaconUUID3,
            iBeaconMajorVersion, iBeaconMinorVersion, iBeaconMeasuredPower, WorkMode, ConnectionNotification, ConnNotificationMode, Name, OutputDriver, Parity, ConnectionLEDMode, SetPIOTemp, Pin,
            PowerLevel, SleepType, ReliableAdvertizing, Renew, Reset, Role, RSSI, LastConnectedAddress, SensorWorkInterval, StopBits, StartWork, Sleep, SaveConnectedAddress, SensorType,
            DiscoveryParameter, TemperatureSensor, ICTemperature, RemoteDeviceTimeout, BondType, Service, WakeThroughUART, Version
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
            "Connect to HM1X Only",
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
            "Version"
        };
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
                        settingsExplained.Items.Add(deviceType.ToString() + " does not support this feature.");
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
                //"AT+BATC"
                //"AT+BATT",
                //"AT+BIT",
                //"AT+BAUD",
                //"AT+COMI",
                //"AT+COMA",
                //"AT+COLA",
                //"AT+COUP",
                //"AT+CHAR", 
                //"AT+CLEAR",
                //"AT+CONL",
                //"AT+CO",
                //"AT+COL",
                //"AT+CYC", 
                //"AT+DISC", 
                //"AT+DISI", 
                //"AT+CONN", 
                //"AT+DELO", 
                //"AT+ERASE", 
                //"AT+FLAG", 
                //"AT+FILT", 
                //"AT+FIOW", 
                //"AT+GAIN",
                //"AT+HELP
                //"AT+IMME
                //"AT+IBEA
                //"AT+BEA0
                //"AT+BEA1
                //"AT+BEA2
                //"AT+BEA3
                //"AT+MARJ
                //"AT+MINO
                //"AT+MEAS
                //"AT+MODE
                //"AT+NOTI
                //"AT+NOTP
                //"AT+NAME
                //"AT+PCTL
                //"AT+PARI
                //"AT+PIO
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

        public void adjustParametersAndOtherSettings(hm1xConstants.hm1xEnumCommands enumeratedSelection, int settingsSelection, TextBox parameterOne, TextBox parameterTwo)
        {

            switch (enumeratedSelection)
            {
                case hm1xConstants.hm1xEnumCommands.WhitelistMACAddress:
                    if (settingsSelection > 2)
                    {
                        int macAddressCharCount = parameterOne.Text.Count();
                        if (macAddressCharCount < 12)
                        {
                            for (int i = 0; i < (12 - macAddressCharCount); i++)
                            {
                                parameterOne.Text += "0";
                            }
                        }
                        else if (macAddressCharCount > 12)
                        {
                            Console.WriteLine(macAddressCharCount);
                            Console.WriteLine(macAddressCharCount - 12);
                            parameterOne.Text = parameterOne.Text.Remove(12, macAddressCharCount - 12);
                        }
                    }
                    else
                    {
                        // Must clear the parameter box or it will intefer with other commands.S
                        parameterOne.Text = "";
                    }
                    break;
                case hm1xConstants.hm1xEnumCommands.PIOStateAfterPowerOn:
                    if (settingsSelection > 0)
                    {
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
                            Console.WriteLine(numberOfBits);
                            Console.WriteLine(numberOfBits - 12);
                            parameterOne.Text = parameterOne.Text.Remove(12, numberOfBits - 12);
                        }
                        parameterOne.Text = BinaryStringToHexString(parameterOne.Text);
                        Console.WriteLine(parameterOne.Text);
                        // Remove the upper nibble.
                        parameterOne.Text = parameterOne.Text.Remove(0, 1);
                        Console.WriteLine(parameterOne.Text);
                    }
                    else
                    {
                        // Must clear the parameter box or it will intefer with other commands.S
                        parameterOne.Text = "";
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

    } // End hm1xSettings

} // End Namespace
