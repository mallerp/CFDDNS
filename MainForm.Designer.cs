namespace CFDDNS
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvDomains;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnToggleService;
        private System.Windows.Forms.Button btnForceUpdate;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblPublicIp;

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
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "Cloudflare DDNS Client";

            // Main Layout
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.dgvDomains = new System.Windows.Forms.DataGridView();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblPublicIp = new System.Windows.Forms.ToolStripStatusLabel();

            // Buttons
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnForceUpdate = new System.Windows.Forms.Button();
            this.btnToggleService = new System.Windows.Forms.Button();
            
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDomains)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();

            // tableLayoutPanel1
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dgvDomains, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtLog, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            
            // flowLayoutPanel1
            this.flowLayoutPanel1.Controls.Add(this.btnAdd);
            this.flowLayoutPanel1.Controls.Add(this.btnEdit);
            this.flowLayoutPanel1.Controls.Add(this.btnDelete);
            this.flowLayoutPanel1.Controls.Add(this.btnSettings);
            this.flowLayoutPanel1.Controls.Add(this.btnForceUpdate);
            this.flowLayoutPanel1.Controls.Add(this.btnToggleService);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(5);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";

            // dgvDomains
            this.dgvDomains.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDomains.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDomains.Name = "dgvDomains";
            this.dgvDomains.RowTemplate.Height = 25;
            this.dgvDomains.AllowUserToAddRows = false;
            this.dgvDomains.AllowUserToDeleteRows = false;
            this.dgvDomains.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDomains.MultiSelect = false;
            this.dgvDomains.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvDomains_CellDoubleClick);
            this.dgvDomains.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DgvDomains_CellContentClick);

            // txtLog
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;

            // Buttons Text and Size
            this.btnAdd.Name = "btnAdd"; this.btnAdd.Text = "添加域名"; this.btnAdd.Size = new System.Drawing.Size(80, 25); this.btnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            this.btnEdit.Name = "btnEdit"; this.btnEdit.Text = "编辑域名"; this.btnEdit.Size = new System.Drawing.Size(80, 25); this.btnEdit.Click += new System.EventHandler(this.BtnEdit_Click);
            this.btnDelete.Name = "btnDelete"; this.btnDelete.Text = "删除域名"; this.btnDelete.Size = new System.Drawing.Size(80, 25); this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            this.btnSettings.Name = "btnSettings"; this.btnSettings.Text = "全局设置"; this.btnSettings.Size = new System.Drawing.Size(80, 25); this.btnSettings.Click += new System.EventHandler(this.BtnSettings_Click);
            this.btnForceUpdate.Name = "btnForceUpdate"; this.btnForceUpdate.Text = "立即更新"; this.btnForceUpdate.Size = new System.Drawing.Size(80, 25); this.btnForceUpdate.Click += new System.EventHandler(this.BtnForceUpdate_Click);
            this.btnToggleService.Name = "btnToggleService"; this.btnToggleService.Text = "启动服务"; this.btnToggleService.Size = new System.Drawing.Size(80, 25); this.btnToggleService.Click += new System.EventHandler(this.BtnToggleService_Click);

            // statusStrip1
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.lblPublicIp });
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.lblPublicIp.Name = "lblPublicIp";
            this.lblPublicIp.Text = "正在获取公网IP...";

            // Add Controls to Form
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);

            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDomains)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
} 