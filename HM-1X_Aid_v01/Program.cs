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



    // Open port flag.
    private bool portOpen = false;

    // HM-1X //////////////////////
    private string captureBuffer = "";
    private bool captureStream = false;
    public enum hm1xCallbackSwitch : int { None = 0, Version = 1 }
    private static System.Timers.Timer HM1Xtimer;
    hm1xCallbackSwitch waitingOn = 0;

    // Device characteristics.
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
            }
            catch (UnauthorizedAccessException ex)
            {
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
        try
        {
            ComPort.Close();
            portOpen = false;
        }
        catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message); }
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

    // Read Data.
    private void HM1XupdatedHandler(object sender, object originator, object value)
    {
        try
        {
            this.HM1XupdatedEventHandler(this, originator, value);
        }
        catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message); }
    }

    //Write Data
    public void WriteData(string dataToWrite)
    {
        try
        {
            if (isPortOpen())
            {
                ComPort.Write(dataToWrite);
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
                this.HM1XupdatedEventHandler(this, hm1xCallbackSwitch.Version, hm1xVersion);
            }
            catch (UnauthorizedAccessException ex) { MessageBox.Show(ex.Message); }
        }
        return hm1xVersion;
    }
    
    //
    private void findVersion()
    {
        captureStream = true;
        WriteData("AT+VERS?"); // Command to get version info.
        waitingOn = hm1xCallbackSwitch.Version;
        HM1XCallbackSetTimer(500); // Wait half a second for reply.
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
        switch (waitingOn) {

            case hm1xCallbackSwitch.Version:
                setVersion();
                HM1XupdatedHandler(this, waitingOn, hm1xVersion);
                waitingOn = hm1xCallbackSwitch.None;
                break;
        }
    }

    ///// HM-1X END/////////////////////////////////////////////////////////////////////////////////////
}