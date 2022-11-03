using System.Diagnostics;
using System.Text;

namespace Laboratornaya_1_threads
{
    public partial class Form1 : Form
    {
        private long timePosledovat; // счмиает время при последовательном способе метода
        private long[] times; // массив запоминает время 8 параллельных вычислений
        public Form1()
        {
            InitializeComponent();
        }

        #region методы для дальнейшего подсчета времени
        // Инициализация массива рандомными числами
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

        // Функция над элементом массива
        public double Function(double x)
        {
            return 1 / Math.Tan(x);
        }
        
        //Метод параллельного сложения матриц
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

        // Метод последовательного сложения матриц
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


        // Рисуем график
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = panel1.CreateGraphics(); // Создаём объект рисования
            g.TranslateTransform(50, 320); // Смещение начала координат (в пикселях)
            g.ScaleTransform(0.4f, 0.4f); // Масштабирование

            Pen penCO = new Pen(Color.Green, 2f); // Перо для отрисовки осей

            // Рисуем координатные оси
            g.DrawLine(penCO, new Point(-5000, 0), new Point(5000, 0));
            g.DrawLine(penCO, new Point(0, -5000), new Point(0, 5000));

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Записываем размер массива
            const int n = 1000;

            // Создаём массивы
            double[,] a = Initialize(n);
            double[,] b = Initialize(n);


            #region время для последовательного потока
            Stopwatch sw = new Stopwatch();
            sw.Start();
            // Считаем последовательно
            Posledovatel(a, b, n); // не параллельный, последовательный 
            sw.Stop();

            timePosledovat = sw.ElapsedMilliseconds; // время при последовательном вычислении
            #endregion


            #region время для параллельного потока, записываем в массив times
            // Считаем параллельно и запоминаем время для кол-ва потоков от 1 до 9
            // массив куда закидываем время потоков
            times = new long[9];

            for (int i = 1; i < 9; i++)
            {
                // Создаём массивы заново
                a = Initialize(n);
                b = Initialize(n);

                // Считаем
                sw.Restart();
                Parallelniy(a, b, i);
                sw.Stop();

                times[i] = sw.ElapsedMilliseconds; // отображаем в милисекундах
            }
            #endregion


            #region запись значений времени в dataGridView при паралельном вычислении
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView1.ColumnCount = 2;
            dataGridView1.RowCount = 9;
            dataGridView1.Rows[0].Cells[0].Value = "Число потоков";
            dataGridView1.Rows[0].Cells[1].Value = "Время выполнения";

            for (int i = 1; i < 9; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = i;
                dataGridView1.Rows[i].Cells[1].Value = times[i];
            }
            #endregion


            #region отображение на графике
            Graphics g = panel1.CreateGraphics(); // Создаём объект рисования
            g.TranslateTransform(50, 320); // Смещение начала координат (в пикселях)
            g.ScaleTransform(0.4f, 0.4f); // Масштабирование


            PointF[] points = new PointF[9]; // Список точек графика

            double k = 0.5; // Коэффициент масштабирования значений

            // Запоминаем точки
            for (int i = 0; i < 9; i++)
            {
                points[i] = new PointF((i) * 100f, -float.Parse((times[i] * k).ToString()));
            }


            Pen pen = new Pen(Color.Black, 0.5f);// Перо для отрисовки графика
            //Pen penCO = new Pen(Color.Green, 2f); // Перо для отрисовки осей

            Font fo = new System.Drawing.Font(FontFamily.GenericSerif, 20f); // Шрифт для вывода текста

            // Рисуем линии графика
            g.DrawLines(pen, points);

            // Выводим значения на оси и легенду
            g.DrawString("0", fo, Brushes.Black, -20, 20);
            for (int i = 1; i <= 8; i++)
            {
                g.DrawString("" + i, fo, Brushes.Black, i * 100f - 10, 20);
                g.DrawString("" + (points[i - 1].Y * -2), fo, Brushes.Black, -80, points[i - 1].Y - 10);
            }

            g.DrawString("Число потоков", fo, Brushes.Black, 1000, 20);
            g.DrawString("Время работы\n в мс", fo, Brushes.Black, 10, -800);
            g.DrawString("Время работы последовательной программы, мс: " + timePosledovat, fo, Brushes.Black, 200, 80);
            #endregion 
        }
    }
}
