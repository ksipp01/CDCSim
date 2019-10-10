using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ASCOM.DeviceInterface;
using System.Collections;
using ASCOM.DriverAccess;

namespace ASCOM.SimCDC
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {

        // 9-30-19 added UseDSS

        // add 
        public static ASCOM.DriverAccess.Focuser focuser;
        public static ASCOM.DriverAccess.Telescope telescope;



        //private const string STR_N0 = "N0";
        private const string STR_N2 = "N2";
        private Camera camera;
       // public static Camera camera; // changed for focus sim

        internal bool okButtonPressed = false;

        public SetupDialogForm()
        {
            InitializeComponent();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            okButtonPressed = true;
            SaveProperties();
            Dispose();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            okButtonPressed = false;
            Dispose();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void BrowseToAscom(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }



        //added 10-7-19 for dss
        //private static string dssRA;
        //private static string dssDEC;
        //private static string dssScale;
        //private static string dssWidth;
        //private static string dssHeight;

        internal void InitProperties(Camera theCamera)
        {
            checkBoxLogging.Checked = Log.Enabled;
            this.checkBoxInterfaceVersion.Checked = (theCamera.interfaceVersion == 2);
            this.textBoxPixelSizeX.Text = theCamera.pixelSizeX.ToString(STR_N2, CultureInfo.CurrentCulture);
            this.textBoxPixelSizeY.Text = theCamera.pixelSizeY.ToString(STR_N2, CultureInfo.CurrentCulture);
            //this.textBoxFullWellCapacity.Text = camera.fullWellCapacity.ToString();
            this.textBoxMaxADU.Text = theCamera.maxADU.ToString(CultureInfo.CurrentCulture);
            this.textBoxElectronsPerADU.Text = theCamera.electronsPerADU.ToString(STR_N2, CultureInfo.CurrentCulture);

            this.textBoxCameraXSize.Text = theCamera.cameraXSize.ToString(CultureInfo.CurrentCulture);
            width = theCamera.cameraXSize;
            this.textBoxCameraYSize.Text = theCamera.cameraYSize.ToString(CultureInfo.CurrentCulture);
            height = theCamera.cameraYSize;
            this.checkBoxCanAsymmetricBin.Checked = theCamera.canAsymmetricBin;
            this.textBoxMaxBinX.Text = theCamera.maxBinX.ToString(CultureInfo.CurrentCulture);
            this.textBoxMaxBinY.Text = theCamera.maxBinY.ToString(CultureInfo.CurrentCulture);
            this.checkBoxHasShutter.Checked = theCamera.hasShutter;
            this.textBoxSensorName.Text = theCamera.sensorName;
            this.comboBoxSensorType.SelectedIndex = (int)theCamera.sensorType;
            this.textBoxBayerOffsetX.Text = theCamera.bayerOffsetX.ToString(CultureInfo.CurrentCulture);
            this.textBoxBayerOffsetY.Text = theCamera.bayerOffsetY.ToString(CultureInfo.CurrentCulture);
            this.checkBoxOmitOddBins.Checked = theCamera.omitOddBins;

            this.checkBoxHasCooler.Checked = theCamera.hasCooler;
            this.checkBoxCanSetCCDTemperature.Checked = theCamera.canSetCcdTemperature;
            this.checkBoxCanGetCoolerPower.Checked = theCamera.canGetCoolerPower;

            this.checkBoxCanAbortExposure.Checked = theCamera.canAbortExposure;
            this.checkBoxCanStopExposure.Checked = theCamera.canStopExposure;
            this.textBoxMaxExposure.Text = theCamera.exposureMax.ToString(CultureInfo.CurrentCulture);
            this.textBoxMinExposure.Text = theCamera.exposureMin.ToString(CultureInfo.CurrentCulture);

            if (theCamera.gains != null && theCamera.gains.Count > 0)
            {
                radioButtonUseGains.Checked = true;
            }
            else if (theCamera.gainMax > theCamera.gainMin)
            {
                radioButtonUseMinAndMax.Checked = true;
            }
            else
            {
                radioButtonNoGain.Checked = true;
            }
            this.textBoxGainMin.Text = theCamera.gainMin.ToString(CultureInfo.CurrentCulture);
            this.textBoxGainMax.Text = theCamera.gainMax.ToString(CultureInfo.CurrentCulture);
            this.checkBoxApplyNoise.Checked = theCamera.applyNoise;

            this.checkBoxCanPulseGuide.Checked = theCamera.canPulseGuide;

            this.checkBoxCanFastReadout.Checked = theCamera.canFastReadout;
            if (theCamera.canFastReadout)
            {
                this.checkBoxUseReadoutModes.Enabled = false;
            }
            else
            {
                this.checkBoxUseReadoutModes.Checked = theCamera.readoutModes.Count > 1;
            }

            //add
            this.textBoxFocusPoint.Text = theCamera.FocusPoint.ToString(CultureInfo.CurrentCulture);
            this.textBoxFocusStepSize.Text = theCamera.FocusStepSize.ToString(CultureInfo.CurrentCulture);
            this.checkBoxUseFocusSim.Checked = theCamera.useFocusSim;
            this.checkBoxUseCapture.Checked = theCamera.useCapture;
            checkBox1.Checked = theCamera.useDSS;
            capturePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ASCOM_SimCDC_Camera";

            this.camera = theCamera;


            //dssWidth = theCamera.CameraXSize.ToString(CultureInfo.CurrentCulture);
            //dssHeight = theCamera.CameraYSize.ToString(CultureInfo.CurrentCulture);

        }

        private static int focusPoint;
        private static int focusStepSize;
        private static int height;
        private static int width;
        private static int _xPoint;
        private static int _yPoint;
        private static string capturePath;
        private static string setImage;
        private static string ra;
        private static string dec;

        private static bool useDSS = true;  // for some reason not reading checkbox1 properly 
        public static bool UseDSS
        {
            get { return useDSS; }
            set { useDSS = value; }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                useDSS = true;
            else
                useDSS = false;
        }


        public static string Ra
        { 
            get { return telescope.RightAscension.ToString();  }
            set { ra = value; }
        }
        public static string Dec
        {
            get { return telescope.Declination.ToString(); }
            set { dec = value; }
        }

        public static int Height
        {
            get { return height; }
            set { height = value; }
        }
        public static int Width
        {
            get { return width; }
            set { width = value; }
        }
        public static int FocusPoint
        {
            get { return focusPoint; }
            set { focusPoint = value; }
        }
        public static int FocusStepSize
        {
            get { return focusStepSize; }
            set { focusStepSize = value; }
        }
        public static int xPoint
        {
            get { return _xPoint; }
            set { _xPoint = value; }
        }
        public static int yPoint
        {
            get { return _yPoint; }
            set { _yPoint = value; }
        }
        public static string CapturePath
        {
            get { return capturePath; }
            set { capturePath = value; }
        }
        public static string SetImage
        {
            get { return setImage; }
            set { setImage = value; }
        }

        private void SetCamareValues()
        {
            textBoxCameraYSize.Text = width.ToString();
            textBoxCameraXSize.Text = height.ToString();
        }
        private void SaveProperties()
        {
            Log.Enabled = checkBoxLogging.Checked;
            camera.pixelSizeX = double.Parse(this.textBoxPixelSizeX.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
            camera.pixelSizeY = double.Parse(this.textBoxPixelSizeY.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
          
            //camera.fullWellCapacity = Convert.ToDouble(this.textBoxFullWellCapacity.Text, CultureInfo.InvariantCulture);
            camera.maxADU = int.Parse(this.textBoxMaxADU.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
            camera.electronsPerADU = double.Parse(this.textBoxElectronsPerADU.Text, NumberStyles.Number, CultureInfo.CurrentCulture);

          //  camera.cameraXSize = int.Parse(this.textBoxCameraXSize.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
            camera.cameraXSize = Width;
            camera.cameraYSize = Height;
         //   camera.cameraYSize = int.Parse(this.textBoxCameraYSize.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
            
            camera.canAsymmetricBin = this.checkBoxCanAsymmetricBin.Checked;
            camera.maxBinX = short.Parse(this.textBoxMaxBinX.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
            camera.maxBinY = short.Parse(this.textBoxMaxBinY.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
            camera.hasShutter = this.checkBoxHasShutter.Checked;
            camera.sensorName = this.textBoxSensorName.Text;
            camera.sensorType = (SensorType)this.comboBoxSensorType.SelectedIndex;
            camera.bayerOffsetX = short.Parse(this.textBoxBayerOffsetX.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
            camera.bayerOffsetY = short.Parse(this.textBoxBayerOffsetY.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
            camera.omitOddBins = this.checkBoxOmitOddBins.Checked;

            camera.hasCooler = this.checkBoxHasCooler.Checked;
            camera.canSetCcdTemperature = this.checkBoxCanSetCCDTemperature.Checked;
            camera.canGetCoolerPower = this.checkBoxCanGetCoolerPower.Checked;

            camera.canAbortExposure = this.checkBoxCanAbortExposure.Checked;
            camera.canStopExposure = this.checkBoxCanStopExposure.Checked;
            camera.exposureMin = double.Parse(this.textBoxMinExposure.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
            camera.exposureMax = double.Parse(this.textBoxMaxExposure.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
            camera.applyNoise = this.checkBoxApplyNoise.Checked;


            //add
            camera.focusPoint = int.Parse(this.textBoxFocusPoint.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
            focusPoint = camera.focusPoint;
            camera.focusStepSize = int.Parse(this.textBoxFocusStepSize.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
            focusStepSize = camera.focusStepSize;
            camera.useFocusSim = this.checkBoxUseFocusSim.Checked;
            camera.xPoint = _xPoint;
            camera.yPoint = _yPoint;

            camera.useCapture = this.checkBoxUseCapture.Checked;
            camera.useDSS = this.checkBox1.Checked;
            if (useCapture)
            camera.imagePath = Path.Combine(CapturePath, @"SimCapture.jpg");
            if (useDSS)
            camera.imagePath = Path.Combine(CapturePath, @"SimCapture.jpg");

            // add




            camera.canPulseGuide = this.checkBoxCanPulseGuide.Checked;

            if (this.radioButtonNoGain.Checked)
            {
                camera.gainMin = camera.gainMax = 0;
                camera.gains = null;
            }
            else if (this.radioButtonUseGains.Checked)
            {
                camera.gains = new ArrayList { "ISO 100", "ISO 200", "ISO 400", "ISO 800", "ISO 1600" };
                camera.gainMin = (short)0;
                camera.gainMax = (short)(camera.gains.Count - 1);
            }
            if (this.radioButtonUseMinAndMax.Checked)
            {
                camera.gains = null;
                camera.gainMin = short.Parse(textBoxGainMin.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
                camera.gainMax = short.Parse(textBoxGainMax.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
            }
            camera.interfaceVersion = (short)(checkBoxInterfaceVersion.Checked ? 2 : 1);

            camera.canFastReadout = this.checkBoxCanFastReadout.Checked;
            if (this.checkBoxUseReadoutModes.Checked)
            {
                camera.readoutModes = new ArrayList { "Raw Monochrome", "Live View", "Raw To Hard Drive" };
            }
            else
            {
                camera.readoutModes = new ArrayList { "Default" };
            }
        }

        private void buttonSetImageFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.CheckPathExists = true;
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(camera.imagePath);
            openFileDialog1.FileName = Path.GetFileName(camera.imagePath);
            openFileDialog1.ShowDialog();
            camera.imagePath = openFileDialog1.FileName;
            useCapture = false;
            checkBoxUseCapture.Checked = false;
            setImage = camera.imagePath;
          //  CapturePath = Path.GetDirectoryName(camera.imagePath);
        }

        private void checkBoxInterfaceVersion_CheckedChanged(object sender, EventArgs e)
        {
            // enable the V2 properties if checked
            this.textBoxBayerOffsetX.Enabled = checkBoxInterfaceVersion.Checked;
            this.textBoxBayerOffsetY.Enabled = checkBoxInterfaceVersion.Checked;
            this.textBoxGainMax.Enabled = checkBoxInterfaceVersion.Checked;
            this.textBoxGainMin.Enabled = checkBoxInterfaceVersion.Checked;
            this.textBoxMaxExposure.Enabled = checkBoxInterfaceVersion.Checked;
            this.textBoxMinExposure.Enabled = checkBoxInterfaceVersion.Checked;
            this.textBoxSensorName.Enabled = checkBoxInterfaceVersion.Checked;
            this.comboBoxSensorType.Enabled = checkBoxInterfaceVersion.Checked;
            this.radioButtonNoGain.Enabled = checkBoxInterfaceVersion.Checked;
            this.radioButtonUseGains.Enabled = checkBoxInterfaceVersion.Checked;
            this.radioButtonUseMinAndMax.Enabled = checkBoxInterfaceVersion.Checked;
        }

        private void checkBoxCanFastReadout_CheckedChanged(object sender, EventArgs e)
        {
            this.checkBoxUseReadoutModes.Enabled = !(sender as CheckBox).Checked;
        }

        private void comboBoxSensorType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var si = (sender as ComboBox).SelectedItem as string;
            labelBayerOffsetX.Enabled =
                labelBayerOffsetY.Enabled =
                textBoxBayerOffsetX.Enabled =
                textBoxBayerOffsetY.Enabled = (si != "Monochrome" && si != "Color");
        }

        private void checkBoxLogging_CheckedChanged(object sender, EventArgs e)
        {
            Log.Enabled = checkBoxLogging.Checked;
        }


       private string focusId;
       private string mountId;

        private bool IsConnected
        {
            get
            {
                return ((SetupDialogForm.focuser != null) && (focuser.Connected == true));
            }
        }

        private bool IsMountConnected
        {
            get
            {
                return ((SetupDialogForm.telescope != null) && (telescope.Connected == true));
            }
        }

        private void SetupDialogForm_Load(object sender, EventArgs e)
        {
         if (focuser != null && focuser.Connected) return;
            try
            {
                focusId = Focuser.Choose(focusId);
                if (IsConnected)
                {
                   focuser.Connected = false;
                  //  timer1.Stop();
                   // SetUIState();
                    return;
                }
                else
                {
                    //   if (driver == null)
                    focuser = new ASCOM.DriverAccess.Focuser(focusId);
                    //   driver.Link = true;
                    focuser.Connected = true;
                   // SetUIState();
                    //timer1.Start();
                }


               // lblCameraName.Text = cameraId;
            }
            catch (Exception ex)
            {
                String msg = ex.Message;
                if (ex.InnerException != null)
                    msg += " - " + ex.InnerException.Message;
                MessageBox.Show(string.Format("Choose failed with error {0}", msg));
            }
// added 10-7-19 try to connect mount too for DSS image tracking
            if (telescope != null && telescope.Connected) return;
            try
            {
                mountId = Telescope.Choose(mountId);
                if (IsMountConnected)
                {
                    telescope.Connected = false;
                    //  timer1.Stop();
                    // SetUIState();
                    return;
                }
                else
                {
                    //   if (driver == null)
                    telescope = new ASCOM.DriverAccess.Telescope(mountId);
                    //   driver.Link = true;
                    telescope.Connected = true;
                    // SetUIState();
                    //timer1.Start();
                }


                // lblCameraName.Text = cameraId;
            }
            catch (Exception ex)
            {
                String msg = ex.Message;
                if (ex.InnerException != null)
                    msg += " - " + ex.InnerException.Message;
                MessageBox.Show(string.Format("Choose failed with error {0}", msg));
            }



        }

        private void checkBoxUseFocusSim_CheckedChanged(object sender, EventArgs e)
        {
            // todo add close focus simulator if unchecked from checked  
            //open focus chooser if checked from unchecked.  
        }

        private void buttonCapture_Click(object sender, EventArgs e)
        {
            SetCamareValues();
            ScreenCapture sc = new ScreenCapture();
            sc.GetCapture();
           
        }

        private void textBoxCameraXSize_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBoxCameraYSize_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void textBoxCameraYSize_Validated(object sender, EventArgs e)
        {
            Height = int.Parse(this.textBoxCameraYSize.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
        }

        private void textBoxCameraXSize_Validated(object sender, EventArgs e)
        {
            Width = int.Parse(this.textBoxCameraXSize.Text, NumberStyles.Number, CultureInfo.CurrentCulture);
        }

        //add for selectcapturewindow

       
        private void button1_Click(object sender, EventArgs e)// set capture size button
        {
            //ControlPanel cp = new ControlPanel();
            //cp.InstanceRef = this;
            //cp.Show();
            CapturePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ASCOM_SimCDC_Camera";
            camera.imagePath = Path.Combine(CapturePath, @"SimCapture.jpg");
            //  this.Hide();
            SelectCaptureWindow scw = new SelectCaptureWindow();
            scw.InstanceRef = this;
            scw.Show();
      
        }
        private static bool useCapture;
        public static bool UseCapture
        {
            get { return useCapture; }
            set { useCapture = value; }
        }
        private void checkBoxUseCapture_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUseCapture.Checked)
                useCapture = true;
            else
                useCapture = false;
        }
       
       
       
    }
}