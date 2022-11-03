using System.Diagnostics;
using System.Text;

namespace Laboratornaya_1_threads
{
    public partial class Form1 : Form
    {
        private long timePosledovat; // ������� ����� ��� ���������������� ������� ������
        private long[] times; // ������ ���������� ����� 8 ������������ ����������
        public Form1()
        {
            InitializeComponent();
        }

        #region ������ ��� ����������� �������� �������
        // ������������� ������� ���������� �������
        public double[,] Initialize(int n)
        {
            double[,] result = new double[n, n];
            Random r = new Random();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result[i, j] = r.Next();
                }
            }
            return result;
        }

        // ������� ��� ��������� �������
        public double Function(double x)
        {
            return 1 / Math.Tan(x);
        }
        
        //����� ������������� �������� ������
        public void Parallelniy(double[,] a, double[,] b, int m)
        {
            var endMatrix = a;
            var opt = new ParallelOptions();
            opt.MaxDegreeOfParallelism = m;
            
            Parallel.For(
                0, m, opt, l =>
                {
                    int Rowdivm = endMatrix.GetLength(0) / m;
                    int Coldivm = endMatrix.GetLength(1) / m;

                    int begRow = Rowdivm * l;
                    int begCol = Coldivm * l;

                    int endRow = Rowdivm * (l + 1) - 1 + (l == m - 1 ? endMatrix.GetLength(0) % m : 0);
                    int endCol = Coldivm * (l + 1) - 1 + (l == m - 1 ? endMatrix.GetLength(1) % m : 0);

                    for (int i = begRow; i <= endRow; i++)
                    {
                        for (int j = begCol; j <= endCol; j++)
                        {
                            endMatrix[i,j] = Function(a[i, j] + b[i, j]);
                        }
                    }

                }
            );

            a = endMatrix; 
        }

        // ����� ����������������� �������� ������
        public void Posledovatel(double[,] a, double[,] b, int n)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    a[i, j] = Function(a[i, j] + b[i, j]);
                }
            }
        }
        #endregion


        // ������ ������
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = panel1.CreateGraphics(); // ������ ������ ���������
            g.TranslateTransform(50, 320); // �������� ������ ��������� (� ��������)
            g.ScaleTransform(0.4f, 0.4f); // ���������������

            Pen penCO = new Pen(Color.Green, 2f); // ���� ��� ��������� ����

            // ������ ������������ ���
            g.DrawLine(penCO, new Point(-5000, 0), new Point(5000, 0));
            g.DrawLine(penCO, new Point(0, -5000), new Point(0, 5000));

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // ���������� ������ �������
            const int n = 1000;

            // ������ �������
            double[,] a = Initialize(n);
            double[,] b = Initialize(n);


            #region ����� ��� ����������������� ������
            Stopwatch sw = new Stopwatch();
            sw.Start();
            // ������� ���������������
            Posledovatel(a, b, n); // �� ������������, ���������������� 
            sw.Stop();

            timePosledovat = sw.ElapsedMilliseconds; // ����� ��� ���������������� ����������
            #endregion


            #region ����� ��� ������������� ������, ���������� � ������ times
            // ������� ����������� � ���������� ����� ��� ���-�� ������� �� 1 �� 9
            // ������ ���� ���������� ����� �������
            times = new long[9];

            for (int i = 1; i < 9; i++)
            {
                // ������ ������� ������
                a = Initialize(n);
                b = Initialize(n);

                // �������
                sw.Restart();
                Parallelniy(a, b, i);
                sw.Stop();

                times[i] = sw.ElapsedMilliseconds; // ���������� � ������������
            }
            #endregion


            #region ������ �������� ������� � dataGridView ��� ����������� ����������
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView1.ColumnCount = 2;
            dataGridView1.RowCount = 9;
            dataGridView1.Rows[0].Cells[0].Value = "����� �������";
            dataGridView1.Rows[0].Cells[1].Value = "����� ����������";

            for (int i = 1; i < 9; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = i;
                dataGridView1.Rows[i].Cells[1].Value = times[i];
            }
            #endregion


            #region ����������� �� �������
            Graphics g = panel1.CreateGraphics(); // ������ ������ ���������
            g.TranslateTransform(50, 320); // �������� ������ ��������� (� ��������)
            g.ScaleTransform(0.4f, 0.4f); // ���������������


            PointF[] points = new PointF[9]; // ������ ����� �������

            double k = 0.5; // ����������� ��������������� ��������

            // ���������� �����
            for (int i = 0; i < 9; i++)
            {
                points[i] = new PointF((i) * 100f, -float.Parse((times[i] * k).ToString()));
            }


            Pen pen = new Pen(Color.Black, 0.5f);// ���� ��� ��������� �������
            //Pen penCO = new Pen(Color.Green, 2f); // ���� ��� ��������� ����

            Font fo = new System.Drawing.Font(FontFamily.GenericSerif, 20f); // ����� ��� ������ ������

            // ������ ����� �������
            g.DrawLines(pen, points);

            // ������� �������� �� ��� � �������
            g.DrawString("0", fo, Brushes.Black, -20, 20);
            for (int i = 1; i <= 8; i++)
            {
                g.DrawString("" + i, fo, Brushes.Black, i * 100f - 10, 20);
                g.DrawString("" + (points[i - 1].Y * -2), fo, Brushes.Black, -80, points[i - 1].Y - 10);
            }

            g.DrawString("����� �������", fo, Brushes.Black, 1000, 20);
            g.DrawString("����� ������\n � ��", fo, Brushes.Black, 10, -800);
            g.DrawString("����� ������ ���������������� ���������, ��: " + timePosledovat, fo, Brushes.Black, 200, 80);
            #endregion 
        }
    }
}
