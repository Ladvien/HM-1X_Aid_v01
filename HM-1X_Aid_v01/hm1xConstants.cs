using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            None = 0, CheckStatus, ADC, MACAddress, AdvertizingInterval, AdvertizingType, ANCS, Whitelist, WhitelistMACAddress, PIOStateAfterPowerOn, PIOStateAfterConnection,
            BatteryMonitor, BatteryInformation, BitFormat, BaudRate, MinLinkLayerInterval, MaxLinkLayerInterval, LinkLayerSlaveLatency, UpdateConnectionParameter, Characteristic,
            ClearLastConnected, TryLastConnected, TryConnectionAddress, PIOState, PIOCollectionRate, StartDiscovery, StartiBeaconDiscovery, ConnectToDiscoveredDevice, iBeaconMode, RemoveBondInfo,
            AdvertizingFlag, HM1XConnectionFilter, FlowControlSwitch, RXGain, Help, WorkType, iBeaconModeSwitch, iBeaconUUID0, iBeaconUUID1, iBeaconUUID2, iBeaconUUID3,
            iBeaconMajorVersion, iBeaconMinorVersion, iBeaconMeasuredPower, WorkMode, ConnectionNotification, ConnNotificationMode, Name, OutputDriver, Parity, ConnectionLEDMode, SetPIOTemp, Pin,
            PowerLevel, SleepType, ReliableAdvertizing, Renew, Reset, Role, RSSI, LastConnectedAddress, SensorWorkInterval, StopBits, StartWork, Sleep, SaveConnectedAddress, SensorType,
            DiscoveryParameter, TemperatureSensor, ICTemperature, RemoteDeviceTimeout, BondType, Service, WakeThroughUART, Version
        }


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
}
