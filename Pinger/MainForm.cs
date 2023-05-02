using Pinger.Services.Network;
using System.Data;

namespace Pinger
{
    public partial class MainForm : Form
    {
        private readonly NetScan scanner = new NetScan();
        private DataTable pingResults = new DataTable();

        public MainForm()
        {
            InitializeComponent();
            pingResults.Columns.AddRange(new DataColumn[] {
                        new DataColumn("IP", typeof(string)),
                        new DataColumn("MAC-Адрес",typeof(string)),
                        new DataColumn("Имя компьютера",typeof(string)),
                        new DataColumn("Сообщение",typeof(string))});
            gridView1.DataSource = pingResults;
            gridView1.Update();
        }

        private async void StartBtn_Click(object sender, EventArgs e)
        {
            StartBtn.Enabled = false;
            StartBtn.Text = "Сканирую";

            try
            {
                pingResults.Clear();
                var range = new IPRange(IpRangeTb.Text);
                var statuses = await scanner.ScanRange(range);

                lock (pingResults.Rows.SyncRoot)
                {
                    foreach (var stat in statuses)
                    {
                        pingResults.Rows.Add(stat.Address.ToString(), stat.Mac, stat.Name, stat.Status);
                    }
                }
                gridView1.Update();

                var red = Color.FromArgb(244, 67, 54);
                var green = Color.FromArgb(76, 175, 80);

                foreach (DataGridViewRow row in gridView1.Rows)
                {
                    if (row.Cells[3].Value.ToString() != "Success")
                    {
                        row.DefaultCellStyle.BackColor = red;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = green;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            StartBtn.Enabled = true;
            StartBtn.Text = "Сканировать";
        }
    }
}