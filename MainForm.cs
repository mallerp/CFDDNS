using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CFDDNS
{
    public partial class MainForm : Form
    {
        private AppConfig _config = null!;
        private BindingList<DomainConfig> _domainBindingList = null!;
        private readonly System.Windows.Forms.Timer _updateTimer;
        private bool _isServiceRunning = false;
        private bool _isUpdating = false;

        public MainForm()
        {
            InitializeComponent();
            LoadConfigAndBindData();
            SetupDataGridView();

            _updateTimer = new System.Windows.Forms.Timer();
            _updateTimer.Tick += async (s, e) => await UpdateDnsRecordsAsync();

            _ = InitializeDomainStatesAsync();
        }

        private void LoadConfigAndBindData()
        {
            Log("正在加载配置...");
            try
            {
                _config = ConfigManager.LoadConfig();
                _domainBindingList = new BindingList<DomainConfig>(_config.Domains);
                dgvDomains.DataSource = _domainBindingList;
                Log("配置加载完毕。");
            }
            catch (Exception ex)
            {
                Log($"加载配置失败: {ex.Message}");
                MessageBox.Show($"加载配置文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDataGridView()
        {
            dgvDomains.AutoGenerateColumns = false;
            dgvDomains.Columns.Clear();

            dgvDomains.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "Enabled",
                HeaderText = "启用",
                Name = "Enabled",
                Width = 40
            });

            dgvDomains.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Domain",
                HeaderText = "域名",
                Name = "Domain",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvDomains.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Type",
                HeaderText = "类型",
                Name = "Type",
                Width = 50
            });

            dgvDomains.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CurrentRecordIp",
                HeaderText = "当前记录值",
                Name = "CurrentRecordIp",
                Width = 120,
                ReadOnly = true
            });

            dgvDomains.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Status",
                HeaderText = "状态",
                Name = "Status",
                Width = 120,
                ReadOnly = true
            });

            dgvDomains.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LastUpdated",
                HeaderText = "上次更新",
                Name = "LastUpdated",
                Width = 140,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" },
                ReadOnly = true
            });
        }

        private void Log(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(Log), message);
                return;
            }
            txtLog.AppendText($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
        }

        private async Task UpdateDnsRecordsAsync(bool forceUpdate = false)
        {
            if (_isUpdating)
            {
                Log("更新任务已在运行中，请稍后再试。");
                return;
            }

            if (string.IsNullOrWhiteSpace(_config.Global.Email) || string.IsNullOrWhiteSpace(_config.Global.ApiKey))
            {
                Log("请先在'全局设置'中配置您的 Email 和 API Key。");
                if (_isServiceRunning)
                {
                    BtnToggleService_Click(this, EventArgs.Empty); // Stop the service
                }
                return;
            }

            _isUpdating = true;
            btnForceUpdate.Enabled = false;
            Log("开始检查并更新 DNS 记录...");

            try
            {
                string? ipv4 = null;
                string? ipv6 = null;

                var domainsToUpdate = _domainBindingList.Where(d => d.Enabled).ToList();

                if (!domainsToUpdate.Any())
                {
                    Log("没有已启用的域名，跳过更新。");
                    _isUpdating = false;
                    btnForceUpdate.Enabled = true;
                    return;
                }

                if (domainsToUpdate.Any(d => d.Type == "A"))
                {
                    ipv4 = await IpService.GetPublicIpv4Async();
                    if (string.IsNullOrEmpty(ipv4)) Log("获取 IPv4 地址失败。");
                    else Log($"获取到当前 IPv4 地址: {ipv4}");
                }

                if (domainsToUpdate.Any(d => d.Type == "AAAA"))
                {
                    ipv6 = await IpService.GetPublicIpv6Async();
                    if (string.IsNullOrEmpty(ipv6)) Log("获取 IPv6 地址失败。");
                    else Log($"获取到当前 IPv6 地址: {ipv6}");
                }

                UpdateIpStatusLabel(ipv4, ipv6);
                
                var globalClient = new CloudflareClient(_config.Global.Email, _config.Global.ApiKey);

                foreach (var domain in domainsToUpdate)
                {
                    var email = domain.Email ?? _config.Global.Email;
                    var apiKey = domain.ApiKey ?? _config.Global.ApiKey;

                    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(apiKey))
                    {
                        Log($"域名 {domain.Domain} 跳过更新，因为未配置有效的 Email 或 API Key (全局或独立)。");
                        continue;
                    }
                    
                    var client = (email == _config.Global.Email && apiKey == _config.Global.ApiKey)
                        ? globalClient
                        : new CloudflareClient(email, apiKey);

                    // Get current record from Cloudflare
                    var (getSuccess, currentIp, getMessage) = await client.GetDnsRecordIpAsync(domain);
                    domain.CurrentRecordIp = getSuccess ? currentIp : "查询失败";
                    _domainBindingList.ResetItem(_domainBindingList.IndexOf(domain));
                    if (!getSuccess)
                    {
                        Log($"查询域名 {domain.Domain} 的当前IP失败: {getMessage}");
                        continue;
                    }

                    string? targetIp = domain.Type == "A" ? ipv4 : ipv6;

                    if (string.IsNullOrEmpty(targetIp))
                    {
                        Log($"域名 {domain.Domain} ({domain.Type}) 跳过更新，因为未能获取到相应的公网 IP 地址。");
                        continue;
                    }

                    if (!forceUpdate && targetIp == domain.CurrentRecordIp)
                    {
                        Log($"域名 {domain.Domain} 的 IP 地址 ({targetIp}) 未发生变化，跳过更新。");
                        domain.Status = "IP未变化";
                        _domainBindingList.ResetItem(_domainBindingList.IndexOf(domain));
                        continue;
                    }

                    domain.Status = "更新中...";
                    _domainBindingList.ResetItem(_domainBindingList.IndexOf(domain));

                    var (success, message) = await client.UpdateDnsRecordAsync(domain, targetIp);
                    Log(message);
                    
                    if (success)
                    {
                        domain.Status = "更新成功";
                        domain.LastKnownIp = targetIp;
                        domain.LastUpdated = DateTime.Now;
                        ConfigManager.SaveConfig(_config);
                    }
                    else
                    {
                        domain.Status = "更新失败";
                    }
                    _domainBindingList.ResetItem(_domainBindingList.IndexOf(domain));
                }
            }
            catch (Exception ex)
            {
                Log($"更新过程中发生未预料的错误: {ex.Message}");
            }
            finally
            {
                _isUpdating = false;
                btnForceUpdate.Enabled = true;
                Log("DNS 记录检查更新完成。");
            }
        }

        private async Task InitializeDomainStatesAsync()
        {
            Log("正在初始化域名状态...");
            await UpdateIpStatusAsync(); // Fetches public IPs and updates status bar

            string? ipv4 = null;
            string? ipv6 = null;

            if (lblPublicIp.Tag is not null)
            {
                var tagType = lblPublicIp.Tag.GetType();
                ipv4 = tagType.GetProperty("Ipv4")?.GetValue(lblPublicIp.Tag, null) as string;
                ipv6 = tagType.GetProperty("Ipv6")?.GetValue(lblPublicIp.Tag, null) as string;
            }

            var domains = _domainBindingList.ToList();
            var globalClient = new CloudflareClient(_config.Global.Email, _config.Global.ApiKey);

            foreach (var domain in domains)
            {
                var email = domain.Email ?? _config.Global.Email;
                var apiKey = domain.ApiKey ?? _config.Global.ApiKey;
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(apiKey)) continue;

                var client = (email == _config.Global.Email && apiKey == _config.Global.ApiKey) ? globalClient : new CloudflareClient(email, apiKey);

                var (getSuccess, currentIp, getMessage) = await client.GetDnsRecordIpAsync(domain);
                domain.CurrentRecordIp = getSuccess ? currentIp : "查询失败";

                if (getSuccess)
                {
                    string? publicIp = domain.Type == "A" ? ipv4 : ipv6;
                    domain.Status = publicIp == currentIp ? "IP一致" : "待更新";
                }
                else
                {
                    domain.Status = "状态未知";
                }
                _domainBindingList.ResetItem(_domainBindingList.IndexOf(domain));
            }
            Log("域名状态初始化完成。");
        }

        private async Task UpdateIpStatusAsync()
        {
            Log("正在获取公网IP地址...");
            string? ipv4 = await IpService.GetPublicIpv4Async();
            string? ipv6 = await IpService.GetPublicIpv6Async();
            UpdateIpStatusLabel(ipv4, ipv6);
            Log("获取公网IP地址完成。");
        }

        private void UpdateIpStatusLabel(string? ipv4, string? ipv6)
        {
            var ipStatus = "公网 IP: ";
            if (!string.IsNullOrEmpty(ipv4))
            {
                ipStatus += $"IPv4: {ipv4}";
            }
            if (!string.IsNullOrEmpty(ipv6))
            {
                if (!string.IsNullOrEmpty(ipv4)) ipStatus += " / ";
                ipStatus += $"IPv6: {ipv6}";
            }
            if (string.IsNullOrEmpty(ipv4) && string.IsNullOrEmpty(ipv6))
            {
                ipStatus = "未能获取公网IP";
            }

            if (statusStrip1.InvokeRequired)
            {
                statusStrip1.Invoke(() => {
                    lblPublicIp.Text = ipStatus;
                    lblPublicIp.Tag = new { Ipv4 = ipv4, Ipv6 = ipv6 };
                });
            }
            else
            {
                lblPublicIp.Text = ipStatus;
                lblPublicIp.Tag = new { Ipv4 = ipv4, Ipv6 = ipv6 };
            }
        }

        private void DgvDomains_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvDomains.Columns["Enabled"].Index)
            {
                dgvDomains.EndEdit(); // Commit the change
                ConfigManager.SaveConfig(_config);
                Log("配置已保存。");
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var newDomain = new DomainConfig { Domain = "sub.example.com" };
            using (var editForm = new EditDomainForm(newDomain))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    _domainBindingList.Add(newDomain);
                    ConfigManager.SaveConfig(_config);
                    Log($"已添加新域名: {newDomain.Domain}");
                }
            }
        }

        private void EditSelectedDomain()
        {
            if (dgvDomains.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择一个要编辑的域名。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var domainToEdit = (DomainConfig)dgvDomains.SelectedRows[0].DataBoundItem;
            using (var editForm = new EditDomainForm(domainToEdit))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    _domainBindingList.ResetItem(_domainBindingList.IndexOf(domainToEdit));
                    ConfigManager.SaveConfig(_config);
                    Log($"已更新域名: {domainToEdit.Domain}");
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            EditSelectedDomain();
        }

        private void DgvDomains_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                EditSelectedDomain();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvDomains.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择一个要删除的域名。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var domainToDelete = (DomainConfig)dgvDomains.SelectedRows[0].DataBoundItem;
            var result = MessageBox.Show($"您确定要删除域名 {domainToDelete.Domain} 吗?", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                _domainBindingList.Remove(domainToDelete);
                ConfigManager.SaveConfig(_config);
                Log($"已删除域名: {domainToDelete.Domain}");
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm(_config.Global))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    ConfigManager.SaveConfig(_config);
                    Log("全局设置已保存。");

                    if (_isServiceRunning)
                    {
                        _updateTimer.Stop();
                        _updateTimer.Interval = Math.Max(1, _config.Global.UpdateIntervalMinutes) * 60 * 1000;
                        _updateTimer.Start();
                        Log("服务已根据新的时间间隔重启。");
                    }
                }
            }
        }

        private async void BtnForceUpdate_Click(object sender, EventArgs e)
        {
            await UpdateDnsRecordsAsync(true);
        }

        private void BtnToggleService_Click(object sender, EventArgs e)
        {
            _isServiceRunning = !_isServiceRunning;
            if (_isServiceRunning)
            {
                Log("启动自动更新服务...");
                _updateTimer.Interval = Math.Max(1, _config.Global.UpdateIntervalMinutes) * 60 * 1000;
                _updateTimer.Start();
                btnToggleService.Text = "停止服务";
                Task.Run(() => UpdateDnsRecordsAsync(true));
            }
            else
            {
                Log("停止自动更新服务。");
                _updateTimer.Stop();
                btnToggleService.Text = "启动服务";
            }
        }
    }

    #region Forms Defined In-File as Workaround

    public partial class SettingsForm : Form
    {
        private readonly GlobalSettings _settings;
        private Label lblEmail = new();
        private TextBox txtEmail = new();
        private Label lblApiKey = new();
        private TextBox txtApiKey = new();
        private Label lblUpdateInterval = new();
        private NumericUpDown numUpdateInterval = new();
        private Button btnSave = new();
        private Button btnCancel = new();

        public SettingsForm(GlobalSettings settings)
        {
            InitializeComponent();
            _settings = settings;
            txtEmail.Text = _settings.Email;
            txtApiKey.Text = _settings.ApiKey;
            numUpdateInterval.Value = Math.Max(1, _settings.UpdateIntervalMinutes);
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@"))
            {
                MessageBox.Show("请输入有效的邮箱地址。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtApiKey.Text))
            {
                MessageBox.Show("API Key 不能为空。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _settings.Email = txtEmail.Text;
            _settings.ApiKey = txtApiKey.Text;
            _settings.UpdateIntervalMinutes = (int)numUpdateInterval.Value;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void InitializeComponent()
        {
            ((System.ComponentModel.ISupportInitialize)(this.numUpdateInterval)).BeginInit(); this.SuspendLayout();
            this.lblEmail.AutoSize = true; this.lblEmail.Location = new System.Drawing.Point(25, 28); this.lblEmail.Text = "Cloudflare 邮箱:";
            this.txtEmail.Location = new System.Drawing.Point(140, 25); this.txtEmail.Name = "txtEmail"; this.txtEmail.Size = new System.Drawing.Size(280, 23);
            this.lblApiKey.AutoSize = true; this.lblApiKey.Location = new System.Drawing.Point(25, 71); this.lblApiKey.Text = "Global API Key:";
            this.txtApiKey.Location = new System.Drawing.Point(140, 68); this.txtApiKey.PasswordChar = '*'; this.txtApiKey.Size = new System.Drawing.Size(280, 23);
            this.lblUpdateInterval.AutoSize = true; this.lblUpdateInterval.Location = new System.Drawing.Point(25, 114); this.lblUpdateInterval.Text = "更新间隔 (分钟):";
            this.numUpdateInterval.Location = new System.Drawing.Point(140, 112); this.numUpdateInterval.Maximum = 1440; this.numUpdateInterval.Minimum = 1; this.numUpdateInterval.Size = new System.Drawing.Size(120, 23);
            this.btnSave.Location = new System.Drawing.Point(240, 160); this.btnSave.Text = "保存"; this.btnSave.Click += new EventHandler(this.btnSave_Click);
            this.btnCancel.Location = new System.Drawing.Point(335, 160); this.btnCancel.Text = "取消"; this.btnCancel.DialogResult = DialogResult.Cancel;
            this.ClientSize = new System.Drawing.Size(450, 210);
            this.Controls.AddRange(new Control[] { this.btnCancel, this.btnSave, this.numUpdateInterval, this.lblUpdateInterval, this.txtApiKey, this.lblApiKey, this.txtEmail, this.lblEmail });
            this.FormBorderStyle = FormBorderStyle.FixedDialog; this.MaximizeBox = false; this.MinimizeBox = false; this.StartPosition = FormStartPosition.CenterParent; this.Text = "全局设置";
            ((System.ComponentModel.ISupportInitialize)(this.numUpdateInterval)).EndInit(); this.ResumeLayout(false); this.PerformLayout();
        }
    }

    public partial class EditDomainForm : Form
    {
        private readonly DomainConfig _domain;
        private Label lblDomain = new();
        private TextBox txtDomain = new();
        private Label lblType = new();
        private ComboBox cmbType = new();
        private Label lblZoneId = new();
        private TextBox txtZoneId = new();
        private Label lblRecordId = new();
        private TextBox txtRecordId = new();
        private Label lblEmailOverride = new();
        private TextBox txtEmailOverride = new();
        private Label lblApiKeyOverride = new();
        private TextBox txtApiKeyOverride = new();
        private Button btnSave = new();
        private Button btnCancel = new();

        public EditDomainForm(DomainConfig domain)
        {
            InitializeComponent();
            _domain = domain;
            txtDomain.Text = _domain.Domain;
            cmbType.SelectedItem = _domain.Type ?? "A";
            txtZoneId.Text = _domain.ZoneId;
            txtRecordId.Text = _domain.RecordId;
            txtEmailOverride.Text = _domain.Email;
            txtApiKeyOverride.Text = _domain.ApiKey;
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDomain.Text) || string.IsNullOrWhiteSpace(txtZoneId.Text) || string.IsNullOrWhiteSpace(txtRecordId.Text) || cmbType.SelectedItem == null)
            {
                MessageBox.Show("域名、类型、Zone ID 和 Record ID 不能为空。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _domain.Domain = txtDomain.Text;
            _domain.Type = cmbType.SelectedItem.ToString() ?? "A";
            _domain.ZoneId = txtZoneId.Text;
            _domain.RecordId = txtRecordId.Text;
            _domain.Email = string.IsNullOrWhiteSpace(txtEmailOverride.Text) ? null : txtEmailOverride.Text;
            _domain.ApiKey = string.IsNullOrWhiteSpace(txtApiKeyOverride.Text) ? null : txtApiKeyOverride.Text;

            DialogResult = DialogResult.OK;
            Close();
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.lblDomain.Text = "子域名:"; this.lblDomain.Location = new System.Drawing.Point(25, 28); this.lblDomain.AutoSize = true;
            this.lblType.Text = "类型:"; this.lblType.Location = new System.Drawing.Point(25, 71); this.lblType.AutoSize = true;
            this.lblZoneId.Text = "Zone ID:"; this.lblZoneId.Location = new System.Drawing.Point(25, 114); this.lblZoneId.AutoSize = true;
            this.lblRecordId.Text = "Record ID:"; this.lblRecordId.Location = new System.Drawing.Point(25, 157); this.lblRecordId.AutoSize = true;
            
            this.lblEmailOverride.Text = "邮箱 (覆盖全局):"; this.lblEmailOverride.Location = new System.Drawing.Point(25, 200); this.lblEmailOverride.AutoSize = true;
            this.lblApiKeyOverride.Text = "API Key (覆盖全局):"; this.lblApiKeyOverride.Location = new System.Drawing.Point(25, 243); this.lblApiKeyOverride.AutoSize = true;

            this.txtDomain.Location = new System.Drawing.Point(160, 25); this.txtDomain.Size = new System.Drawing.Size(280, 23);
            this.cmbType.Location = new System.Drawing.Point(160, 68); this.cmbType.Items.AddRange(new object[] { "A", "AAAA" }); this.cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.txtZoneId.Location = new System.Drawing.Point(160, 111); this.txtZoneId.Size = new System.Drawing.Size(280, 23);
            this.txtRecordId.Location = new System.Drawing.Point(160, 154); this.txtRecordId.Size = new System.Drawing.Size(280, 23);
            
            this.txtEmailOverride.Location = new System.Drawing.Point(160, 197); this.txtEmailOverride.Size = new System.Drawing.Size(280, 23);
            this.txtEmailOverride.PlaceholderText = "留空则使用全局设置";
            this.txtApiKeyOverride.Location = new System.Drawing.Point(160, 240); this.txtApiKeyOverride.Size = new System.Drawing.Size(280, 23);
            this.txtApiKeyOverride.PlaceholderText = "留空则使用全局设置";
            this.txtApiKeyOverride.PasswordChar = '*';

            this.btnSave.Text = "保存"; this.btnSave.Location = new System.Drawing.Point(250, 290); this.btnSave.Click += new EventHandler(this.btnSave_Click);
            this.btnCancel.Text = "取消"; this.btnCancel.Location = new System.Drawing.Point(345, 290); this.btnCancel.DialogResult = DialogResult.Cancel;
            
            this.ClientSize = new System.Drawing.Size(480, 340);
            this.Controls.AddRange(new Control[] { 
                this.lblDomain, this.txtDomain, this.lblType, this.cmbType, 
                this.lblZoneId, this.txtZoneId, this.lblRecordId, this.txtRecordId, 
                this.lblEmailOverride, this.txtEmailOverride, this.lblApiKeyOverride, this.txtApiKeyOverride,
                this.btnSave, this.btnCancel });
            this.FormBorderStyle = FormBorderStyle.FixedDialog; this.MaximizeBox = false; this.MinimizeBox = false; this.StartPosition = FormStartPosition.CenterParent; this.Text = "编辑域名";
            this.ResumeLayout(false); this.PerformLayout();
        }
    }

    #endregion
} 