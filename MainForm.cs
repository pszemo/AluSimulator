using System;
using System.Drawing;
using System.Windows.Forms;

namespace AluSimulator
{
    public class MainForm : Form
    {
        private readonly Alu _alu = new Alu();

        // Kontrolki wejścia
        private TextBox txtA = null!;
        private TextBox txtB = null!;
        private ComboBox cmbOperation = null!;
        private Button btnExecute = null!;
        private Button btnClear = null!;

        // Kontrolki wyjścia - wynik
        private TextBox txtResultDec = null!;
        private TextBox txtResultDecSigned = null!;
        private TextBox txtResultBin = null!;
        private TextBox txtResultHex = null!;

        // Kontrolki wyjścia - flagi
        private CheckBox chkZF = null!;
        private CheckBox chkCF = null!;
        private CheckBox chkSF = null!;
        private CheckBox chkOF = null!;

        // Historia
        private ListBox lstHistory = null!;
        private Label lblStatus = null!;

        public MainForm()
        {
            InitializeComponent();
            PopulateOperations();
        }

        private void InitializeComponent()
        {
            Text = "Symulator ALU 16-bit (U2)";
            Size = new Size(720, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(720, 620);

            // === GroupBox: Wejście ===
            var gbInput = new GroupBox
            {
                Text = "Operandy i operacja",
                Location = new Point(12, 12),
                Size = new Size(680, 130)
            };
            Controls.Add(gbInput);

            var lblA = new Label { Text = "Operand A:", Location = new Point(15, 30), AutoSize = true };
            txtA = new TextBox { Location = new Point(100, 27), Width = 180, Text = "10" };
            var lblAHint = new Label
            {
                Text = "(dec, 0x.., 0b.., same 0/1)",
                Location = new Point(290, 30),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            var lblB = new Label { Text = "Operand B:", Location = new Point(15, 60), AutoSize = true };
            txtB = new TextBox { Location = new Point(100, 57), Width = 180, Text = "3" };
            var lblBHint = new Label
            {
                Text = "(dla przesunięć: licznik 0-15)",
                Location = new Point(290, 60),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            var lblOp = new Label { Text = "Operacja:", Location = new Point(15, 90), AutoSize = true };
            cmbOperation = new ComboBox
            {
                Location = new Point(100, 87),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnExecute = new Button
            {
                Text = "Wykonaj",
                Location = new Point(480, 27),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnExecute.Click += BtnExecute_Click;

            btnClear = new Button
            {
                Text = "Wyczyść historię",
                Location = new Point(480, 72),
                Size = new Size(180, 28)
            };
            btnClear.Click += (s, e) => { lstHistory.Items.Clear(); };

            gbInput.Controls.AddRange(new Control[]
            {
                lblA, txtA, lblAHint, lblB, txtB, lblBHint,
                lblOp, cmbOperation, btnExecute, btnClear
            });

            // === GroupBox: Wynik ===
            var gbResult = new GroupBox
            {
                Text = "Wynik",
                Location = new Point(12, 150),
                Size = new Size(400, 180)
            };
            Controls.Add(gbResult);

            gbResult.Controls.Add(new Label { Text = "Dec (bez znaku):", Location = new Point(15, 28), AutoSize = true });
            txtResultDec = new TextBox { Location = new Point(140, 25), Width = 240, ReadOnly = true, BackColor = Color.White };
            gbResult.Controls.Add(txtResultDec);

            gbResult.Controls.Add(new Label { Text = "Dec (U2):", Location = new Point(15, 58), AutoSize = true });
            txtResultDecSigned = new TextBox { Location = new Point(140, 55), Width = 240, ReadOnly = true, BackColor = Color.White };
            gbResult.Controls.Add(txtResultDecSigned);

            gbResult.Controls.Add(new Label { Text = "Bin:", Location = new Point(15, 88), AutoSize = true });
            txtResultBin = new TextBox
            {
                Location = new Point(140, 85),
                Width = 240,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Consolas", 9.5F)
            };
            gbResult.Controls.Add(txtResultBin);

            gbResult.Controls.Add(new Label { Text = "Hex:", Location = new Point(15, 118), AutoSize = true });
            txtResultHex = new TextBox
            {
                Location = new Point(140, 115),
                Width = 240,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Consolas", 9.5F)
            };
            gbResult.Controls.Add(txtResultHex);

            lblStatus = new Label
            {
                Text = "Gotowy.",
                Location = new Point(15, 148),
                Size = new Size(370, 20),
                ForeColor = Color.DarkGreen
            };
            gbResult.Controls.Add(lblStatus);

            // === GroupBox: Flagi ===
            var gbFlags = new GroupBox
            {
                Text = "Flagi procesora",
                Location = new Point(422, 150),
                Size = new Size(270, 180)
            };
            Controls.Add(gbFlags);

            chkZF = MakeFlagCheckBox("ZF - Zero Flag", 30);
            chkCF = MakeFlagCheckBox("CF - Carry Flag", 60);
            chkSF = MakeFlagCheckBox("SF - Sign Flag", 90);
            chkOF = MakeFlagCheckBox("OF - Overflow Flag", 120);

            gbFlags.Controls.AddRange(new Control[] { chkZF, chkCF, chkSF, chkOF });

            var lblFlagHint = new Label
            {
                Text = "(CF - przeniesienie bez znaku,\n OF - przepełnienie w U2)",
                Location = new Point(15, 145),
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 7.5F)
            };
            gbFlags.Controls.Add(lblFlagHint);

            // === GroupBox: Historia ===
            var gbHistory = new GroupBox
            {
                Text = "Historia operacji",
                Location = new Point(12, 340),
                Size = new Size(680, 230),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            Controls.Add(gbHistory);

            lstHistory = new ListBox
            {
                Location = new Point(10, 22),
                Size = new Size(660, 200),
                Font = new Font("Consolas", 9F),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            gbHistory.Controls.Add(lstHistory);

            AcceptButton = btnExecute;
        }

        private CheckBox MakeFlagCheckBox(string text, int y)
        {
            return new CheckBox
            {
                Text = text,
                Location = new Point(15, y),
                AutoSize = true,
                AutoCheck = false,
                Font = new Font("Consolas", 10F)
            };
        }

        private void PopulateOperations()
        {
            foreach (Alu.Operation op in Enum.GetValues(typeof(Alu.Operation)))
            {
                cmbOperation.Items.Add(op);
            }
            cmbOperation.SelectedIndex = 0;
        }

        private void BtnExecute_Click(object? sender, EventArgs e)
        {
            if (!Alu.TryParse(txtA.Text, out ushort a))
            {
                ShowError("Nieprawidłowy operand A. Użyj: dec, 0x.., 0b.., same 0/1.");
                return;
            }
            if (!Alu.TryParse(txtB.Text, out ushort b))
            {
                ShowError("Nieprawidłowy operand B. Użyj: dec, 0x.., 0b.., same 0/1.");
                return;
            }

            var op = (Alu.Operation)cmbOperation.SelectedItem!;
            ushort result = _alu.Execute(op, a, b, out string description);

            // Wyświetlanie wyniku
            txtResultDec.Text = result.ToString();
            txtResultDecSigned.Text = Alu.ToSigned(result).ToString();
            txtResultBin.Text = Alu.ToBinary(result);
            txtResultHex.Text = Alu.ToHex(result);

            // Flagi
            chkZF.Checked = _alu.ZF;
            chkCF.Checked = _alu.CF;
            chkSF.Checked = _alu.SF;
            chkOF.Checked = _alu.OF;

            // Status
            lblStatus.Text = description;
            lblStatus.ForeColor = Color.DarkGreen;

            // Historia
            string flags = $"ZF={B(_alu.ZF)} CF={B(_alu.CF)} SF={B(_alu.SF)} OF={B(_alu.OF)}";
            string entry = $"{op,-4} A={a,5} B={b,5} -> {result,5} ({Alu.ToSigned(result),6})  [{flags}]";
            lstHistory.Items.Insert(0, entry);
        }

        private static string B(bool v) => v ? "1" : "0";

        private void ShowError(string msg)
        {
            lblStatus.Text = msg;
            lblStatus.ForeColor = Color.DarkRed;
        }
    }
}
