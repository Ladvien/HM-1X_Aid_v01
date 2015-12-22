using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO.Ports;

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

    enum charsAfterRXTX : int { None = 0, LineFeed, CarriageReturn, CarriageReturnLineFeed, LineFeedCarriageReturn };
    enum hm1xDeviceType : int { Unknown = 0, HM10 = 1, HM11 = 2, HM15 = 3};



    Dictionary<hm1xDeviceType, string> HM1XCommandEnumLookUp = new Dictionary<hm1xDeviceType, string>();

    // Open port flag.
    private bool portOpen = false;

    // HM-1X //////////////////////
    private string captureBuffer = "";
    private bool captureStream = false;

    string[] hm1xCommandsString = {"None", "AT", "AT+ADC", "AT+ADDR","AT+ADVI","AT+ADTY","AT+ANCS","AT+ALLO","AT+AD","AT+BEFC","AT+AFTC","AT+BATC","AT+BATT",
                                   "AT+BIT","AT+BAUD","AT+COMI","AT+COMA","AT+COLA","AT+COUP","AT+CHAR", "AT+CLEAR","AT+CONL","AT+CO","AT+COL",
                                   "AT+CYC", "AT+DISC", "AT+DISI", "AT+CONN", "AT+DELO", "AT+ERASE", "AT+FLAG", "AT+FILT", "AT+FIOW", "AT+GAIN",
                                   "AT+HELP", "AT+IMME", "AT+IBEA", "AT+BEA0", "AT+BEA1", "AT+BEA2", "AT+BEA3", "AT+MARJ", "AT+MINO", "AT+MEAS",
                                   "AT+MODE", "AT+NOTI", "AT+NOTP", "AT+NAME", "AT+PCTL", "AT+PARI", "AT+PIO", "AT+PASS", "AT+PIN", "AT+POWE",
                                   "AT+PWRM", "AT+RELI", "AT+RENEW", "AT+RESTART", "AT+ROLE", "AT+RSSI", "AT+RADD", "AT+RAT", "AT+STOP", "AT+START",
                                   "AT+SLEEP", "AT+SAVE", "AT+SENS", "AT+SHOW", "AT+TEHU", "AT+TEMP", "AT+TCON", "AT+TYPE", "AT+UUID", "AT+UART","AT+VERS" };

    public enum hm1xEnumCommands : int { None = 0, CheckStatus, ADC,  }

    private static System.Timers.Timer HM1Xtimer;
    hm1xEnumCommands waitingOn = 0;
    hm1xDeviceType hm1xModuleType = 0;


    // Device characteristics.
    private bool hm1xConnected = false;
    private int hm1xVersion = 0;


    // HM-1X END //////////////////

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
    public delegate void SerialSystemUpdate(object sender, string text, int progressBarValue, Color progressBarColor);
    public event SerialSystemUpdate SerialSystemUpdateEventHandler;

    // Received data buffer.
    private string InputData = string.Empty;

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
        
        SerialSystemUpdateHandler(this, "Trying port " + port + "\n", 0, Color.LimeGreen);
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
                ComPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                SerialSystemUpdateHandler(this, "Opened port " + port + "\n", 100, Color.LimeGreen);
            }
            catch (UnauthorizedAccessException ex)
            {
                SerialSystemUpdateHandler(this, "Failed to open port " + port + "\n", 0, Color.Crimson);
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
        SerialSystemUpdateHandler(this, "Closing all ports.\n", 0, Color.LimeGreen);
        try
        {
            ComPort.Close();
            portOpen = false;
            SerialSystemUpdateHandler(this, "Closing all ports.\n", 100, Color.LimeGreen);
        }
        catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message);
            SerialSystemUpdateHandler(this, "Failed to close port(s).\n", 100, Color.Crimson);
        }
    }

    public bool isPortOpen()
    {
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
        
        // If not empty and we want to foreit capture to delegate.
        if (InputData != String.Empty && captureStream == false)
        {
            try
            {
                this.DataReceivedEventHandler(this, InputData);
            }
            catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message); }
        } else
        {
            // We retain capture. 
            captureBuffer = InputData;
        }
    }



    private void SerialSystemUpdateHandler(object sender, string text, int progressBarValue, Color progressBarColor)
    {
        try
        {
            this.SerialSystemUpdateEventHandler(this, text, progressBarValue, progressBarColor);
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
                Console.WriteLine(dataToWrite);
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
    // 3. Send command to module.
    // 4. Set onWaiting to what command is being waited on.
    // 5. Set timer.
    // 6. When HM1X sends response analyze it for validity.
    //    OR when timer expires, decide if we got a valid response.
    // 7. If no valid response was obtained, sned last command.
    // 8. Repeat 7. until timeout or valid response.
    // 9. Set appropriate variable based upon response.
    // 10. Release stream.
    
    private void HM1XCallbackSetTimer(int milliseconds)
    {
        // Create a timer with a two second interval.
        HM1Xtimer = new System.Timers.Timer(milliseconds);
        // Hook up the Elapsed event for the timer. 
        HM1Xtimer.Elapsed += HM1XCommandCallback;
        HM1Xtimer.AutoReset = false;
        HM1Xtimer.Enabled = true;
    }

    private void HM1XCommandCallback(Object source, EventArgs e)
    {
        switch (waitingOn)
        {
            case hm1xEnumCommands.Version:
                setVersion();
                HM1XupdatedHandler(this, waitingOn, hm1xVersion);
                waitingOn = hm1xEnumCommands.None;
                break;
            case hm1xEnumCommands.Connected:
                setHM1XConnection();
                HM1XupdatedHandler(this, waitingOn, hm1xConnected);
                waitingOn = hm1xEnumCommands.None;
                break;
        }
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

    public void setModuleType(int type)
    { 
        hm1xModuleType = (hm1xDeviceType)type; 
    }


    public void sendCommandToHM1X(ComboBox commandComboBox, ComboBox settingsComboBox, TextBox sendTextBox, TextBox sysLogTextBox, ProgressBar progressBar)
    {    

        string finalCommand = "";
        string command = "";
        string settings = "";
        string textBox = "";

        if (commandComboBox.Items.Count > 0)
        {
            command = commandComboBox.SelectedItem.ToString();
        }
        if (settingsComboBox.Items.Count > 0)
        {
            settings = settingsComboBox.SelectedItem.ToString();
        }
        if (sendTextBox.Text != "")
        {
            textBox = sendTextBox.Text;
        }
        
        if(command != "")
        {
            finalCommand += command;
        }
        if(settings != "")
        {
            finalCommand += settings;
        }
        if(textBox != "")
        {
            finalCommand += textBox;
        }

        if(finalCommand != "")
        {
            Console.WriteLine(finalCommand);
            WriteData(finalCommand);
        }
    }

    public void connectToHM1X()
    {
        SerialSystemUpdateHandler(this, "Connecting to HM-1X\n", 0, Color.LimeGreen);
        captureStream = true;
        WriteData("AT"); // Command to get version info.
        waitingOn = hm1xEnumCommands.Connected;
        HM1XCallbackSetTimer(200); // Wait half a second for reply.
        SerialSystemUpdateHandler(this, "", 50, Color.LimeGreen);
    }

    private void setHM1XConnection()
    {
        if (captureBuffer.Contains("OK"))
        {
            hm1xConnected = true;
            captureStream = false;
            this.DataReceivedEventHandler(this, captureBuffer);
        }
        else
        {
            hm1xConnected = false;
            captureStream = false;
        }
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
    public int getVersion()
    {
        if(hm1xVersion < 1)
        {
            findVersion();
        } else
        {
            try
            {
                this.HM1XupdatedEventHandler(this, hm1xEnumCommands.Version, hm1xVersion);
            }
            catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message); }
        }
        return hm1xVersion;
    }
    
    //
    private void findVersion()
    {
        SerialSystemUpdateHandler(this, "Getting Version", 0, Color.LimeGreen);
        captureStream = true;
        WriteData("AT+VERS?"); // Command to get version info.
        waitingOn = hm1xEnumCommands.Version;
        HM1XCallbackSetTimer(250); // Wait half a second for reply.
        SerialSystemUpdateHandler(this, "Getting Version", 50, Color.LimeGreen);
    }

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
        // Fills a referenced combobox with data bits settings.
        comboBox.Items.Add("AT");
        comboBox.Items.Add("AT+ADC");
        comboBox.Items.Add("AT+ADDR");
        comboBox.Items.Add("AT+ADVI");
        comboBox.Items.Add("AT+ADTY");
        comboBox.Items.Add("AT+ANCS");
        comboBox.Items.Add("AT+ALLO");
        comboBox.Items.Add("AT+AD");
        comboBox.Items.Add("AT+BEFC");
        comboBox.Items.Add("AT+AFTC");
        comboBox.Items.Add("AT+BATC");
        comboBox.Items.Add("AT+BATT");
        comboBox.Items.Add("AT+BIT");
        comboBox.Items.Add("AT+BAUD");
        comboBox.Items.Add("AT+COMI");
        comboBox.Items.Add("AT+COMA");
        comboBox.Items.Add("AT+COLA");
        comboBox.Items.Add("AT+COUP");
        comboBox.Items.Add("AT+CHAR");
        comboBox.Items.Add("AT+CLEAR");
        comboBox.Items.Add("AT+CONL");
        comboBox.Items.Add("AT+CO");
        comboBox.Items.Add("AT+COL");
        comboBox.Items.Add("AT+CYC");
        comboBox.Items.Add("AT+DISC");
        comboBox.Items.Add("AT+DISI");
        comboBox.Items.Add("AT+CONN");
        comboBox.Items.Add("AT+DELO");
        comboBox.Items.Add("AT+ERASE");
        comboBox.Items.Add("AT+FLAG");
        comboBox.Items.Add("AT+FILT");
        comboBox.Items.Add("AT+FIOW");
        comboBox.Items.Add("AT+GAIN");
        comboBox.Items.Add("AT+HELP");
        comboBox.Items.Add("AT+IMME");
        comboBox.Items.Add("AT+IBEA");
        comboBox.Items.Add("AT+BEA0");
        comboBox.Items.Add("AT+BEA1");
        comboBox.Items.Add("AT+BEA2");
        comboBox.Items.Add("AT+BEA3");
        comboBox.Items.Add("AT+MARJ");
        comboBox.Items.Add("AT+MINO");
        comboBox.Items.Add("AT+MEAS");
        comboBox.Items.Add("AT+MODE");
        comboBox.Items.Add("AT+NOTI");
        comboBox.Items.Add("AT+NOTP");
        comboBox.Items.Add("AT+NAME");
        comboBox.Items.Add("AT+PCTL");
        comboBox.Items.Add("AT+PARI");
        comboBox.Items.Add("AT+PIO");
        comboBox.Items.Add("AT+PASS");
        comboBox.Items.Add("AT+PIN");
        comboBox.Items.Add("AT+POWE");
        comboBox.Items.Add("AT+PWRM");
        comboBox.Items.Add("AT+RELI");
        comboBox.Items.Add("AT+RENEW");
        comboBox.Items.Add("AT+RESTART");
        comboBox.Items.Add("AT+ROLE");
        comboBox.Items.Add("AT+RSSI");
        comboBox.Items.Add("AT+RADD");
        comboBox.Items.Add("AT+RAT");
        comboBox.Items.Add("AT+STOP");
        comboBox.Items.Add("AT+START");
        comboBox.Items.Add("AT+SLEEP");
        comboBox.Items.Add("AT+SAVE");
        comboBox.Items.Add("AT+SENS");
        comboBox.Items.Add("AT+SHOW");
        comboBox.Items.Add("AT+TEHU");
        comboBox.Items.Add("AT+TEMP");
        comboBox.Items.Add("AT+TCON");
        comboBox.Items.Add("AT+TYPE");
        comboBox.Items.Add("AT+UUID");
        comboBox.Items.Add("AT+UART");
        comboBox.Items.Add("AT+VERS");
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



    public void addHM1XSettingsToComboBox(ComboBox comboBox, string commandName, TextBox sysLogTextBox)
    {
        if (hm1xModuleType == hm1xDeviceType.Unknown)
        {
            sysLogTextBox.Text = "Please discover module type.";
        }
        else
        {
            // Fills a referenced combobox with HM1X settings.
            switch (commandName)
            {
                case "AT":
                    switch (hm1xModuleType)
                    {
                        case hm1xDeviceType.HM10:
                            break;
                        case hm1xDeviceType.HM11:
                            break;
                        case hm1xDeviceType.HM15:
                            break;
                    }
                    break;
                case "AT+VERS":
                    comboBox.Items.Add("?");
                    comboBox.Enabled = true;
                    comboBox.SelectedIndex = 0;
                    break;

            }
        }

        // Default to 
        // comboBox.SelectedIndex = 0;
    }
    ///// HM-1X END/////////////////////////////////////////////////////////////////////////////////////
}