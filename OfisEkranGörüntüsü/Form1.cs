// Form1.cs
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OfisEkranGörüntüsü// Ofis Ekran Görüntüsü Uygulaması

{
    public partial class Form1 : Form
    {
        private TcpListener? _listener;
        private readonly ConcurrentDictionary<TcpClient, string> _clients = new();
        private CancellationTokenSource _cts = new();

        public Form1()
        {
            InitializeComponent();
            StartServer();
        }

        private void StartServer()
        {
            _listener = new TcpListener(IPAddress.Any, 5000);
            _listener.Start();
            _ = Task.Run(AcceptClientsAsync, _cts.Token); // CS4014 çözümü
        }
        private async Task AcceptClientsAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                    var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                    if (endpoint != null)
                    {
                        _clients[client] = endpoint.Address.ToString();
                        _ = Task.Run(async () => await HandleClientAsync(client, endpoint.Address.ToString()), _cts.Token);

                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
        }
        private void ClearOldImages()
        {
            try
            {
                var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.png");
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Görüntü silme hatası: {ex.Message}");
            }
        }

        private async Task HandleClientAsync(TcpClient client, string ip)
        {
            try
            {
                var stream = client.GetStream();
                while (client.Connected)
                {
                    await Task.Delay(100, _cts.Token);
                }
            }
            catch { }
            finally
            {
                _clients.TryRemove(client, out _);
            }
        }

        private async void btnCaptureScreens_Click(object sender, EventArgs e)
        {
            ClearOldImages(); // Önceki görüntüleri sil

            var tasks = new List<Task<bool>>();
            foreach (var kvp in _clients)
            {
                var client = kvp.Key;
                var ip = kvp.Value;
                if (client.Connected)
                {
                    tasks.Add(Task.Run(() => SendCaptureCommandAndReceiveImageAsync(client, ip)));
                }
            }

            bool[] results = await Task.WhenAll(tasks);
            int successCount = results.Count(r => r);
            AddLog($"{successCount} istemciden ekran görüntüsü alındı.");

            RefreshImagePreviews(); // Yeni görüntüleri göster
        }

        private async Task<bool> SendCaptureCommandAndReceiveImageAsync(TcpClient client, string ip)
        {
            try
            {
                if (!client.Connected) return false;

                NetworkStream stream = client.GetStream();

                var command = Encoding.UTF8.GetBytes("CAPTURE");
                await stream.WriteAsync(command, 0, command.Length);

                var lengthBuffer = new byte[4];
                int read = await stream.ReadAsync(lengthBuffer, 0, 4);
                if (read < 4) return false;

                int imageLength = BitConverter.ToInt32(lengthBuffer, 0);
                var imageBuffer = new byte[imageLength];
                int totalRead = 0;

                while (totalRead < imageLength)
                {
                    int bytesRead = await stream.ReadAsync(imageBuffer, totalRead, imageLength - totalRead);
                    if (bytesRead == 0) break;
                    totalRead += bytesRead;
                }

                if (totalRead < imageLength) return false;

                using var ms = new MemoryStream(imageBuffer);
                using var img = Image.FromStream(ms);
                using var bmp = new Bitmap(img);

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    var font = new Font("Arial", 16, FontStyle.Bold);
                    var brush = new SolidBrush(Color.Yellow);
                    var point = new PointF(10, bmp.Height - 30);
                    g.DrawString(ip, font, brush, point);
                }

                var fileName = $"{ip.Replace(":", "_")}.png";
                try
                {
                    if (File.Exists(fileName))
                        File.Delete(fileName);

                    bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                }
                catch (Exception ex)
                {
                    AddLog($"Hata ({ip}): {ex.Message}");
                    return false;
                }

                bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);

                return true;
            }
            catch (Exception ex)
            {
                AddLog($"Hata: {ex.Message}");
                return false;
            }
        }


        private void AddLog(string message)
        {
            if (lstLog.InvokeRequired)
            {
                lstLog.Invoke(new Action(() => lstLog.Items.Add($"{DateTime.Now:HH:mm:ss} {message}")));
            }
            else
            {
                lstLog.Items.Add($"{DateTime.Now:HH:mm:ss} {message}");
            }
        }

        private void RefreshImagePreviews()
        {
            if (flowImages.InvokeRequired)
            {
                flowImages.Invoke(new Action(RefreshImagePreviews));
                return;
            }

            flowImages.Controls.Clear();

            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.png")
                .OrderBy(f => GetIpFromFileName(Path.GetFileName(f)))
                .ToList();

            foreach (var file in files)
            {
                PictureBox pb = new PictureBox();
                pb.SizeMode = PictureBoxSizeMode.Zoom;
                pb.Width = 200;
                pb.Height = 120;
                pb.Margin = new Padding(10);
                pb.Cursor = Cursors.Hand;

                // Dosyadan okurken kilitlenmemesi için
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    using var ms = new MemoryStream();
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    pb.Image = Image.FromStream(ms);
                }

                pb.Click += (s, e) =>
                {
                    var frm = new Form
                    {
                        WindowState = FormWindowState.Maximized,
                        FormBorderStyle = FormBorderStyle.None,
                        BackColor = Color.Black
                    };

                    var bigPb = new PictureBox
                    {
                        Dock = DockStyle.Fill,
                        Image = Image.FromFile(file),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = Color.Black
                    };

                    bigPb.Click += (ss, ee) => frm.Close();
                    frm.Controls.Add(bigPb);
                    frm.ShowDialog();
                };

                flowImages.Controls.Add(pb);
            }
        }


        private string GetIpFromFileName(string fileName)
        {
            var parts = fileName.Split('_');
            return parts.Length > 0 ? parts[0] : "";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cts.Cancel();
            _listener.Stop();
            base.OnFormClosing(e);
        }

        static async Task StartClientLoop()
        {
            string serverIp = "127.0.0.1";
            int port = 5000;

            while (true)
            {
                try
                {
                    using var client = new TcpClient();
                    await client.ConnectAsync(serverIp, port);
                    using var stream = client.GetStream();

                    while (client.Connected)
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead <= 0) break;

                        string command = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        if (command.StartsWith("CAPTURE"))
                        {
                            byte[] imageBytes = CaptureScreenAsPng();
                            byte[] lengthBytes = BitConverter.GetBytes(imageBytes.Length);
                            await stream.WriteAsync(lengthBytes, 0, 4);
                            await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
                        }
                    }
                }
                catch
                {
                    await Task.Delay(2000); // Bağlantı hatası olursa tekrar dene
                }
            }
        }

        private static byte[] CaptureScreenAsPng()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            using Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            }
            using MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
