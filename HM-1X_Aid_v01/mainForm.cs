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
        delegate void HM1XVariableUpdate(object sender, object originator, object value);
        
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

            // Initialize the HM1X dictionary.
            serialPorts.initDictionary();

            serialPorts.addModuleTypesToComboBox(cmbHM1XDeviceType, 0);
            // Populate COM info.
            loadCOMInfo();
            
            // RX'ed data callback.
            serialPorts.DataReceivedEventHandler += new SerialPortsExtended.DataReceivedCallback(gotData);
            serialPorts.HM1XupdatedEventHandler += new SerialPortsExtended.HM1Xupdated(HM1XupdateValue);
            serialPorts.SerialSystemUpdateEventHandler += new SerialPortsExtended.SerialSystemUpdate(serialSystemUpdate);
            this.cmbHM1XCommands.SelectedIndexChanged += new System.EventHandler(HM1XSettingsBoxChanged);

            // Setup display.
            lblConnectionStatus.BackColor = Color.Red;
        }

        private void loadSettings()
        {
            serialSystemUpdate(this, "Loading settings.\r\n", 0);
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
            cmbCharsAfterTx.SelectedIndex = Properties.Settings.Default.charsAfterTX;
            cmbHM1XDeviceType.SelectedIndex = Properties.Settings.Default.moduleType;
            tbcBottomPane.SelectedIndex = Properties.Settings.Default.bottomAreaTab;
            if(cmbHM1XCommands.Items.Count > 0)
            {
                cmbHM1XCommands.SelectedIndex = Properties.Settings.Default.hm1xCommand;
            }
            
            serialSystemUpdate(this, "Loaded settings.\r\n", 100);
        }

        private void saveSettings()
        {
            serialSystemUpdate(this, "Saving settings.\r\n", 0);
            Properties.Settings.Default.lastCom = cmbPortNumberInPortSettingsTab.SelectedIndex;
            Properties.Settings.Default.BaudRate = cmbBaudRatePortSettingsTab.SelectedIndex;
            Properties.Settings.Default.dataBits = cmbDataBitsPortSettingsTab.SelectedIndex;
            Properties.Settings.Default.stopBits = cmbStopBitsPortSettingsTab.SelectedIndex;
            Properties.Settings.Default.parity = cmbParityPortSettingsTab.SelectedIndex;
            Properties.Settings.Default.handshaking = cmbHandshakingPortSettingsTab.SelectedIndex;
            Properties.Settings.Default.displayDataSettings = cmbDataAs.SelectedIndex;
            Properties.Settings.Default.charsAfterRX = cmbCharAfterRx.SelectedIndex;
            Properties.Settings.Default.charsAfterTX = cmbCharsAfterTx.SelectedIndex;
            Properties.Settings.Default.moduleType = cmbHM1XDeviceType.SelectedIndex;
            Properties.Settings.Default.hm1xCommand = cmbHM1XCommands.SelectedIndex;
            Properties.Settings.Default.bottomAreaTab = tbcBottomPane.SelectedIndex;
            Properties.Settings.Default.Save();
            serialSystemUpdate(this, "Saved settings.\r\n", 100);
        }

        private void resetSettings()
        {
            serialSystemUpdate(this, "Clearing settings.\r\n", 0);
            Properties.Settings.Default.lastCom = 0;
            Properties.Settings.Default.BaudRate = 0;
            Properties.Settings.Default.dataBits = 0;
            Properties.Settings.Default.stopBits = 0;
            Properties.Settings.Default.parity = 0;
            Properties.Settings.Default.handshaking = 0;
            Properties.Settings.Default.displayDataSettings = 0;
            Properties.Settings.Default.charsAfterRX = 0;
            Properties.Settings.Default.charsAfterTX = 0;
            Properties.Settings.Default.moduleType = 0;
            Properties.Settings.Default.hm1xCommand = 0;
            Properties.Settings.Default.Save();
            serialSystemUpdate(this, "Cleared.\r\n", 100);
        }

        void setText(string text)
        {
            rtbMainDisplay.Text = text;
        }


        // RX event handler triggered by SerialPort.  Hands off data quick.  
        // If you try to update UI from this method, threads tangel.
        public void gotData(object sender, string data)
        {
            // Incoming data is on another thread UI cannot be updated without crashing.
            tempBuffer = data;
            this.BeginInvoke(new SetTextCallback(SetText), new object[] { tempBuffer });
        }

        // HM1XupdateValue is on the serialPorts thread, so it simply passes its load to updateHM1XVariables on the main.S
        public void HM1XupdateValue(object sender, object originator, object value)
        {
            this.BeginInvoke(new HM1XVariableUpdate(updateHM1XVariables), new object[] { sender, originator, value });
        } 

        public void updateHM1XVariables(object sender, object originator, object value)
        {
            // Updates UI after a data RX'ed event.
            serialPorts.updateUIAfterDataRX(this, originator, value, rtbMainDisplay, txbSysMsg, pbSysStatus, lblHM1XConnectionStatus);

            // This enables settings ONLY after HM-1X responds with "OK".
            if(lblHM1XConnectionStatus.Text == "Connected")
            {
                cmbHM1XSettings.Enabled = true;
                btnConfirmHM1XSetting.Enabled = true;
            }

        }


        public void serialSystemUpdate(object sender, string text, int progressBarValue)
        {
            addSysMsgText(text);
            txbSysMsg.SelectionStart = txbSysMsg.Text.Length + 10;
            txbSysMsg.ScrollToCaret();
            pbSysStatus.Value = progressBarValue;
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
            //serialSystemUpdate(this, "Loading COM info.\n", 0);
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
            serialSystemUpdate(this, "", 50);
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

                // Add commands to the combobox
                serialPorts.addHM1XCommandsToComboBox(cmbHM1XCommands, 0);

                loadSettings();

            }
            serialSystemUpdate(this, "Loaded COM info.\r\n", 100);
        }

        private void HM1XSettingsBoxChanged(object sender, EventArgs e)
        {
            hm1xConstants.hm1xEnumCommands selectedEnumeration = (hm1xConstants.hm1xEnumCommands) cmbHM1XCommands.SelectedIndex;
            if (cmbHM1XCommands.Items.Count > 0)
            {
                serialPorts.addHM1XSettingsToComboBox(cmbHM1XSettings, selectedEnumeration, txbParameterOne, txbParameterTwo, lblParameterOne, lblParameterTwo, txbSysMsg, btnConfirmHM1XSetting);
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

        private void toggleHM1XMenu(bool offOrOn)
        {
            btnConfirmHM1XSetting.Enabled = offOrOn;
            cmbHM1XCommands.Enabled = offOrOn;
            cmbHM1XDeviceType.Enabled = offOrOn;

        }

        private void toggleHM1XSettings(bool offOrOn)
        {
            cmbHM1XSettings.Enabled = offOrOn;
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
            txbSysMsg.Text += text;
             txbSysMsg.ScrollToCaret();
        }

        private void setSysMsgText(string text)
        {
            txbSysMsg.Text = text;
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
                    btnConnectHM1X.Enabled = true;
                    serialPorts.setReadTimeout(1000);
                    serialPorts.setWriteTimeout(1000);
                    btnConnect.Text = "Disconnect";

                    // Clears SerialPortsExtended settings.
                    hm1xConstants.hm1xEnumCommands selectedEnumeration = (hm1xConstants.hm1xEnumCommands)cmbHM1XCommands.SelectedIndex;
                    serialPorts.addHM1XSettingsToComboBox(cmbHM1XSettings, selectedEnumeration, txbParameterOne, txbParameterTwo, lblParameterOne, lblParameterTwo, txbSysMsg, btnConfirmHM1XSetting);
                    cmbHM1XSettings.Enabled = false;
                    btnConfirmHM1XSetting.Enabled = false;
//                    serialPorts.clearCaptureFlag();
                }
                else // If the port didn't connect, make sure UI reflects it.
                {
                    togglePortMenu(false);
                    btnConnectHM1X.Enabled = false;
                    btnConnect.Text = "Connect";
                    cmbHM1XSettings.Enabled = false;
                    btnConfirmHM1XSetting.Enabled = false;
//                    serialPorts.clearCaptureFlag();
                    serialPorts.closePort();
                }
                serialPorts.updateConnectionLabel(lblConnectionStatus);
            }
            else // If connceted, disconnect.
            {
                serialPorts.closePort();
                togglePortMenu(true);
                btnConnectHM1X.Enabled = false;
                serialPorts.updateConnectionLabel(lblConnectionStatus);
                btnConnect.Text = "Connect";
                btnConnectHM1X.Enabled = false;
                cmbHM1XSettings.Enabled = false;
                btnConfirmHM1XSetting.Enabled = false;
                serialPorts.clearCaptureFlag();
                
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
                    addSysMsgText("Invalid hex string.\r\n");
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
            serialPorts.sendCommandToHM1X(cmbHM1XCommands, cmbHM1XSettings, txbParameterOne, txbParameterTwo, txbSysMsg, pbSysStatus, btnConfirmHM1XSetting);
        }

        private void btnConnectHM1X_Click(object sender, EventArgs e)
        {
            // CONNECT
            // 1. Connect (send "AT" and get "OK").
            // 2. Get list of basic module info.
            // 3. Populate drop down with AT commands.
            toggleHM1XMenu(true);
            serialPorts.connectToHM1X();

        }

        private void cmbHM1XDeviceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            serialPorts.setModuleType(comboBox.SelectedIndex);
            hm1xConstants.hm1xEnumCommands selectedEnumeration = (hm1xConstants.hm1xEnumCommands)cmbHM1XCommands.SelectedIndex;
            if (cmbHM1XCommands.Items.Count > 0)
            {
                serialPorts.addHM1XSettingsToComboBox(cmbHM1XSettings, selectedEnumeration, txbParameterOne, txbParameterTwo, lblParameterOne, lblParameterTwo, txbSysMsg, btnConfirmHM1XSetting);
            }
        }

        private void clearParameterUI()
        {
            txbParameterOne.Visible = false;
            txbParameterTwo.Visible = false;
            lblParameterOne.Visible = false;
            lblParameterTwo.Visible = false;
            lblParameterOne.Text = "";
            lblParameterTwo.Text = "";
        }

        private void btnClearSettings_Click(object sender, EventArgs e)
        {
            resetSettings();
        }

        private void cmbHM1XCommands_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Make sure the parameter boxes and labels are hidden unless needed.
            clearParameterUI();
            cmbHM1XSettings.Items.Clear();
            serialPorts.clearSettingsList();
            serialPorts.commandSelectedMessage(rtbMainDisplay, (hm1xConstants.hm1xEnumCommands)cmbHM1XCommands.SelectedIndex);
    }

        private void cmbHM1XSettings_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Make sure the parameter boxes and labels are hidden unless needed.
            clearParameterUI();

            hm1xConstants.hm1xEnumCommands selectedEnumeration = (hm1xConstants.hm1xEnumCommands) cmbHM1XCommands.SelectedIndex;

            /// Keep it DRY.  The PowerOn and AfterConnection are the same case.
            if (selectedEnumeration == hm1xConstants.hm1xEnumCommands.PIOStateAfterConnection) { selectedEnumeration = hm1xConstants.hm1xEnumCommands.PIOStateAfterPowerOn; }

            // Default the response timeout.
            serialPorts.setResponseTimeout(250);

            switch (selectedEnumeration)
            {
                case hm1xConstants.hm1xEnumCommands.WhitelistMACAddress:
                    if (cmbHM1XSettings.SelectedIndex > 2)
                    {
                        txbParameterOne.Visible = true;
                        lblParameterOne.Text = "MAC Address (12 digits)";
                        lblParameterOne.Visible = true;
                        // Default the response timeout.
                        serialPorts.setResponseTimeout(250);
                    }
                    break;
                case hm1xConstants.hm1xEnumCommands.PIOStateAfterPowerOn:
                    if (cmbHM1XSettings.SelectedIndex > 0)
                    {
                        txbParameterOne.Visible = true;
                        lblParameterOne.Text = "1 = HIGH, 0 = LOW.";
                        lblParameterTwo.Text = "Pin 0 to B";
                        lblParameterOne.Visible = true;
                        lblParameterTwo.Visible = true;
                        // Default the response timeout.
                        serialPorts.setResponseTimeout(250);
                    }
                    break;
                case hm1xConstants.hm1xEnumCommands.Characteristic:
                    if (cmbHM1XSettings.SelectedIndex > 0)
                    {
                        txbParameterOne.Visible = true;
                        lblParameterOne.Text = "Char as HEX (4)";
                        lblParameterOne.Visible = true;
                        // Default the response timeout.
                        serialPorts.setResponseTimeout(250);
                    }
                    break;
                case hm1xConstants.hm1xEnumCommands.TryConnectionAddress:
                    lblParameterOne.Text = "MAC Address (12 digits)";
                    lblParameterOne.Visible = true;
                    txbParameterOne.Visible = true;
                    break;
                
            }
        }
    }



}
