using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ImageProcessingWinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpenBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FolderInput = new FolderBrowserDialog();
            FolderInput.ShowDialog();
            txtInputFolder.Text = FolderInput.SelectedPath;
        }

        private void btnOutputBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FolderInput = new FolderBrowserDialog();
            FolderInput.ShowDialog();
            txtOutputFolder.Text = FolderInput.SelectedPath;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            DirectoryInfo FolderInputInfo = new DirectoryInfo(txtInputFolder.Text.Trim());
            FileInfo[] Images = FolderInputInfo.GetFiles("*.tif");

            // Process all images
            foreach (FileInfo f in Images)
            {

                // Open image
                Bitmap imgInput = (Bitmap)Image.FromFile(f.FullName);

                // Create deskew object and rotate image
                Deskew deskewImage = new Deskew();
                Bitmap skewImage = deskewImage.DeskewImage(imgInput);
                skewImage.Save(txtOutputFolder.Text.Trim() + "\\" + f.Name, System.Drawing.Imaging.ImageFormat.Jpeg);

                imgInput.Dispose();
                skewImage.Dispose();

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
             DirectoryInfo FolderInputInfo = new DirectoryInfo(txtInputFolder.Text.Trim());
            FileInfo[] Images = FolderInputInfo.GetFiles("*.tif");

            // Process all images
            foreach (FileInfo f in Images)
            {
                // Open image
                Bitmap imgInput = (Bitmap)Image.FromFile(f.FullName);

                // Create Crop object and crop image
                Crop cropImage = new Crop();
                Bitmap cropedImage = cropImage.CropImage(imgInput);

                // Save the result
                cropedImage.Save(txtOutputFolder.Text.Trim() + "\\" + f.Name, System.Drawing.Imaging.ImageFormat.Jpeg);
                imgInput.Dispose();
                cropedImage.Dispose();
            }
        }
    }
}
