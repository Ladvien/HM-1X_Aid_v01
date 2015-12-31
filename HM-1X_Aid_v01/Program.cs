using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO.Ports;
using HM_1X_Aid_v01;

namespace HM_1X_Aid_v01
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainDisplay());        
        }        
        
    }
}

class SerialPortsExtended: SerialPort
{

    SerialPort ComPort = new SerialPort();
    hm1xConstants hm1xConstants = new hm1xConstants();
    hm1xSettings hm1xSettings = new hm1xSettings();

    enum charsAfterRXTX : int { None = 0, LineFeed, CarriageReturn, CarriageReturnLineFeed, LineFeedCarriageReturn };
    
    // Open port flag.
    private bool portOpen = false;
    bool dataHandlerAttached = false;

    // HM-1X //////////////////////
    private string captureBuffer = "";
    private bool captureStream = false;

    Dictionary<string, hm1xConstants.hm1xEnumCommands> hm1xCommandsDict = new Dictionary<string, hm1xConstants.hm1xEnumCommands>();
    Dictionary<string, hm1xConstants.hm1xEnumCommands> hm1xCommandsExplainedDict = new Dictionary<string, hm1xConstants.hm1xEnumCommands>();

    private static System.Timers.Timer HM1Xtimer;
    hm1xConstants.hm1xEnumCommands waitingOn = 0;
    hm1xConstants.hm1xDeviceType hm1xModuleType = 0;
    
    // Device characteristics.
    private bool hm1xConnected = false;
    private int hm1xVersion = 0;

    // Response timeout
    int responseTimeout = 300;

    // HM-1X END //////////////////

    delegate void HM1XVariableUpdate(object sender, object originator, object value);

    // List containing all discovered COMs.
    List<String> portList = new List<String>();

    // Port setup.
    int readTimeout = 0;
    int writeTimeout = 0;

    // Callback and event handler for passing serial data to the main object.
    public delegate void DataReceivedCallback(object sender, string data);
    public event DataReceivedCallback DataReceivedEventHandler;

    // Callback and event handler for passing serial data to the main object.
    public delegate void HM1Xupdated(object sender, object originator, object value);
    public event HM1Xupdated HM1XupdatedEventHandler;

    // Callback and event handler for passing serial data to the main object.
    public delegate void SerialSystemUpdate(object sender, string text, int progressBarValue);
    public event SerialSystemUpdate SerialSystemUpdateEventHandler;

    // Received data buffer.
    private string InputData = string.Empty;

    // Settings AT suffix.
    List<string> atCommandSuffixList = new List<string>();

    public void initDictionary()
    {
        HM1XupdatedEventHandler += new SerialPortsExtended.HM1Xupdated(HM1XupdateValue);
        int index = 0;
        foreach (hm1xConstants.hm1xEnumCommands commands in Enum.GetValues(typeof(hm1xConstants.hm1xEnumCommands)))
        {
            hm1xCommandsDict.Add(hm1xConstants.hm1xCommandsString[index], commands);
            hm1xCommandsExplainedDict.Add(hm1xConstants.hm1xCommandsExplained[index], commands);
            index++;
        }
    }

    // Setters for the timeouts.
    public void setReadTimeout(int timeout)
    {
        ComPort.ReadTimeout = timeout;
        readTimeout = timeout;
    }

    public void setWriteTimeout(int timeout)
    {
        ComPort.WriteTimeout = timeout;
        writeTimeout = timeout;
    }

    public List<string> getPortsList()
    {
        // Updates the COMs port list and returns it.
        foreach (String portName in System.IO.Ports.SerialPort.GetPortNames())
        {
            portList.Add(portName);
        }
        return portList;
    }

    // Open port using string identifiers.
    public void openPort(string port, string baudRate, string dataBits, string stopBits, string parity, string handshaking)
    {
        SerialSystemUpdateHandler(this, "Trying port " + port + "\n", 0);
        // Open if port isn't and there is at least one port listed.
        if (portOpen == false && portList.Count > 0)
        {
            ComPort.PortName = Convert.ToString(port);
            ComPort.BaudRate = Convert.ToInt32(baudRate);
            ComPort.DataBits = Convert.ToInt16(dataBits);
            ComPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
            ComPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), handshaking);
            ComPort.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
            try
            {
                ComPort.Open();

                // Only create one data handler, otherwise to whom do we listen?
                if (!dataHandlerAttached)
                {
                    ComPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                    dataHandlerAttached = true;
                }

                SerialSystemUpdateHandler(this, ("Opened port " + port + "\r\n"), 100);
            }
            catch (UnauthorizedAccessException ex)
            {
                SerialSystemUpdateHandler(this, "Failed to open port " + port + "\r\n", 0);
                MessageBox.Show(ex.Message);
            }
            portOpen = true;
        }
        else if (portOpen == false)
        {
            closePort();
        }
    }

    public void closePort()
    {
        SerialSystemUpdateHandler(this, "Closing all ports.\r\n", 0);
        try
        {
            ComPort.Close();
            portOpen = false;
            SerialSystemUpdateHandler(this, "Closed all ports.\r\n", 100);
        }
        catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message);
            SerialSystemUpdateHandler(this, "Failed to close port(s).\r\n", 100);
        }
    }

    public bool isPortOpen()
    {
        // Well, is it?
        return portOpen;
    }


    // Helper methods.
    public void AddBaudRatesToComboBox(ComboBox comboBox, int defaultIndex)
    {
        // Fills a referenced combobox with common baud rates.
        comboBox.Items.Add(300);
        comboBox.Items.Add(600);
        comboBox.Items.Add(1200);
        comboBox.Items.Add(2400);
        comboBox.Items.Add(9600);
        comboBox.Items.Add(14400);
        comboBox.Items.Add(19200);
        comboBox.Items.Add(38400);
        comboBox.Items.Add(57600);
        comboBox.Items.Add(115200);
        comboBox.Items.Add(230400);

        if (defaultIndex > comboBox.Items.Count)
        {
            defaultIndex = comboBox.Items.Count;
        } else if (defaultIndex < 0)
        {
            defaultIndex = 0;
        }
        // Default to 
        comboBox.SelectedIndex = defaultIndex;
    }

    public void AddDataBitsToComboBox(ComboBox comboBox, int defaultIndex)
    {
        // Fills a referenced combobox with data bits settings.
        comboBox.Items.Add(6);
        comboBox.Items.Add(7);
        comboBox.Items.Add(8);
        comboBox.Items.Add(9);
        comboBox.Items.ToString();
        if (defaultIndex > comboBox.Items.Count)
        {
            defaultIndex = comboBox.Items.Count;
        }
        else if (defaultIndex < 0)
        {
            defaultIndex = 0;
        }
        // Default to 
        comboBox.SelectedIndex = defaultIndex;
    }

    public void AddStopBitsToComboBox(ComboBox comboBox, int defaultIndex)
    {
        // Fills a referenced combobox with stop bits settings.
        comboBox.Items.Add(1);
        comboBox.Items.Add(1.5);
        comboBox.Items.Add(2);
        comboBox.Items.ToString();
        if (defaultIndex > comboBox.Items.Count)
        {
            defaultIndex = comboBox.Items.Count;
        }
        else if (defaultIndex < 0)
        {
            defaultIndex = 0;
        }
        // Default to 
        comboBox.SelectedIndex = defaultIndex;
    }

    public void AddParityToComboBox(ComboBox comboBox, int defaultIndex)
    {
        // Fills a referenced combobox with parity settings.
        comboBox.Items.Add("None");
        comboBox.Items.Add("Odd");
        comboBox.Items.Add("Even");
        comboBox.Items.Add("Mark");
        comboBox.Items.Add("Space");
        if (defaultIndex > comboBox.Items.Count)
        {
            defaultIndex = comboBox.Items.Count;
        }
        else if (defaultIndex < 0)
        {
            defaultIndex = 0;
        }
        // Default to 
        comboBox.SelectedIndex = defaultIndex;
    }

    public void AddHandshakingToComboBox(ComboBox comboBox, int defaultIndex)
    {
        // Fills a referenced combobox with Handshaking settings.
        comboBox.Items.Add("None");
        comboBox.Items.Add("XOnXOff");
        comboBox.Items.Add("RequestToSend");
        comboBox.Items.Add("RequestToSendXOnXOff");
        if (defaultIndex > comboBox.Items.Count)
        {
            defaultIndex = comboBox.Items.Count;
        }
        else if (defaultIndex < 0)
        {
            defaultIndex = 0;
        }
        // Default to 
        comboBox.SelectedIndex = defaultIndex;
    }

    public void updateConnectionLabel(Label connectionLabel)
    {
        // Updates a referenced label with current port status and 
        // corresponding colors.
        if (portOpen == false)
        {
            connectionLabel.Text = "Disconnected";
            connectionLabel.BackColor = Color.Red;
        }
        else if (portOpen == true)
        {
            connectionLabel.Text = "Connected";
            connectionLabel.BackColor = Color.LimeGreen;
        }
    }

    // Read Data.
    private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
    {
        // Gets incoming data from open port and passes it to a handler.
        InputData = ComPort.ReadExisting();
        Console.WriteLine("RXed: {0}", InputData);
        
        // If not empty and we want to foreit capture to delegate.
        if (InputData != String.Empty && captureStream == false)
        {
            try
            {
                this.DataReceivedEventHandler(this, InputData);
            }
            catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message); }
        } else if (InputData != String.Empty && captureStream == true)
        {
            captureBuffer = InputData;
            Console.WriteLine("Debug1: {0}", captureBuffer);
            HM1XupdatedHandler(this, waitingOn, captureBuffer);
            captureBuffer = "";
        }
        else
        {
            // We retain capture. 
            captureBuffer = InputData;
        }
    }



    private void SerialSystemUpdateHandler(object sender, string text, int progressBarValue)
    {
        try
        {
            this.SerialSystemUpdateEventHandler(this, text, progressBarValue);
        } catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message); }
    }

    //Write Data
    public void WriteData(string dataToWrite)
    {
        try
        {
            if (isPortOpen())
            {
                ComPort.Write(dataToWrite);
                Console.WriteLine("Data Sent: {0}", dataToWrite);
            } else
            {
                MessageBox.Show(null, "No open port.", "Port error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message); }

    }

    // Gets are single character from InputData buffer, removing it from the buffer.
    public char getChar()
    {
        char returnChar = '\0';
        try
        {
            returnChar = InputData[0];
            InputData.Remove(1, 1);
        }
        catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message); }
        return returnChar;
    }

    // Mutate data
    public string convertASCIIStringToHexString(string text)
    {
        string result = "";
        // Replace the string "NULL" with an actual ASCII null.
        text = text.Replace("NULL", "\0");
        try
        {
            char[] values = text.ToCharArray();
            foreach (char letter in values)
            {
                // Get the integral value of the character.
                int value = Convert.ToInt32(letter);
                // Convert the decimal value to a hexadecimal value in string form.
                string hexOutput = string.Format("0x{0:X2} ", value);
                result += hexOutput;
            }

        }
        catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message); }
        return result;
    }

    public string convertASCIIStringToDecimalString(string text)
    {
        string result = "";
        try
        {
            char[] values = text.ToCharArray();
            // Convert each Char value to a Decimal.
            foreach (var value in values)
            {
                decimal decValue = value;
                string decimalOutput = string.Format("{0} ", decValue);
                result += decimalOutput;
            }
        }
        catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message); }

        return result;
    }

    public string convertHexStringToASCIIHex(string text)
    {
        string result = "";
        string resultString = "";
        // Checked for null.  Good.
        try
        {
            result = text.Replace(" ", "");
            result = result.Replace("0x", "");

            byte[] byteArray = StringToByteArrayFastest(result);

            int index = 0;
            while (index < byteArray.Length)
            {
                if (byteArray[index] == 0x00)
                {
                    resultString += "NULL";
                    index++;
                }
                else
                {
                    resultString += System.Text.Encoding.Default.GetString(byteArray, index, 1);
                    index++;
                }
            }
        }
        catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message); }
        return resultString;
    }

    public bool isValidHexString(string text)
    {
        text = text.Replace(" ", "");
        text = text.Replace("0x", "");

        if (text.Count() % 2 != 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public byte[] handleOddByteArray(byte[] byteArray)
    {
        if (byteArray.Count() % 2 == 1)
        {
            int newArrayLength = byteArray.Length - 1;
            byte[] tempByteArray = new byte[newArrayLength];
            Array.Copy(byteArray, tempByteArray, newArrayLength);
            return tempByteArray;
        } else
        {
            return byteArray;
        }
    }

    public static byte[] StringToByteArrayFastest(string hex)
    {
        // Checked for null.  Good.
        byte[] arr = new byte[hex.Length >> 1];
        try
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                var bfr = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
                if (bfr == 0)
                {
                    arr[i] = 0x00;
                } else
                {
                    arr[i] = bfr;
                }
            }

        }
        catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message); }

        return arr;
    }

    public static int GetHexVal(char hex)
    {
        int val = (int)hex;
        //Or the two combined, but a bit slower:
        return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
    }

    public string getSelectedEndChars(int caseNumber)
    {
        string result = "";

        // Add user's selected character after RX.
        switch (caseNumber)
        {
            case (int)charsAfterRXTX.None:
                break;
            case (int)charsAfterRXTX.LineFeed:
                result += "\n";
                break;
            case (int)charsAfterRXTX.CarriageReturn:
                result += "\r";
                break;
            case (int)charsAfterRXTX.LineFeedCarriageReturn:
                result += "\n\r";
                break;
            case (int)charsAfterRXTX.CarriageReturnLineFeed:
                result += "\r\n";
                break;
        }

        return result;
    }

    private bool canConvertASCIIToInt(string text)
    {
        char[] byteArray = text.ToCharArray();
        foreach (char letter in byteArray)
        {
            if (letter > '/' && letter < ':') {; } else { return false; }
        }
        return true;
    }

    ///// HM-1X /////////////////////////////////////////////////////////////////////////////////////

    // 1. Populate combo boxes.
    // 2. Capture stream.
    // 3. Build the command dynamically.
    // 4. Set onWaiting to what command is being waited on.
    // 5. Set response timeout timer.
    // 6. When HM1X sends response analyze it for validity.
    //    OR when timer expires, decide if we got a valid response.S
    // 7. If no valid response was obtained, send last command again.
    // 8. Repeat 7. until attempt thresehold or valid response is received.
    // 9. Set appropriate variable based upon response.
    // 10. Release stream.

    public void sendCommandToHM1X(ComboBox commandComboBox, ComboBox settingsComboBox, TextBox parameterOne, 
        TextBox parameterTwo, TextBox sysLogTextBox, ProgressBar progressBar, Button confirmButton)
    {
        captureStream = true;

        string finalCommand = "";
        string command = "";
        string settings = "";
        string parameterOneStr = "";
        string parameterTwoStr = "";

        SerialSystemUpdateHandler(this, "Executing " + commandComboBox.Text + ":" + settingsComboBox.Text + "\r\n", 50);



        hm1xConstants.hm1xEnumCommands commandComboBoxSelectedIndexAsEnum = (hm1xConstants.hm1xEnumCommands)commandComboBox.SelectedIndex;
        waitingOn = commandComboBoxSelectedIndexAsEnum;
        hm1xSettings.adjustParametersAndOtherSettings(commandComboBoxSelectedIndexAsEnum, settingsComboBox.SelectedIndex, parameterOne, parameterTwo);

        if (commandComboBox.Items.Count > 0)
        {
            command = hm1xConstants.hm1xCommandsString[commandComboBox.SelectedIndex];
        }
        if (atCommandSuffixList.Count > 0)
        {
            settings = atCommandSuffixList[settingsComboBox.SelectedIndex];
        }
        if (parameterOne.Text != "")
        {
            parameterOneStr = parameterOne.Text;
        }
        if (parameterTwo.Text != "")
        {
           parameterTwoStr = parameterTwo.Text;
        }

        if (command != "")
        {
            finalCommand += command;
        }
        if (settings != "")
        {
            finalCommand += settings;
        }
        if (parameterOneStr != "")
        {
            finalCommand += parameterOneStr;
        }
        if (parameterTwoStr != "")
        {
            finalCommand += parameterTwoStr;
        }

        responseTimeout = hm1xSettings.getResponseTimeNeeded(commandComboBoxSelectedIndexAsEnum);
        HM1XCallbackSetTimer(responseTimeout); // Wait half a second for reply.

        if (finalCommand != "")
        {
            WriteData(finalCommand);
        }

        parameterOne.Text = "";
        parameterTwo.Text = "";
        SerialSystemUpdateHandler(this, "", 50);
    }

    public void setResponseTimeout(int responseTime)
    {
        responseTimeout = responseTime;
    }

    private void HM1XCallbackSetTimer(int milliseconds)
    {

        Console.WriteLine("Set timer for {0}ms", milliseconds);

        // Create a timer with a two second interval.
        HM1Xtimer = new System.Timers.Timer(milliseconds);
        // Hook up the Elapsed event for the timer. 
        HM1Xtimer.Elapsed += hm1xCommandTimedCallback;
        HM1Xtimer.AutoReset = false;
        HM1Xtimer.Enabled = true;
    }

    private void hm1xCommandTimedCallback(Object source, EventArgs e)
    {
        if (captureBuffer == "")
        { 
            waitingOn = hm1xConstants.hm1xEnumCommands.ERROR;
        }
        HM1XupdatedHandler(this, waitingOn, captureBuffer);
        waitingOn = hm1xConstants.hm1xEnumCommands.None;
        captureBuffer = "";
    }

    public void clearCaptureFlag()
    {
        captureStream = false;
        captureBuffer = "";
    }

    // Read Data.
    private void HM1XupdatedHandler(object sender, object originator, object value)
    {
        try
        {
            this.HM1XupdatedEventHandler(this, originator, value);
        } 
        catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message); }
    }

    private void HM1XupdateValue(object sender, object originator, object value)
    {
        hm1xConstants.hm1xEnumCommands switchValue = (hm1xConstants.hm1xEnumCommands)originator;
        switch (switchValue)
        {
            case hm1xConstants.hm1xEnumCommands.CheckStatus:
                setHM1XConnection();
                break;
            case hm1xConstants.hm1xEnumCommands.Version:
                break;
            case hm1xConstants.hm1xEnumCommands.ADC:
                break;
            case hm1xConstants.hm1xEnumCommands.MACAddress:
                break;
            case hm1xConstants.hm1xEnumCommands.AdvertizingInterval:
                break;
            case hm1xConstants.hm1xEnumCommands.LastConnectedAddress:
                break;
        }
    }

    public void updateUIAfterDataRX(object sender, object originator, object value, RichTextBox mainDisplay, TextBox sysTextBox, ProgressBar progressBar, Label label)
    {
        // Response builder.
        string getOrSet = "";
        String valueString = (string)value;
        int replySwitch = 0;
        byte replyByte = 0x00;
        UInt16 endianCorrectedByte = 0x00;
        bool isSet = false;
        string[] pinInfo = { "Pin B", "Pin A", "Pin 9", "Pin 8", "Pin 7", "Pin 6", "Pin 5", "Pin 4", "Pin 3", "Pin 2", "Pin 1 (NA)", "Pin 0 (NA)" };
        bool errorFlag = false;
        string macAddress = "";
        progressBar.BackColor = Color.LimeGreen;
        progressBar.Value = 50;

        // Must invalidate timer or updateUIAfterDataRX will fire again after it fired due to RX.
        HM1Xtimer.Enabled = false;

        hm1xConstants.hm1xEnumCommands switchValue = (hm1xConstants.hm1xEnumCommands)originator;

        sysTextBox.Text += "Got " + hm1xConstants.getCommandStringFromEnum(switchValue) + "\r\n";

        Console.WriteLine("UI Update Switch: {0}", switchValue.ToString());

        switch (switchValue)
        {
            case hm1xConstants.hm1xEnumCommands.None:
                errorFlag = true;
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.CheckStatus:
                label.Text = "Connected";
                label.BackColor = Color.LimeGreen;
                progressBar.BackColor = Color.LimeGreen;
                mainDisplay.Text += "Connected\r\n";
                setHM1XConnection();
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.Version:
                mainDisplay.Text += "Firmware version: " + value.ToString() + "\r\n";
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.ADC:
                valueString = valueString.Replace("OK+ADC", "Pin IO #");
                valueString += "\r\n";
                mainDisplay.Text += valueString;
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.MACAddress:
                valueString = valueString.Replace("OK+ADDR:", "MAC Address: ");
                valueString += "\r\n";
                mainDisplay.Text += valueString;
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.AdvertizingInterval:
                if (valueString.Contains("OK+Get:"))
                {
                    valueString = valueString.Replace("OK+Get:", "Got Advertizing interval: ");
                } else
                {
                    valueString = valueString.Replace("OK+Set:", "Set advertizing interval: ");
                }
                valueString += "\r\n";
                mainDisplay.Text += valueString;
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.AdvertizingType:
                
                if (valueString.Contains("OK+Get:"))
                {
                    valueString = valueString.Replace("OK+Get:", "");
                    getOrSet = "Got response: ";
                }
                else if(valueString.Contains("OK+Set:"))
                {
                    valueString = valueString.Replace("OK+Set:", "");
                    getOrSet = "Set to: ";
                }

                try { replySwitch = Convert.ToInt32(valueString); } catch {; }
                valueString = getOrSet;
                switch (replySwitch)
                {
                    case 0:
                        valueString += "Advertising, Scan-Response, Connectable";
                        break;
                    case 1:
                        valueString += "Only permit last device within 1.28 seconds";
                        break;
                    case 2:
                        valueString += "Allow advertizing and Scan-Response";
                        break;
                    case 3:
                        valueString += "Only allow advertizing";
                        break;
                }
                valueString += "\r\n";
                mainDisplay.Text += valueString;
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.ANCS:

                isItGetOrSet(valueString, out getOrSet, out replySwitch,out isSet);

                valueString = getOrSet;

                switch (replySwitch)
                {
                    case 0:
                        valueString += " ANCS is Off";
                        break;
                    case 1:
                        valueString += " ANCS is On  -- the module must be reset for this to go into effect \r\n";
                        valueString += "and the bond mode must be set to \"Authorize and Bond.\"";
                        break;
                }
                valueString += "\r\n";
                mainDisplay.Text += valueString;
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.LastConnectedAddress:

                if (valueString.Contains("OK+RADD:"))
                {
                    valueString = valueString.Replace("OK+RADD:", "Got MAC address of last connected device: ");
                } else {
                    // Error
                }  
                valueString += "\r\n";
                mainDisplay.Text += valueString;
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.WhiteListSwitch:

                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);

                valueString = getOrSet;

                switch (replySwitch)
                {
                    case 0:
                        valueString += " Whitelist Switch is OFF";
                        break;
                    case 1:
                        valueString += " Whitelist Switch is ON";
                        break;
                }
                valueString += "\r\n";
                mainDisplay.Text += valueString;
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.WhitelistMACAddress:
                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);

                if (valueString.Contains("OK+AD1?:"))
                {
                    macAddress = "Whitelist MAC 1: " + valueString.Replace("OK+AD1?:", "");
                } else if (valueString.Contains("OK+AD2?:"))
                {
                    macAddress = "Whitelist MAC 2: " + valueString.Replace("OK+AD2?:", "");
                } else if (valueString.Contains("OK+AD3?:"))
                {
                    macAddress = "Whitelist MAC 3: " + valueString.Replace("OK+AD3?:", "");
                }
                else if (valueString.Contains("OK+AD1"))
                {
                    macAddress = "Set Whitelist MAC Address 1 to: " + valueString.Replace("OK+AD1", "");
                }
                else if (valueString.Contains("OK+AD2"))
                {
                    macAddress = "Set Whitelist MAC Address 2 to: " + valueString.Replace("OK+AD2", "");
                }
                else if (valueString.Contains("OK+AD3"))
                {
                    macAddress = "Set Whitelist MAC Address 3 to: " + valueString.Replace("OK+AD3", "");
                }

                valueString = macAddress;
                valueString += "\r\n";
                mainDisplay.Text += valueString;
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.PIOStateAfterPowerOn:
                isItGetOrSetSAndByte(valueString, out getOrSet, out endianCorrectedByte, out isSet);

                if (isSet)
                {
                    //char[] hexCharArray = valueString.ToCharArray();
                    string addedLeadZero = "0";
                    addedLeadZero += valueString.Replace("OK+Set:", "");
                    int hexToInt = Convert.ToInt32(addedLeadZero, 16);

                    for (int i = 11; i > -1; i--)
                    {
                        mainDisplay.Text += pinInfo[i];
                        if (IsBitSet((byte)endianCorrectedByte, i))
                        {
                            mainDisplay.Text += " was set HIGH.\r\n";
                        }
                        else
                        {
                            mainDisplay.Text += " was set LOW.\r\n";
                        }
                    }

                } else
                {
                    for (int i = 11; i > -1; i--)
                    {
                        mainDisplay.Text += pinInfo[i];
                        if (IsBitSet((byte)endianCorrectedByte, i))
                        {
                            mainDisplay.Text += " after power on is set to HIGH.\r\n";   
                        }
                        else
                        {
                            mainDisplay.Text += " after power on is set to LOW.\r\n";
                        }
                    }
                }
                valueString += "\n";
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.PIOStateAfterConnection:
                isItGetOrSetSAndByte(valueString, out getOrSet, out endianCorrectedByte, out isSet);

                if (isSet)
                {
                    //char[] hexCharArray = valueString.ToCharArray();
                    string addedLeadZero = "0";
                    addedLeadZero += valueString.Replace("OK+Set:", "");
                    int hexToInt = Convert.ToInt32(addedLeadZero, 16);

                    for (int i = 11; i > -1; i--)
                    {
                        mainDisplay.Text += pinInfo[i];
                        if (IsBitSet((byte)endianCorrectedByte, i))
                        {
                            mainDisplay.Text += " was set HIGH.\r\n";
                        }
                        else
                        {
                            mainDisplay.Text += " was set LOW.\r\n";
                        }
                    }

                }
                else
                {
                    for (int i = 11; i > -1; i--)
                    {
                        mainDisplay.Text += pinInfo[i];
                        if (IsBitSet((byte)endianCorrectedByte, i))
                        {
                            mainDisplay.Text += " after connected is set to HIGH.\r\n";
                        }
                        else
                        {
                            mainDisplay.Text += " after connected on is set to LOW. \r\n";
                        }
                    }
                }
                valueString += "\r\n";
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.BatteryMonitor:
                
                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);

                switch (replySwitch)
                {
                    case 0:
                        mainDisplay.Text += "Battery monitor set OFF.\r\n";
                        break;
                    case 1:
                        mainDisplay.Text += "Battery monitor set ON.\r\n";
                        break;
                }
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.BatteryInformation:
                mainDisplay.Text += "Battery Power: %" + valueString.Replace("OK+Get:", "") + "\r\n";
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.BitFormat:
                
                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);

                switch (replySwitch)
                {
                    case 0:
                        mainDisplay.Text += "Set to 7-Bit NON-Compatible.\r\n";
                        break;
                    case 1:
                        mainDisplay.Text += "Set to 7-Bit Compatible.\r\n";
                        break;
                }
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.BaudRate:
               

                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);

                switch (replySwitch)
                {
                    case 0:
                        mainDisplay.Text += "Set to Baud 9600\r\n";
                        break;
                    case 1:
                        mainDisplay.Text += "Set to Baud 19200\r\n";
                        break;
                    case 2:
                        mainDisplay.Text += "Set to Baud 38400\r\n";
                        break;
                    case 3:
                        mainDisplay.Text += "Set to Baud 57600\r\n";
                        break;
                    case 4:
                        mainDisplay.Text += "Set to Baud 115200\r\n";
                        break;
                    case 5:
                        mainDisplay.Text += "Set to Baud 4800\r\n";
                        break;
                    case 6:
                        mainDisplay.Text += "Set to Baud 2400\r\n";
                        break;
                    case 7:
                        mainDisplay.Text += "Set to Baud 1200\r\n";
                        break;
                    case 8:
                        mainDisplay.Text += "Set to Baud 230400\r\n";
                        break;
                }
                if (isSet)
                {
                    mainDisplay.Text += "The module needs to be powered down before baud set is complete.\r\n";
                }
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.Characteristic:
                
                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);
                
                if(isSet)
                {
                    mainDisplay.Text += "Characteristic set to: " + valueString.Replace("OK+Set:", "") + "\r\n";
                } else
                {
                    mainDisplay.Text += "Your characteristic setting is currently: " + valueString.Replace("OK+Get:", "") + "\r\n";
                }
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.ClearLastConnected:
                if (valueString.Contains("OK+CLEAR"))
                {
                    valueString = valueString.Replace("OK+CONN", "");
                    mainDisplay.Text += "Last Connected Device Cleared.\r\n";
                }
                finishedCommand();
                break;
            case hm1xConstants.hm1xEnumCommands.TryLastConnected:

                if (valueString.Contains("OK+CONN"))
                {
                    string switchString = valueString.Replace("OK+CONN", "");
                    switch (switchString)
                    {
                        case "N":
                            mainDisplay.Text += "No address provided\r\n";
                            break;
                        case "L":
                            mainDisplay.Text += "Connecting\r\n";
                            break;
                        case "E":
                            mainDisplay.AppendText("Connection error\r\n", Color.Red);
                            errorFlag = true;
                            break;
                        case "F":
                            errorFlag = true;
                            mainDisplay.AppendText("Connect Fail\r\n", Color.Red);
                            break;
                    }
                }
                break;
            case hm1xConstants.hm1xEnumCommands.TryConnectionAddress:                

                if (valueString.Contains("OK+CONN"))
                {
                    string switchString = valueString.Replace("OK+CONN", "");
                    switch (switchString)
                    {
                        case "A":
                            mainDisplay.Text += "Accept request, connecting...\r\n";
                            break;
                        case "":
                            mainDisplay.Text += "Connected\r\n";
                            finishedCommand();
                            break;
                        case "E":
                            mainDisplay.AppendText("Connection error\r\n", Color.Red);
                            errorFlag = true;
                            finishedCommand();
                            break;
                        case "F":
                            mainDisplay.AppendText("Connect Fail\r\n", Color.Red);
                            finishedCommand();
                            break;
                    }
                }
                break;
            case hm1xConstants.hm1xEnumCommands.PIOState:
                if (valueString.Contains("OK+Col:"))
                {
                    valueString = valueString.Replace("OK+Col:", "");

                    byte[] byteArray = StringToByteArrayFastest(valueString);

                    replyByte = byteArray[0];
                    for(int i = 11; i != 0; i--)
                    {
                        // I probably need to come back and work on this bit.
                        // The actual register for PINs is 12-bits wide; I'm only listing 8 here.
                        mainDisplay.Text += pinInfo[i];
                        if(IsBitSet((byte)replyByte, i))
                        {
                            mainDisplay.Text += " HIGH\r\n";
                        } else
                        {
                            mainDisplay.Text += " LOW\r\n";
                        }
                    }
                }
                break;
            case hm1xConstants.hm1xEnumCommands.PIOCollectionRate:

                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);
                if (isSet)
                {
                    mainDisplay.Text += "Pin IO collection rate set to " + replySwitch + " seconds\r\n";
                }
                else
                {
                    mainDisplay.Text += "Pin IO collection is currently set to " + replySwitch + " seconds\r\n";
                }

                break;

            case hm1xConstants.hm1xEnumCommands.StartDiscovery:
                ///////////////////////////////////////////////////////////////////////////////////////////////
                /////////////////////////////// NOT COMPLETE //////////////////////////////////////////////////
                /////////////////////////////// MUST HANDLE DISCOVERY /////////////////////////////////////////

                if (valueString.Contains("OK+DISC"))
                {
                    valueString = valueString.Replace("OK+DISC", "");
                    switch (valueString)
                    {
                        case "S":
                            mainDisplay.Text += "Device search STARTED...\r\n";
                            break;
                        case "E":
                            mainDisplay.Text += "Device search ENDED...\r\n";
                            finishedCommand();
                            break;
                    }
                } else
                {
                    errorFlag = true;
                    finishedCommand();
                }
                ///////////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////////////////
                break;
            case hm1xConstants.hm1xEnumCommands.StartiBeaconDiscovery:
                ///////////////////////////////////////////////////////////////////////////////////////////////
                /////////////////////////////// NOT COMPLETE //////////////////////////////////////////////////
                
                Console.WriteLine("iBeacon found: {0}", valueString);
                break;
            case hm1xConstants.hm1xEnumCommands.ConnectToDiscoveredDevice:

                if (valueString.Contains("OK+CONN"))
                {
                    string switchString = valueString.Replace("OK+CONN", "");
                    switch (switchString)
                    {
                        case "N":
                            mainDisplay.Text += "No address provided\r\n";
                            break;
                        case "L":
                            mainDisplay.Text += "Connecting\r\n";
                            break;
                        case "E":
                            mainDisplay.AppendText("Connection error\r\n", Color.Red);
                            errorFlag = true;
                            break;
                        case "F":
                            errorFlag = true;
                            mainDisplay.AppendText("Connect Fail\r\n", Color.Red);
                            break;
                    }
                }
                break;
            case hm1xConstants.hm1xEnumCommands.iBeaconMode:

                //////////////// WILL NEED TO TEST AFTER UPGRADE ///////////////////////

                if (valueString.Contains("OK+DELO"))
                {
                    string switchString = valueString.Replace("OK+DELO", "");
                    switch (switchString)
                    {
                        case "1":
                            mainDisplay.Text += "Set to allow broadcast and scanning.\r\n";
                            break;
                        case "2":
                            mainDisplay.Text += "Set to only allow broadcast.\r\n";
                            break;
                    } 
                } else
                {
                    mainDisplay.AppendText("No response received.  Not unsual, switching iBeacon mode requires reset.  Resets are weird.");
                }
                break;

            case hm1xConstants.hm1xEnumCommands.RemoveBondInfo:
                if (valueString.Contains("OK+ERASE")) { mainDisplay.Text += "Successfully erased bond info";  }
                else { errorFlag = true; }
                break;
            case hm1xConstants.hm1xEnumCommands.AdvertizingFlag:
                 
                //////////////// WILL NEED TO TEST AFTER UPGRADE ///////////////////////

                break;
            case hm1xConstants.hm1xEnumCommands.FlowControlSwitch:

                //////////////// WILL NEED TO TEST AFTER UPGRADE ///////////////////////
                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);
                if (isSet)
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "Flow control switch was set to OFF.\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "Flow control switch was set to ON.\r\n";
                            break;
                    }
                } else
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "Flow control switch is currently set to OFF.\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "Flow control switch is currently set to ON.\r\n";
                            break;
                    }
                }
                break;
            case hm1xConstants.hm1xEnumCommands.RXGain:
                //////////////// WILL NEED TO TEST AFTER UPGRADE ///////////////////////
                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);
                if (isSet)
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "RX Gain was set to OFF\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "RX Gain was set to ON\r\n";
                            break;
                    }
                }
                else
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "RX Gain is currently set to OFF\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "RX Gain is currently set to ON\r\n";
                            break;
                    }
                }
                break;
            case hm1xConstants.hm1xEnumCommands.Help:
                if(valueString.Length > 0) { mainDisplay.Text += "Here is your 'help'--good luck with it, my friend: " + valueString; }
                break;
            case hm1xConstants.hm1xEnumCommands.WorkType:
                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);
                if (isSet)
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "Set Enter Serial Mode with Start Command\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "Set Enter Serial Mode Immediately\r\n";
                            break;
                    }
                }
                else
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "Module is currently set to Enter Serial Mode with Start Command\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "Module is currently set to Enter Serial Mode Immediately\r\n";
                            break;
                    }
                }
                break;
            
            /////////////////// IBEACON CRAP!!///////////////////

            case hm1xConstants.hm1xEnumCommands.WorkMode:
                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);
                if (isSet)
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "Set Module to Transmission Mode.\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "Set PIO Collection and Transmission Mode.\r\n";
                            break;
                        case 2:
                            mainDisplay.Text += "Set to Remote Control Mode.\r\n";
                            break;
                    }
                }
                else
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "Module is currently in Transmission Mode.\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "Module is currently in PIO Collection and Transmission Mode.\r\n";
                            break;
                        case 2:
                            mainDisplay.Text += "Module is currently in Remote Control Mode.\r\n";
                            break;
                    }
                }
                break;
            case hm1xConstants.hm1xEnumCommands.ConnectionNotification:
                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);
                if (isSet)
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "Set to NOT notify when connection is established.\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "Set notify when connection is established.\r\n";
                            break;
                    }
                }
                else
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "Module is currently set NOT to notify on connection.\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "Module is currently set to notify on connection.\r\n";
                            break;
                    }
                }
                break;
            case hm1xConstants.hm1xEnumCommands.ConnNotificationMode:
                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);
                if (isSet)
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "Set to notify WITHOUT address when connection is established.\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "Set to notify WITH address when connection is established.\r\n";
                            break;
                    }
                }
                else
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "Module is currently set notify WITHOUT address when connection is established.\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "Module is currently set to notify WITH address when connection is established..\r\n";
                            break;
                    }
                }
                break;
            case hm1xConstants.hm1xEnumCommands.Name:
                if (valueString.Contains("OK+NAME:")) { mainDisplay.Text += "Module's name: " + valueString.Replace("OK+NAME:", ""); }
                if (valueString.Contains("OK+Set:")) { mainDisplay.Text += "Module's name was set to: " + valueString.Replace("OK+Set:", ""); }
                break;
            case hm1xConstants.hm1xEnumCommands.OutputDriver:
                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);
                if (isSet)
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "Set Output Power Driver to NORMAL.\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "Set Output Power Driver to HIGH.\r\n";
                            break;
                    }
                }
                else
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "Output Power Driver is currently set to NORMAL.\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "Output Power Driver is currently set to HIGH.\r\n";
                            break;
                    }
                }
                break;
            case hm1xConstants.hm1xEnumCommands.Parity:
                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);
                if (isSet)
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "Set parity to NONE.\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "Set parity to EVEN.\r\n";
                            break;
                        case 2:
                            mainDisplay.Text += "Set parity to ODD.\r\n";
                            break;
                    }
                    mainDisplay.AppendText("Don't forget to change your serial connection settings! Oh my!", Color.Red);
                }
                else
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "Parity is currently set to NONE.\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "Parity is currently set to EVEN.\r\n";
                            break;
                        case 2:
                            mainDisplay.Text += "Parity is currently set to ODD.\r\n";
                            break;
                    }
                }
                break;
            case hm1xConstants.hm1xEnumCommands.ConnectionLEDMode:
                isItGetOrSet(valueString, out getOrSet, out replySwitch, out isSet);
                if (isSet)
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "Set LED to Blink 500ms when unconnected. High when connected.\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "Set LED to be LOW unconnected and HIGH when connected.\r\n";
                            break;
                    }
                }
                else
                {
                    switch (replySwitch)
                    {
                        case 0:
                            mainDisplay.Text += "LED currently will BLINK (500ms) when unconnected and is HIGH when connected.\r\n";
                            break;
                        case 1:
                            mainDisplay.Text += "LED currently will be LOW unconnected and HIGH when connected.\r\n";
                            break;
                    }
                }
                break;
            case hm1xConstants.hm1xEnumCommands.ERROR:
                errorFlag = true;
                break;
            default:
                errorFlag = true;
                break;

        }
        if (errorFlag)
        {
            sysTextBox.Text += "ERROR";
            mainDisplay.AppendText("ERROR\n", Color.Red);
            progressBar.Value = 100;
            finishedCommand();
        }
        else
        {
            progressBar.ForeColor = Color.LimeGreen;
            progressBar.Value = 100;
        }

        mainDisplay.SelectionStart = mainDisplay.Text.Length;
        mainDisplay.ScrollToCaret();
        sysTextBox.SelectionStart = mainDisplay.Text.Length;
        sysTextBox.ScrollToCaret();
    }

    public void finishedCommand()
    {
        waitingOn = hm1xConstants.hm1xEnumCommands.None;
        captureStream = false;
    }

    private void isItGetOrSet(string valueString, out string displayResponse, out int replySwitchInt, out bool isSet)
    {
        if (valueString.Contains("OK+Get:"))
        {
            valueString = valueString.Replace("OK+Get:", "");
            displayResponse = "Got response: ";
            isSet = false;
        }
        else if (valueString.Contains("OK+Set:"))
        {
            valueString = valueString.Replace("OK+Set:", "");
            displayResponse = "Set to: ";
            isSet = true;
        } else
        {
            isSet = false;
            displayResponse = "ERROR";
        }

        try {replySwitchInt = Convert.ToInt32(valueString); } catch { replySwitchInt = 0 ;}

    }

    private void isItGetOrSetSAndByte(string valueString, out string displayResponse, out UInt16 endianCorrectByte, out bool isSet)
    {
        if (valueString.Contains("OK+Get:"))
        {
            valueString = valueString.Replace("OK+Get:", "0");
            displayResponse = "Got response: ";
            isSet = false;
        }
        else if (valueString.Contains("OK+Set:"))
        {
            valueString = valueString.Replace("OK+Set:", "0");
            displayResponse = "Set to: ";
            isSet = true;
        }
        else
        {
            isSet = false;
            displayResponse = "ERROR ";
        }

        correctEndiannessOfByte(valueString, out endianCorrectByte);
    }

    public void correctEndiannessOfByte(string valueString, out UInt16 endianCorrectByte)
    {
        byte[] byteArray = StringToByteArrayFastest(valueString);
        if (BitConverter.IsLittleEndian) { Array.Reverse(byteArray); }
        try { endianCorrectByte = System.BitConverter.ToUInt16(byteArray, 0); } catch { endianCorrectByte = 0x00; }
    }

    bool IsBitSet(byte b, int pos)
    {
        return (b & (1 << pos)) != 0;
    }

    public UInt16 ReadMemory16(Byte[] memory, UInt16 address)
    {
        return System.BitConverter.ToUInt16(memory, address);
    }

    private int convertStringToIntForHM1XSwitch(string valueString)
    {
        int value = 0;
        try { value = Convert.ToInt32(valueString); } catch {; }
        return value;
    }

    public void setModuleType(int type)
    { 
        hm1xModuleType = (hm1xConstants.hm1xDeviceType)type; 
    }



    public void connectToHM1X()
    {
        SerialSystemUpdateHandler(this, "Connecting to HM-1X \r\n", 0);
        captureStream = true;
        WriteData("AT"); // Command to get version info.
        waitingOn = hm1xConstants.hm1xEnumCommands.CheckStatus;
        HM1XCallbackSetTimer(300); // Wait half a second for reply.
        SerialSystemUpdateHandler(this, "", 50);
    }

    private void setHM1XConnection()
    {
        if (captureBuffer.Contains("OK"))
        {
            hm1xConnected = true;
            //this.DataReceivedEventHandler(this, captureBuffer);
        }
        else
        {
            hm1xConnected = false;
        }
        captureStream = false;
    }

    public bool getHM1Xconnection()
    {
        return hm1xConnected;
    }

    // 1. Check if we have the version information, if so, pass it to the event handler
    // 2. If we don't have it, let's  capture serial stream.
    // 3. Send serial command then wait for response.
    // 4. When the response is received, let's parse it and send it to the event handler.
    // 5. Release serial stream.

    private void setVersion()
    {
        captureBuffer = captureBuffer.Replace("HMSoft V", ""); // Remove all but numbers for parsing.
        try
        {
            if (canConvertASCIIToInt(captureBuffer)) //  Check to make sure we got just numbers.
            {
                hm1xVersion = int.Parse(captureBuffer);
            }
        }
        catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message); }
        captureStream = false;  //
    }


    public void addHM1XCommandsToComboBox(ComboBox comboBox, int defaultIndex)
    {
        foreach(string commandString in hm1xConstants.hm1xCommandsExplained)
        {
            comboBox.Items.Add(commandString);
        }
        
        if (defaultIndex > comboBox.Items.Count)
        {
            defaultIndex = comboBox.Items.Count;
        }
        else if (defaultIndex < 0)
        {
            defaultIndex = 0;
        }
        // Default to 
        comboBox.SelectedIndex = defaultIndex;
    }

    public void addModuleTypesToComboBox(ComboBox comboBox, int defaultIndex)
    {
        comboBox.Items.Add("Unknown");
        comboBox.Items.Add("HM-10");
        comboBox.Items.Add("HM-11");
        comboBox.Items.Add("HM-15");
        comboBox.SelectedIndex = defaultIndex;
    }



    public void addHM1XSettingsToComboBox(ComboBox settingsComboBox, hm1xConstants.hm1xEnumCommands commandValue, TextBox parameterOne, 
        TextBox parameterTwo, Label parameterOneLbl, Label parameterTwoLbl, TextBox sysLogTextBox, Button confirmButton)
    {
        if (hm1xModuleType == hm1xConstants.hm1xDeviceType.Unknown)
        {
            sysLogTextBox.Text = "Please select module type.";
        }
        else
        {          
            // Fills a referenced combobox with HM1X settings.
            hm1xSettings.getSettingsHM10(settingsComboBox, atCommandSuffixList, commandValue, parameterOne, parameterTwo, parameterOneLbl, parameterTwoLbl, hm1xModuleType, confirmButton );   
        }

        // Default to 
        // comboBox.SelectedIndex = 0;
    }


    public void clearSettingsList()
    {
        atCommandSuffixList.Clear();
    }


    public void commandSelectedMessage(RichTextBox richTextBox, hm1xConstants.hm1xEnumCommands selectedEnumeration)
    {
        switch (selectedEnumeration)
        {
            case hm1xConstants.hm1xEnumCommands.TryLastConnected:
                richTextBox.Text += "This option only works if module is set to Central (ROLE0) and the Work Mode set to only connect when told (IMME1).\r\nThis command take will take 10 seconds for a response.\r\n";
                break;
            case hm1xConstants.hm1xEnumCommands.TryConnectionAddress:
                richTextBox.Text += "This option only works if module is set to Central (ROLE0) and the Work Mode set to only connect when told (IMME1).\r\nThis command take will take 10 seconds for a response.\r\nMay receive a reply:\r\n\t\tOK + CONNA ========= Accept request, connection \r\n \t\tOK + CONNE ========= Connect error \r\n \t\tOK + CONN   ========= Connected, if AT + NOTI1 is setup \r\n \t\tOK + CONNF ========= Connect Failed, After 10 seconds\r\n";
                break;
            case hm1xConstants.hm1xEnumCommands.StartDiscovery:
                richTextBox.AppendText("For firmware versions less than v539 only six devices can be discovered at a time.  After v539, it is unlimited.\r\n", Color.Red);
                break;
            case hm1xConstants.hm1xEnumCommands.ConnectToDiscoveredDevice:
                richTextBox.AppendText("NOTE: Devices must be discovered first.\r\n", Color.Red);
                break;
            case hm1xConstants.hm1xEnumCommands.iBeaconMode:
                richTextBox.AppendText("After execution, module will reset after 500ms\r\n", Color.Red);
                break;
            case hm1xConstants.hm1xEnumCommands.WorkType:
                richTextBox.AppendText("When this setting is on the module will only respond to AT Commands.\r\nThe module must receive \"AT+START\" before it will enter serial transmission mode.\r\n", Color.Red);
                break;
                 
        }
        richTextBox.SelectionStart = richTextBox.Text.Length;
        richTextBox.ScrollToCaret();
    }
    ///// HM-1X END/////////////////////////////////////////////////////////////////////////////////////


}

// Borrowed from http://stackoverflow.com/questions/1926264/color-different-parts-of-a-richtextbox-string
public static class RichTextBoxExtensions
{
    public static void AppendText(this RichTextBox box, string text, Color color)
    {
        box.SelectionStart = box.TextLength;
        box.SelectionLength = 0;

        box.SelectionColor = color;
        box.AppendText(text);
        box.SelectionColor = box.ForeColor;
    }
}