using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WIA; // Add the WIA namespace

namespace WinFormsWIA
{
    public partial class Form1 : Form
    {
        // Scanner only device properties (DPS)
        public const int WIA_RESERVED_FOR_NEW_PROPS = 1024;
        public const int WIA_DIP_FIRST = 2;
        public const int WIA_DPA_FIRST = WIA_DIP_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
        public const int WIA_DPC_FIRST = WIA_DPA_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
        public const int WIA_DPS_FIRST = WIA_DPC_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
        public const int WIA_DPS_DOCUMENT_HANDLING_STATUS = WIA_DPS_FIRST + 13;
        public const int WIA_DPS_DOCUMENT_HANDLING_SELECT = WIA_DPS_FIRST + 14;
        public const int FEEDER = 1;
        public const int FLATBED = 2;
        public const int DUPLEX = 4;
        public const int FEED_READY = 1;

        public WIA.DeviceManager deviceManager;
        public WIA.Device selectedDevice;
        public WIA.CommonDialog _dialog = new();
        public WIA.Device _scanner;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            deviceManager = new WIA.DeviceManager();
            // Populate the device list combo box
            foreach (WIA.DeviceInfo device in deviceManager.DeviceInfos)
            {
                comboBoxDevices.Items.Add(device.Properties["Name"].get_Value());
            }

            if (comboBoxDevices.Items.Count > 0)
            {
                comboBoxDevices.SelectedIndex = 0; // Select the first device by default
            }
        }

        private void buttonScan_Click(object sender, EventArgs e)
        {
            if (comboBoxDevices.SelectedItem == null)
            {
                MessageBox.Show("Please select a scanner device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string? selectedDeviceName = comboBoxDevices.SelectedItem.ToString();
            foreach (WIA.DeviceInfo device in deviceManager.DeviceInfos)
            {
                if (device.Properties["Name"].get_Value() == selectedDeviceName)
                {
                    selectedDevice = device.Connect();
                    break;
                }
            }

            if (selectedDevice == null)
            {
                MessageBox.Show("Failed to connect to the selected device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                //_dialog = new();
                //_scanner = _dialog.ShowSelectDevice(WIA.WiaDeviceType.ScannerDeviceType, false, false);

                WIA.Item item = selectedDevice.Items[1]; // Get the scanner item (usually index 1)

                // Configure scanner settings (e.g., resolution, color) - IMPORTANT!
                WIA.Properties properties = item.Properties;
                // Example: Set resolution to 300 DPI
                SetProperty(properties, "HorizontalResolution", 300);
                SetProperty(properties, "VerticalResolution", 300);
                // Example: Set color to grayscale
                SetProperty(properties, "ColorDesired", 1); // 1 = Grayscale, 2 = Color

                // Configurar las propiedades del escáner para usar el ADF
                SetProperty(properties, "Document Handling Select", 1);
                SetProperty(properties, "Document Handling Enabled", 1);

                List<Image> scannedImages = new List<Image>();

                while (true) // Loop for multi-page scanning
                {
                    WIA.ImageFile imageFile = (WIA.ImageFile)item.Transfer("{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}"); // Scan to JPEG

                    // Convert WIA ImageFile to System.Drawing.Image
                    Image image = Image.FromStream(new System.IO.MemoryStream(imageFile.FileData.get_BinaryData()));
                    scannedImages.Add(image);

                    // Display the image (optional - for testing)
                    pictureBoxPreview.Image = image;

                    /*DialogResult result = MessageBox.Show("Scan another page?", "Multi-Page Scan", MessageBoxButtons.YesNo);
                    if (result == DialogResult.No)
                    {
                        break; // Exit the loop if the user doesn't want to scan more pages
                    }*/

                    // Comprobar si hay más páginas en el ADF
                    if (item.Properties["Document Handling Status"].get_Value() != 1)
                    {
                        break;
                    }
                }

                int imageCount = 0;
                // Save the scanned images (example)
                for (int i = 0; i < scannedImages.Count; i++)
                {
                    imageCount++;
                    scannedImages[i].Save("C:\\indexador\\scan_" + imageCount.ToString().PadLeft(5, '0') + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss") + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                }

                MessageBox.Show($"Scanned {scannedImages.Count} pages.", "Scan Complete");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during scan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (selectedDevice != null)
                {
                    // Release the device (important!)
                    selectedDevice = null;
                }
            }
        }

        // Helper function to set WIA properties
        private void SetProperty(WIA.Properties properties, string propertyName, object value)
        {
            try
            {
                WIA.Property property = properties[propertyName];
                property.set_Value(value);
            }
            catch (Exception ex)
            {
                // Handle property setting errors (e.g., property not supported)
                Console.WriteLine($"Error setting property {propertyName}: {ex.Message}");
            }
        }
    }
}
