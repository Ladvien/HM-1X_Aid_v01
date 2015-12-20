using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace HM_1X_Aid_v01
{
    public partial class MainDisplay : Form
    {
        enum displayableDataTypes: int {HEX, Decimal, String};
        enum charsAfterRXTX: int {None = 0, LineFeed, CarriageReturn, CarriageReturnLineFeed, LineFeedCarriageReturn};
        bool toggleSendBoxStringHex = true;

        // RX data callback delegate.
        delegate void SetTextCallback(string text);

        // Used to handoff data from ports thread to main thread.
        string tempBuffer = "";

        private SerialPortsExtended serialPorts = new SerialPortsExtended();

        public MainDisplay()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Prevents the GUI from getting illegible.
            this.MinimumSize = new System.Drawing.Size(800, 600);
           
            // Populate COM info.
            loadCOMInfo();
            
            // RX'ed data callback.
            serialPorts.DataReceivedEventHandler += new SerialPortsExtended.DataReceivedCallback(gotData);
            serialPorts.HM1XupdatedEventHandler += new SerialPortsExtended.HM1Xupdated(HM1XupdateValue);
            // Setup display.
            lblConnectionStatus.BackColor = Color.Red;
        }

        private void loadSettings()
        {
            if (cmbPortNumberInPortSettingsTab.Items.Count > 0)
            {
                cmbPortNumberInPortSettingsTab.SelectedIndex = Properties.Settings.Default.lastCom;
            }
            cmbBaudRatePortSettingsTab.SelectedIndex = Properties.Settings.Default.BaudRate;
            cmbDataBitsPortSettingsTab.SelectedIndex = Properties.Settings.Default.dataBits;
            cmbStopBitsPortSettingsTab.SelectedIndex = Properties.Settings.Default.stopBits;
            cmbParityPortSettingsTab.SelectedIndex = Properties.Settings.Default.parity;
            cmbHandshakingPortSettingsTab.SelectedIndex = Properties.Settings.Default.handshaking;
            cmbDataAs.SelectedIndex = Properties.Settings.Default.displayDataSettings;
            cmbCharAfterRx.SelectedIndex = Properties.Settings.Default.charsAfterRX;
        }

        private void saveSettings()
        {

            Properties.Settings.Default.lastCom = cmbPortNumberInPortSettingsTab.SelectedIndex;
            Properties.Settings.Default.BaudRate = cmbBaudRatePortSettingsTab.SelectedIndex;
            Properties.Settings.Default.dataBits = cmbDataBitsPortSettingsTab.SelectedIndex;
            Properties.Settings.Default.stopBits = cmbStopBitsPortSettingsTab.SelectedIndex;
            Properties.Settings.Default.parity = cmbParityPortSettingsTab.SelectedIndex;
            Properties.Settings.Default.handshaking = cmbHandshakingPortSettingsTab.SelectedIndex;
            Properties.Settings.Default.displayDataSettings = cmbDataAs.SelectedIndex;
            Properties.Settings.Default.charsAfterRX = cmbCharAfterRx.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        // RX event handler triggered by SerialPort.  Hands off data quick.  
        // If you try to update UI from this method, threads tangel.
        public void gotData(object sender, string data)
        {
            // Incoming data is on another thread UI cannot be updated without crashing.
            tempBuffer = data;
            this.BeginInvoke(new SetTextCallback(SetText), new object[] { tempBuffer });
        }

        public void HM1XupdateValue(object sender, object originator, object value)
        {
            SerialPortsExtended.hm1xCallbackSwitch switchValue = (SerialPortsExtended.hm1xCallbackSwitch)originator;
            switch (switchValue)
            {
                case SerialPortsExtended.hm1xCallbackSwitch.Version:
                    //string valueAsString = Convert.ToInt32(value).ToString();
                    tempBuffer = value.ToString();
                    // UI is on another thread.
                    this.BeginInvoke(new SetTextCallback(SetText), new object[] { tempBuffer });
                    // Console.WriteLine(value.ToString());
                    break;
            }
        }

        // This is the callback method run each time the tempBuffer changes.
        public void SetText(string text)
        {
            string result = "";
            // Display the RX'ed data in user selected fashion.
            switch (cmbDataAs.SelectedIndex)
            {
                // HEX.
                case 0:
                    result = serialPorts.convertASCIIStringToHexString(text);
                    break;
                // String.
                case 1:
                    result = text;
                    break;
                // Decimal.
                case 2:
                    result = serialPorts.convertASCIIStringToDecimalString(text);
                    break;
                // Defaults to string.
                default:
                    //result = text;
                    break;
            }

            // Add user's selected character after RX.
            result += serialPorts.getSelectedEndChars(cmbCharAfterRx.SelectedIndex);

            addDisplayText(result);
        }

        private void loadCOMInfo()
        {
            clearMainDisplay();
            clearSerialPortMenu();
            
            cmbDataAs.Items.Add(displayableDataTypes.HEX.ToString());
            cmbDataAs.Items.Add(displayableDataTypes.String.ToString());
            cmbDataAs.Items.Add(displayableDataTypes.Decimal.ToString());
            cmbDataAs.SelectedIndex = 1;

            cmbCharAfterRx.Items.Add("None");
            cmbCharAfterRx.Items.Add("LF");
            cmbCharAfterRx.Items.Add("CR");
            cmbCharAfterRx.Items.Add("LF CR");
            cmbCharAfterRx.Items.Add("CR LF");
            cmbCharAfterRx.SelectedIndex = 1;

            cmbCharsAfterTx.Items.Add("None");
            cmbCharsAfterTx.Items.Add("LF");
            cmbCharsAfterTx.Items.Add("CR");
            cmbCharsAfterTx.Items.Add("LF CR");
            cmbCharsAfterTx.Items.Add("CR LF");
            cmbCharsAfterTx.SelectedIndex = 1;

            // Get a list of COM ports.
            List<string> portList = serialPorts.getPortsList();
            portList.ForEach(delegate (String portInfo)
            {
                // Add discoverd ports to menus.
                ToolStripItem subItem = new ToolStripMenuItem(portInfo, null, new EventHandler(subMenuItem_Click));
                serialPortsToolStripMenuItem.DropDownItems.Add(subItem);
                cmbPortNumberInPortSettingsTab.Items.Add(portInfo);
            });
            if (portList.Count > 0)
            {
                togglePortMenu(true);
                
                // Set the default COM port.
                cmbPortNumberInPortSettingsTab.SelectedIndex = 0;

                // Populate the setting boxes.
                serialPorts.AddBaudRatesToComboBox(cmbBaudRatePortSettingsTab, 4);
                serialPorts.AddDataBitsToComboBox(cmbDataBitsPortSettingsTab, 2);
                serialPorts.AddStopBitsToComboBox(cmbStopBitsPortSettingsTab, 0);
                serialPorts.AddParityToComboBox(cmbParityPortSettingsTab, 0);
                serialPorts.AddHandshakingToComboBox(cmbHandshakingPortSettingsTab, 0);
                loadSettings();
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {       
            
        }
        
        private void exitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            saveSettings();
            serialPorts.closePort();
            System.Environment.Exit(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void subMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
                int index = (item.OwnerItem as ToolStripMenuItem).DropDownItems.IndexOf(item);
                cmbPortNumberInPortSettingsTab.SelectedIndex = index;
            }
        }

        private void togglePortMenu(bool offOrOn)
        {
            cmbPortNumberInPortSettingsTab.Enabled = offOrOn;
            cmbBaudRatePortSettingsTab.Enabled = offOrOn;
            cmbDataBitsPortSettingsTab.Enabled = offOrOn;
            cmbStopBitsPortSettingsTab.Enabled = offOrOn;
            cmbParityPortSettingsTab.Enabled = offOrOn;
            cmbHandshakingPortSettingsTab.Enabled = offOrOn;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        // My methods.
        private void clearMainDisplay()
        {
            rtbMainDisplay.Text = "";
        }

        private void setDisplayText(string text)
        {
            rtbMainDisplay.Text = text;
            //txbSysMsg.ScrollToCaret();
        }

        private void addDisplayText(string text)
        {
            rtbMainDisplay.AppendText(text);
            //mainDisplayTextBox.ScrollToCaret();
        }

        private void addSysMsgText(string text)
        {
            txbSysMsg.AppendText(text);
            txbSysMsg.ScrollToCaret();
        }

        private void setSysMsgText(string text)
        {
            txbSysMsg.Text = text;
            txbSysMsg.ScrollToCaret();
        }


        private void clearSerialPortMenu()
        {
            serialPortsToolStripMenuItem.DropDownItems.Clear();
        }

        private void defaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addDisplayText(sender.ToString());
        }

        private void tableLayoutPanel1_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (serialPorts.isPortOpen() == false)
            {
                serialPorts.openPort(cmbPortNumberInPortSettingsTab.Text,
                    cmbBaudRatePortSettingsTab.Text,
                    cmbDataBitsPortSettingsTab.Text,
                    cmbStopBitsPortSettingsTab.Text,
                    cmbParityPortSettingsTab.Text,
                    cmbHandshakingPortSettingsTab.Text
                    );

                // If port did / didn't open, update timeouts and labels.
                if (serialPorts.isPortOpen())
                {
                    togglePortMenu(false);
                    serialPorts.setReadTimeout(1000);
                    serialPorts.setWriteTimeout(1000);
                    btnConnect.Text = "Disconnect";
                    btnConnectHM1X.Enabled = true;
                }
                else // If the port didn't connect, make sure UI reflects it.
                {
                    togglePortMenu(true);
                    btnConnect.Text = "Connect";
                    btnConnectHM1X.Enabled = false;
                }
                serialPorts.updateConnectionLabel(lblConnectionStatus);
            }
            else // If connceted, disconnect.
            {
                togglePortMenu(true);
                serialPorts.closePort();
                serialPorts.updateConnectionLabel(lblConnectionStatus);
                btnConnect.Text = "Connect";
                btnConnectHM1X.Enabled = false;
            }
        }

        private void String_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void btnToggleStringHexSendText_Click(object sender, EventArgs e)
        {
            string result = "";
            
            if (toggleSendBoxStringHex)
            {
                result = serialPorts.convertASCIIStringToHexString(txbSendTextBox.Text);
                toggleSendBoxStringHex = false;
                btnToggleStringHexSendText.Text = "Hex";
                txbSendTextBox.BackColor = Color.Black;
                txbSendTextBox.ForeColor = Color.WhiteSmoke;
            } else
            {
                if (serialPorts.isValidHexString(txbSendTextBox.Text))
                {
                    result = serialPorts.convertHexStringToASCIIHex(txbSendTextBox.Text);
                    btnToggleStringHexSendText.Text = "String";
                    toggleSendBoxStringHex = true;
                    txbSendTextBox.BackColor = Color.White;
                    txbSendTextBox.ForeColor = Color.Black;
                }
                else
                {
                    addSysMsgText("Invalid hex string.\n");
                    result = txbSendTextBox.Text;
                    return;
                }
            }
            txbSendTextBox.Text = result;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {

            string result = txbSendTextBox.Text;

            // If data in send box is already stringy.
            // Else if it is HEX data, convert to string, then send.

            // Add user's selected character after RX.
            result += serialPorts.getSelectedEndChars(cmbCharsAfterTx.SelectedIndex);

            if (toggleSendBoxStringHex)
            {
                serialPorts.WriteData(result);
            } else if(!toggleSendBoxStringHex)
            {
                string data = serialPorts.convertHexStringToASCIIHex(result);
                serialPorts.WriteData(data);
            } else
            {
                serialPorts.WriteData(result);
                MessageBox.Show(null, "Problem sending data.", "Send error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClearSendBox_Click(object sender, EventArgs e)
        {
            txbSendTextBox.Clear();
        }

        private void btmClearMainDisplay_Click(object sender, EventArgs e)
        {
            clearMainDisplay();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void btnConfirmHM1XSetting_Click(object sender, EventArgs e)
        {
            
        }

        private void btnConnectHM1X_Click(object sender, EventArgs e)
        {
            serialPorts.getVersion();
        }
    }



}
