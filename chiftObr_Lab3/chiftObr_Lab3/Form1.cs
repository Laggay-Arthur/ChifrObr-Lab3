using System;
using System.Numerics;
using System.Windows.Forms;


namespace chiftObr_Lab3
{
    public partial class Form1 : Form
    {
        double Emax, // Амплитуда
            ti, // Длительность
            w_verx, // Частотный диапазон
            N, // Кол-во точек отсчета
            dt, dw;
        public Form1() { InitializeComponent(); }
        void Calculating_Click(object sender, EventArgs e)
        {
            if (!double.TryParse(Emax_input.Text, out Emax) || !double.TryParse(ti_input.Text, out ti)) { MessageBox.Show("Ошибка в входных данных"); return; }
            ti *= Math.Pow(10, -6); ;
            dt = ti / 100;
            if (P_input.Text.Contains(".") && double.TryParse(P_input.Text.Replace('.', ','), out double P)) { }
            else if (P_input.Text.Contains(",") && double.TryParse(P_input.Text, out P)) { }
            else { MessageBox.Show("Ошибочный разделитель дробной части!"); return; }
            chart1.Series[0].Points.Clear(); chart1.Series[1].Points.Clear(); chart1.Series[2].Points.Clear();
            w_verx = (1.0 / ti);
            double Ec = this.Ec(), Edw;
            dw = w_verx / 100;
            do
            {
                Edw = CalcEdw();
                w_verx += (int)(1.0 / ti);
            }
            while (Edw / Ec < P);//Пока не достигнем точности в P% продолжаем расчет

            double fd = w_verx / Math.PI;//Частота дискретизации
            double delta_t = 1 / fd;//Расстояние между отсчётами
            N = Math.Ceiling(ti / delta_t);

            delta_t = ti / N;
            fd = 1 / delta_t;
            w_verx = Math.Ceiling(fd * Math.PI);
            if (checkBox1.Checked)
            {// Если отображение графиков включено
                double h = ti / 128;//Шаг отрисовки точки
                for (double t = 0; t <= ti; t += h)
                {// Строим график S(t) на промежутке [ 0 ; ti ]
                    chart1.Series[0].Points.AddXY(t * Math.Pow(10, 6), S(t));
                    chart1.Series[1].Points.AddXY(t * Math.Pow(10, 6), CalcF(t));
                }
                for (int t = 0; t <= N; t++) chart1.Series[2].Points.AddXY(delta_t * t * Math.Pow(10, 6), S(delta_t * t));
            }
            wmax.Text = "Эффективная ширина спектра данного сигнала: " + w_verx/1000000 + " МГц";
        }
        double S(double t)
        {// Функция исходного сигнала
            if (t >0 && t <= (ti/2 )) return (-2*Emax*t)/ti;
            else
            if (t > (ti / 2)) return (2*Emax*(t-ti))/ti;
            return 0;
        }
        Complex Sw(double w)
        {// Спектральная плотность сигнала
            Complex Sum = new Complex(0, 0);
            for (double t = 0; t <= ti; t += dt)
                Sum += S(t) * new Complex(Math.Cos(w * t), -Math.Sin(w * t)) * dt;
            return Sum;
        }
        double Ec()
        {// Полная энергия одиночного импульса
            double E_c = 0;
            for (double t = 0; t <= ti; t += dt)
                E_c += Math.Pow(S(t), 2) * dt;
            return E_c;
        }
        double CalcEdw()
        {// Энергия, сосредоточенная в полосе частот
            double Sum = 0;
            for (double w = 0; w <= w_verx; w += dw)
                Sum += Math.Pow(Sw(w).Magnitude, 2) * dw;
            return (1 / Math.PI) * Sum;
        }
        Complex GetSk(double k)
        {// Мнгновенное значение сигнала
            Complex Sum = new Complex(0, 0);
            for (double w = -w_verx; w <= w_verx; w += dw)
                Sum += Sw(w) * new Complex(Math.Cos(k * Math.PI * w / w_verx), Math.Sin(k * Math.PI * w / w_verx)) * dw;
            return (1 / (2 * Math.PI)) * Sum;
        }
        double CalcF(double t)
        {// Восстановленный сигнал через ряд Котельникова
            double Sum = 0;
            for (double k = 0; k <= N; k++)
                Sum += GetSk(k).Real * Math.Sin(w_verx * (t - (k * Math.PI / w_verx))) / (w_verx * (t - (k * Math.PI / w_verx)));
            return Sum;
        }
    }
}