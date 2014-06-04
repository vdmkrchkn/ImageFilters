using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Filters
{
    public class HSV
    {
        //constructor
        public HSV(int h = 0, double s = 0, double v = 0)
        {
            this.hue = h;
            this.sat = s;
            this.value = v;
        }
        public HSV(Color clr)
        {
            this.toHSV(clr);
        }
        // getters
        public int Hue
        {
            get { return this.hue; }
        }
        public double Saturation
        {
            get { return this.sat; }
        }
        public double Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
        /// <summary>
        /// Преобразовывает цвет из RGB в HSV
        /// </summary>
        /// <param name="color">исходный цвет</param>
        public void toHSV(Color color)
        {
            double red = color.R / 255.0;
            double gr = color.G / 255.0;
            double bl = color.B / 255.0;

            double max1 = Math.Max(red, gr);
            double min1 = Math.Min(red, gr);
            double max = Math.Max(bl, max1);
            double min = Math.Min(min1, bl);

            value = max;

            if (max == 0)
                sat = 0;
            else
                sat = 1 - (double)min / max;

            if (max == min)
                hue = 0;
            else
                if (max == red && gr >= bl)
                    hue = (int)(60 * (gr - bl) / (max - min));
                else if (max == red && gr < bl)
                    hue = (int)(60 * (gr - bl) / (max - min) + 360);
                else if (max == gr)
                    hue = (int)(60 * (bl - red) / (max - min) + 120);
                else
                    hue = (int)(60 * (red - gr) / (max - min) + 240);
        }
        /// <summary>
        /// Преобразовывает модель HSV в RGB
        /// </summary>
        /// <returns>новый цвет</returns>
        public Color toRGB()
        {
            double cB, cG, cR;
            cB = cG = cR = 0.0;
            double f, p, q, t;

            int Hi = (int)Math.Floor(hue / 60.0) % 6;
            f = hue / 60.0 - Math.Floor(hue / 60.0);
            p = value * (1 - sat);
            q = value * (1 - f * sat);
            t = value * (1 - sat * (1 - f));
            switch (Hi)
            {
                case 0:
                    cR = value;
                    cG = t;
                    cB = p;
                    break;
                case 1:
                    cR = q;
                    cG = value;
                    cB = p;
                    break;
                case 2:
                    cR = p;
                    cG = value;
                    cB = t;
                    break;
                case 3:
                    cR = p;
                    cG = q;
                    cB = value;
                    break;
                case 4:
                    cR = t;
                    cG = p;
                    cB = value;
                    break;
                case 5:
                    cR = value;
                    cG = p;
                    cB = q;
                    break;
            };

            Color clr;
            try
            {
                clr = Color.FromArgb((int)(cR * 255.0), (int)(cG * 255.0), (int)(cB * 255.0));
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine(ae.Message);
                return Color.Empty;
            }
            return clr;
        }
        /// <summary>
        /// Увеличение значения яркости
        /// </summary>
        /// <param name="n">увеличивающее значение</param>
        /// <param name="mistake">предыдущая погрешность</param>
        /// <returns>новая погрешность</returns>
        public void addValue(int n, ref double mistake)
        {
            if (mistake < 0)
                if (n > 0 && n / 100.0 >= Math.Abs(mistake))
                {
                    value = mistake + n / 100.0;
                    mistake = 0;
                }
                else
                    //
                    mistake += n / 100.0;
            else
                if (mistake > 1)
                    if (n < 0 && Math.Abs(n) / 100.0 + 1 >= mistake)
                    {
                        value = mistake + n / 100.0;
                        mistake = 0;
                    }
                    else
                        mistake += n / 100.0;
                else
                    if (value * 100 < Math.Abs(n) && n < 0)
                    {
                        mistake = value + n / 100.0;
                        value = 0.0;
                    }
                    else
                        if (value * 100 + n > 100)
                        {
                            mistake = value + n / 100.0;
                            value = 1.0;
                        }
                        else
                            value += n / 100.0;
            //return mistake;
        }

        int hue;
        double sat, value;
    }
}
