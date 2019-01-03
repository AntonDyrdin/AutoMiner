using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Timers;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using System.Web;
namespace AutoMiner_2._0
{
    public partial class AutoMiner : Form
    {
        public AutoMiner()
        {
            InitializeComponent();
            //  this.ShowInTaskbar = false;

            // создаем элементы меню
            ToolStripMenuItem StartMenuItem = new ToolStripMenuItem("Start");
            ToolStripMenuItem StopMenuItem = new ToolStripMenuItem("Stop");
            ToolStripMenuItem ExitMenuItem = new ToolStripMenuItem("Exit");
            StartMenuItem.BackColor = Color.FromArgb(100, 255, 200, 255);
            StartMenuItem.ForeColor = Color.FromArgb(255, 0, 100, 0);
            StopMenuItem.BackColor = Color.FromArgb(100, 255, 150, 255);
            StopMenuItem.ForeColor = Color.FromArgb(200, 100, 100, 0);
            ExitMenuItem.BackColor = Color.FromArgb(100, 150, 50, 255);
            ExitMenuItem.ForeColor = Color.FromArgb(100, 0, 0, 0);
            // добавляем элементы в меню
            contextMenuStrip1.Items.AddRange(new[] { StartMenuItem, StopMenuItem, ExitMenuItem });
            // ассоциируем контекстное меню с текстовым полем
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            // устанавливаем обработчики событий для меню
            StartMenuItem.Click += button2_Click_1;
            StopMenuItem.Click += button3_Click_1;
            ExitMenuItem.Click += Exit;
            try
            {
                var files = Directory.GetFiles(Environment.CurrentDirectory + "//background_image");
                Random r = new Random();
                this.BackgroundImage = Image.FromFile(files[r.Next(0, files.Length)]);
            }
            catch { }
        }
        private void Exit(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Environment.Exit(0);
        }
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
            else
            {
                contextMenuStrip1.Show();
            }
        }


        private void AutoMiner_Resize(object sender, EventArgs e)
        {

            // проверяем наше окно, и если оно было свернуто, делаем событие        

        }
        PictureBox picBox;
        Graphics g;
        Bitmap bitmap;
        public delegate void StringDelegate(string is_completed, Color col);
        public StringDelegate stringDelegate;
        public delegate void DrawDelegate(Color col, double x1, double y1, double x2, double y2);
        public DrawDelegate drawDelegate;
        public delegate void DrawStringDelegate(string s, int depth, double x, double y);
        public DrawStringDelegate drawStringDelegate;
        int off_rate = 200;
        int on_rate = 20000;
        double total_time;
        string command = "";
        private void Form1_Load(object sender, EventArgs e)
        {


            total_time = 0;
            picBox = pictureBox1;
            bitmap = new Bitmap(picBox.Width, picBox.Height);
            g = Graphics.FromImage(bitmap);
            string[] rates = null;
            try
            {
                rates = File.ReadAllLines("rates.txt");
            }
            catch
            {
                log("Put the file rates.txt in the application folder and restart this application", Color.Coral);
                goto nthing;
            }
            command = rates[2];
            if (rates.Length == 4)
            {
                command = rates[2] + "|" + rates[3];
            }
            on_rate = Convert.ToInt32(rates[0].Split(' ')[1]);
            log("on_rate = " + Convert.ToString(on_rate), Color.LightPink);
            off_rate = Convert.ToInt32(rates[1].Split(' ')[1]);
            log("off_rate = " + Convert.ToString(off_rate), Color.LightBlue);

            is_mining_started = false;

            System.Timers.Timer Router = new System.Timers.Timer();
            Router.Elapsed += new ElapsedEventHandler(router_check);
            Router.AutoReset = true;
            Router.Interval = 600000;
            Router.Start();
            nthing:
            System.Timers.Timer One_sec = new System.Timers.Timer();
            One_sec.Elapsed += new ElapsedEventHandler(one_sec);
            One_sec.AutoReset = true;
            One_sec.Interval = 1000;
            One_sec.Start();

            Ushki_na_makushke = new System.Timers.Timer();
            Ushki_na_makushke.Elapsed += new ElapsedEventHandler(ushki_na_makushke);
            Ushki_na_makushke.AutoReset = true;
            int interval = on_rate;
            Ushki_na_makushke.Interval = interval;
            Ushki_na_makushke.Start();
        }

        private void HandleConsoleSignal(ConsoleSignal consoleSignal)
        {
            mining_stop();
        }
        internal delegate void SignalHandler(ConsoleSignal consoleSignal);

        internal enum ConsoleSignal
        {
            CtrlC = 0,
            CtrlBreak = 1,
            Close = 2,
            LogOff = 5,
            Shutdown = 6
        }

        void reload()
        {
            mining_stop();
            mining_start();
        }

        void kill_process(string name)
        {
            Process[] prs = Process.GetProcesses();
            int i = 0;
            for (; i < prs.Length; i++)
            {
                if (prs[i].ProcessName == name)
                {
                    M:
                    try
                    {
                        prs[i].Kill();
                        is_mining_started = false;
                        draw_STOP();
                        // System.Threading.Thread.Sleep(3000);
                    }
                    catch
                    {
                        goto M;
                    }
                }
            }
        }
        System.Timers.Timer Shuher;
        Point mouse_pos;
        Color[,] points;
        public void shuher(object source, EventArgs e)
        {
            if (!forcibly)
            {
                //война с мусором
                /* Int32 totalMemory = Convert.ToInt32(GC.GetTotalMemory(false));
               log(Convert.ToString(totalMemory));
                 if (totalMemory > 526200)
                 {
                   log("collect!!!");
                     GC.Collect();
                     GC.WaitForPendingFinalizers();
                 }*/
                Point newpoint = Cursor.Position;
                if (newpoint != mouse_pos && is_mining_started)
                {
                    stringDelegate = new AutoMiner.StringDelegate(log);
                    richTextBox1.Invoke(stringDelegate, "mouse moving", Color.LightBlue);
                    mining_pause();
                    goto e;

                }
                mouse_pos = Cursor.Position;

                if (capture_image_is_moving() && is_mining_started)
                {
                    stringDelegate = new AutoMiner.StringDelegate(log);
                    richTextBox1.Invoke(stringDelegate, "screen changing", Color.LightBlue);
                    mining_pause();
                    goto e;
                }
                e:;
            }
        }
        System.Timers.Timer Ushki_na_makushke;
        bool is_mining_started;
        public void ushki_na_makushke(object source, EventArgs e)
        {
            if (!forcibly)
            {
                Point newpoint = Cursor.Position;
                if (is_mining_started == false)
                {
                    if (newpoint == mouse_pos)
                    {
                        if (capture_image_is_moving() == false)
                        {
                            mining_continue();
                        }
                    }
                }
            }
            mouse_pos = Cursor.Position;
        }
        bool is_mining_started_checker()
        {
            bool check_for_mining = false;
            Process[] prs = Process.GetProcesses();
            for (int i = 0; i < prs.Length; i++)
            {
                if (prs[i].ProcessName == "miner")
                {
                    check_for_mining = true;
                }
            }

            if (check_for_mining)
                return true;
            else
                return false;

        }
        string mining_start()
        {
            if (!is_mining_started_checker())
            {
                if (command.IndexOf("|") != -1)
                {

                    System.Diagnostics.Process procBCN = new System.Diagnostics.Process();
                    procBCN.StartInfo.FileName = "bcn_miner.exe";
                    //proc.StartInfo.Arguments = "-U - S etc.pool.minergate.com:45777 - u anton_dyrdin @mail.ru";
                    procBCN.StartInfo.Arguments = command.Split('|')[1];
                    // proc.StartInfo.Arguments = "-G -S etc.pool.minergate.com:45777 -u anton_dyrdin@mail.ru";
                    if (!forcibly)
                    {
                        procBCN.StartInfo.RedirectStandardOutput = true;
                        procBCN.StartInfo.UseShellExecute = false;
                        procBCN.StartInfo.CreateNoWindow = true;
                    }
                    procBCN.Start();
                }

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "miner.exe";
                // proc.StartInfo.Arguments = "-o stratum+tcp://bcn.pool.minergate.com:45550 -u anton_dyrdin@mail.ru -p x";
                //proc.StartInfo.Arguments = "-U - S etc.pool.minergate.com:45777 - u anton_dyrdin @mail.ru";
                //proc.StartInfo.Arguments = "-o stratum+tcp://xmr.pool.minergate.com:45560 -u anton_dyrdin@mail.ru -p x";

                proc.StartInfo.Arguments = command.Split('|')[0];

                //     proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                if (!forcibly)
                {
                    Shuher = new System.Timers.Timer();

                    Shuher.Elapsed += new ElapsedEventHandler(shuher);
                    Shuher.AutoReset = true;
                    int interval = off_rate;
                    Shuher.Interval = interval;
                    Shuher.Start();
                }
                if (!forcibly)
                {
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;
                }
                try
                {
                    proc.Start();
                    is_mining_started = true;
                    draw_START();

                    return "mining is started";

                }
                catch
                {
                    log("failed to start mining");
                    return "failed to start mining";
                }
            }
            else
            {

                log("Exeption: mining is alredy started");


                return "Exeption!(may be mining is alredy started or miner.exe is losted)";
            }
        }
        void mining_pause()
        {
            if (!forcibly)
            {
                kill_process("miner");
                Shuher.Dispose();
                Shuher.AutoReset = false;
                Shuher.Stop();

                Ushki_na_makushke = new System.Timers.Timer();
                Ushki_na_makushke.Elapsed += new ElapsedEventHandler(ushki_na_makushke);
                Ushki_na_makushke.AutoReset = true;
                int interval = on_rate;
                Ushki_na_makushke.Interval = interval;
                Ushki_na_makushke.Start();
                //GC.KeepAlive(Ushki_na_makushke);

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        void mining_continue()
        {
            if (!forcibly)
            {
                is_mining_started = true;
                Ushki_na_makushke.Dispose();
                Ushki_na_makushke.AutoReset = false;
                Ushki_na_makushke.Stop();
                mining_start();
            }

        }
        string mining_stop()
        {
            Q:
            try
            {
                kill_process("miner");

                if (command.IndexOf("|") != -1)
                    kill_process("bcn_miner");
            }
            catch
            {
                goto Q;
            }

            is_mining_started = false;
            if (Shuher != null)
            {
                Shuher.AutoReset = false;
                Shuher.Stop();
            }
            //     log("mining is stopped");
            return "mining is stopped";
        }

        bool capture_image_is_moving()
        {
            if (points == null)
                points = new Color[3, 3];

            bool ret = false;
            System.Drawing.Bitmap BM = new System.Drawing.Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            System.Drawing.Bitmap BM1 = new System.Drawing.Bitmap(1, 1);
            System.Drawing.Graphics GH = System.Drawing.Graphics.FromImage(BM1 as System.Drawing.Image);

            int incI = 0;
            int incJ = 0;
            for (int i = BM.Width / 6; i < BM.Width; i = i + BM.Width / 3)
            {
                incJ = 0;
                for (int j = BM.Height / 6; j < BM.Height; j = j + BM.Height / 3)
                {
                    GH.CopyFromScreen(i, j, 0, 0, BM1.Size);
                    Color col = BM1.GetPixel(0, 0);
                    if (points[incI, incJ] != col)
                        ret = true;
                    incJ++;
                }
                incI++;
            }

            incI = 0;
            incJ = 0;
            for (int i = BM.Width / 6; i < BM.Width; i = i + BM.Width / 3)
            {
                incJ = 0;
                for (int j = BM.Height / 6; j < BM.Height; j = j + BM.Height / 3)
                {
                    GH.CopyFromScreen(i, j, 0, 0, BM1.Size);
                    points[incI, incJ] = BM1.GetPixel(0, 0);

                    incJ++;
                }
                incI++;
            }
            GH.Dispose();

            return ret;
        }
        public void log(String s, Color col)
        {
            richTextBox1.SelectionColor = col;
            richTextBox1.AppendText(s + '\n');
            richTextBox1.SelectionColor = Color.White;


            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.ScrollToCaret();
        }
        public void log(String s)
        {
            richTextBox1.AppendText(s + '\n');
        }

        public void one_sec(object source, EventArgs e)
        {
            /*  if (total_time < 86400.0 / 240.0)
              {   */
            total_time = total_time + 1.0 / 240.0;
            /*       }
                   else
                   {                  
                       notifyIcon1.Visible = false;

                       mining_stop();
                       System.Diagnostics.Process proc = new System.Diagnostics.Process();
                       proc.StartInfo.FileName = "AutoMiner.exe";
                       proc.Start();
                       Environment.Exit(0);
                   }  */
        }
        double time_of_start = 0;
        double time_of_stop = 0;
        double mining_time = 0;
        int day = 0;
        void draw_START()
        {

            time_of_start = total_time;

            stringDelegate = new AutoMiner.StringDelegate(log);
            richTextBox1.Invoke(stringDelegate, DateTime.Now.TimeOfDay.ToString().Substring(0, 8) + "  started", Color.MediumSpringGreen);

            drawDelegate = new AutoMiner.DrawDelegate(drawLine);
            picBox.Invoke(drawDelegate, Color.Black, time_of_stop, 10 + day * 20 + day, total_time, 10 + day * 20 + day);
        }
        void draw_STOP()
        {
            time_of_stop = total_time;
            mining_time = (time_of_stop - time_of_start);
            stringDelegate = new AutoMiner.StringDelegate(log);
            richTextBox1.Invoke(stringDelegate, DateTime.Now.TimeOfDay.ToString().Substring(0, 8) + "  stop", Color.SkyBlue);
            var ts = TimeSpan.FromSeconds(Math.Round(mining_time * 240.0));
            richTextBox1.Invoke(stringDelegate, " mining time = " + ts.ToString().Substring(0, 8), Color.White);
            drawDelegate = new AutoMiner.DrawDelegate(drawLine);
            picBox.Invoke(drawDelegate, Color.FromArgb(255, 0, 255, 0), time_of_start, 10 + day * 20 + day, total_time, 10 + day * 20 + day);
        }
        public void drawLine(Color col, double x1, double y1, double x2, double y2)
        {

            g.DrawLine(new Pen(col, 40), Convert.ToInt16(Math.Round(x1)), Convert.ToInt16(Math.Round(y1)), Convert.ToInt16(Math.Round(x2)), Convert.ToInt16(Math.Round(y2)));
            picBox.Image = bitmap;
            picBox.Refresh();
        }
        bool forcibly = false;




        private void button1_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            mining_stop();
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = System.IO.Path.GetFileName(Application.ExecutablePath);
            proc.Start();
            Environment.Exit(0);
        }

        private void AutoMiner_Activated(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }
        public void drawString(string s, int depth, double x, double y)
        {
            g.DrawString(s, new Font("Consolas", depth), Brushes.White, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
        }
        public void router_check(object source, EventArgs e)
        {
            try
            {
                getResponse("https://www.google.ru");
            }
            catch
            {
                try
                {
                    getResponse("https://www.yandex.ru");
                }
                catch
                {
                    stringDelegate = new AutoMiner.StringDelegate(log);
                    richTextBox1.Invoke(stringDelegate, "Reboot Router", Color.Coral);

                    System.IO.Ports.SerialPort serialPort1 = new System.IO.Ports.SerialPort();
                    serialPort1.PortName = "COM3";
                    serialPort1.BaudRate = 9600;
                    serialPort1.Open();
                    serialPort1.Write("reboot");
                    serialPort1.Close();

                    mining_stop();
                    stringDelegate = new AutoMiner.StringDelegate(log);
                    richTextBox1.Invoke(stringDelegate, "wait 1 minute before start again...", Color.Coral);

                    System.Threading.Thread.Sleep(60000);
                    mining_start();

                    /* ProcessStartInfo startInfo = new ProcessStartInfo();
                     startInfo.FileName = "cmd";
                     startInfo.Arguments = "D:\\Reboot Router.bat";
                     startInfo.CreateNoWindow = true;
                     Process proc = Process.Start("cmd", "D:\\Reboot Router.bat");   */
                }
            }
        }

        static public string getResponse(string url)
        {
            string ret = "";
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        ret = ret + line;
                    }
                }
            }
            response.Close();
            return ret;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            forcibly = true;
            mining_start();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            forcibly = false;
            mining_stop();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            stringDelegate = new AutoMiner.StringDelegate(log);
            richTextBox1.Invoke(stringDelegate, "Reboot Router", Color.Coral);

            System.IO.Ports.SerialPort serialPort1 = new System.IO.Ports.SerialPort();
            serialPort1.PortName = "COM3";
            serialPort1.BaudRate = 9600;
            serialPort1.Open();
            serialPort1.Write("reboot");
            serialPort1.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            mining_stop();
            notifyIcon1.Visible = false;
            Environment.Exit(0);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
