using Saraff.Twain;

namespace WinFormsAppScan
{
    public partial class Form1 : Form
    {
        private bool _isEnable = false;
        public int imageCount = 0;

        public Form1()
        {
            InitializeComponent();

            try
            {
                this._twain.OpenDSM();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\n\n{1}", ex.Message, ex.StackTrace), "SAMPLE1", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    using (SelectSourceForm _dlg = new SelectSourceForm { Twain = this._twain })
                    {
                        if (_dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            this._twain.SetDefaultSource(_dlg.SourceIndex);
                            this._twain.SourceIndex = _dlg.SourceIndex;
                        }
                    }
                }
                else
                {
                    this._twain.CloseDataSource();
                    this._twain.SelectSource();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SAMPLE1", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                this._twain.Acquire();
            }
            catch (TwainException ex)
            {
                if (ex.ReturnCode == TwRC.Cancel && ex.ConditionCode == TwCC.Success)
                {
                    MessageBox.Show(text: "Se han cancelado", caption: "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _twain.CloseDataSource();
                    _twain.CloseDSM();
                    return;
                }

                if (ex.Message == "It worked!")
                {
                    MessageBox.Show(text: "Se presentó un error Twain Error: ", caption: "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
            }
        }

        private void _twain_EndXfer(object sender, Twain32.EndXferEventArgs e)
        {
            try
            {
                if (e.Image != null)
                {
                    this.pictureBox1.Image?.Dispose();
                    this.pictureBox1.Image = null;
                    this.pictureBox1.Image = e.Image;

                    imageCount++;
                    string newFilePath = "C:\\indexador\\" + "file_tmp_" + imageCount.ToString().PadLeft(5, '0') + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss") + ".jpg";
                    using Bitmap tempImage = new(e.Image);
                    using System.IO.FileStream fstream = new(newFilePath, FileMode.Create, FileAccess.ReadWrite);
                    tempImage.Save(fstream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    fstream.Dispose();
                    fstream.Close();
                    tempImage.Dispose();
                    GC.Collect();
                }
                else
                {
                    MessageBox.Show(text: "Se han escaneado " + this._twain.ImageCount + " hojas.", caption: "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _twain.CloseDataSource();
                    _twain.CloseDSM();
                }
                //this.pictureBox1.Image = e.Image;
            }
            catch (TwainException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _twain.CloseDataSource();
                _twain.CloseDSM();
                return;
            }
        }

        private void _twain_AcquireCompleted(object sender, EventArgs e)
        {
            try
            {
                /*if (this.pictureBox1.Image != null)
                {
                    this.pictureBox1.Image.Dispose();
                }

                if (this._twain.ImageCount > 0)
                {
                    this.pictureBox1.Image = this._twain.GetImage(0);
                }*/

                //for (int i = 0; i < this._twain.ImageCount; i++)
                //{
                //    //imageCount++;
                //    //string newFilePath = "C:\\indexador\\" + "file_tmp_" + imageCount.ToString().PadLeft(5, '0') + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss") + ".tiff";
                //    //using (Bitmap tempImage = new(this._twain.GetImage(i)))
                //    //{
                //    //    using System.IO.FileStream fstream = new(newFilePath, FileMode.Create, FileAccess.ReadWrite);
                //    //    tempImage.Save(fstream, System.Drawing.Imaging.ImageFormat.Tiff);
                //    //    fstream.Dispose();
                //    //    fstream.Close();
                //    //    GC.Collect();
                //    //}


                //    using var _image = this._twain.GetImage(i);
                //    _image.Save(Path.GetTempFileName());
                //}

                MessageBox.Show(text: "Se han escaneado " + this._twain.ImageCount + " hojas.", caption: "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _twain.CloseDataSource();
                _twain.CloseDSM();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void Twain_ScanError(object? sender, Twain32.AcquireErrorEventArgs e)
        {
            if (e.Exception.ConditionCode == Saraff.Twain.TwCC.Success && e.Exception.ReturnCode == TwRC.Cancel)
            {
                MessageBox.Show(text: "Se han escaneado " + this._twain.ImageCount + " hojas.", caption: "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _twain.CloseDataSource();
                _twain.CloseDSM();
                return;
            }
            else if(e.Exception.Message == "It worked!")
            {
                MessageBox.Show(text: "Se presentó un error Twain Error: " + e.Exception.Message, caption: "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _twain.CloseDataSource();
                _twain.CloseDSM();
                return;
            }
            else
            {
                MessageBox.Show($"Twain Error: {e.Exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _twain.CloseDataSource();
                _twain.CloseDSM();
                return;
            }
        }

        private void _twain_TwainStateChanged(object sender, Twain32.TwainStateEventArgs e)
        {
            try
            {
                if ((e.TwainState & Twain32.TwainStateFlag.DSEnabled) == 0 && this._isEnable)
                {
                    this._isEnable = false;
                    // <<< scaning finished (or closed)
                }
                this._isEnable = (e.TwainState & Twain32.TwainStateFlag.DSEnabled) != 0;
            }
            catch (TwainException ex)
            {
                MessageBox.Show(ex.Message, "SAMPLE1", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }


    }
}
