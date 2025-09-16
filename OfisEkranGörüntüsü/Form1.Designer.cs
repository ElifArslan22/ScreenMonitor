// Form1.Designer.cs
using System.Drawing;
using System.Windows.Forms;

namespace OfisEkranGörüntüsü
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private ListBox lstClients;
        private ListBox lstLog;
        private Button btnCaptureScreens;
        private FlowLayoutPanel flowImages;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lstClients = new ListBox();
            this.lstLog = new ListBox();
            this.btnCaptureScreens = new Button();
            this.flowImages = new FlowLayoutPanel();

            this.SuspendLayout();

            // lstClients
            this.lstClients.FormattingEnabled = true;
            this.lstClients.ItemHeight = 16;
            this.lstClients.Location = new Point(12, 12);
            this.lstClients.Name = "lstClients";
            this.lstClients.Size = new Size(200, 180);

            // lstLog
            this.lstLog.FormattingEnabled = true;
            this.lstLog.ItemHeight = 16;
            this.lstLog.Location = new Point(230, 12);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new Size(400, 180);

            // btnCaptureScreens
            this.btnCaptureScreens.Location = new Point(12, 210);
            this.btnCaptureScreens.Name = "btnCaptureScreens";
            this.btnCaptureScreens.Size = new Size(618, 40);
            this.btnCaptureScreens.Text = "Capture Screens";
            this.btnCaptureScreens.UseVisualStyleBackColor = true;
            this.btnCaptureScreens.Click += new System.EventHandler(this.btnCaptureScreens_Click);

            // flowImages
            this.flowImages.Location = new Point(12, 260);
            this.flowImages.Name = "flowImages";
            this.flowImages.Size = new Size(860, 280);
            this.flowImages.AutoScroll = true;

            // Form1
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(890, 560);
            this.Controls.Add(this.lstClients);
            this.Controls.Add(this.lstLog);
            this.Controls.Add(this.btnCaptureScreens);
            this.Controls.Add(this.flowImages);
            this.Name = "Form1";
            this.Text = "Ofis Ekran Görüntü̈sü Sunucu";
            this.ResumeLayout(false);
        }
    }
}
 