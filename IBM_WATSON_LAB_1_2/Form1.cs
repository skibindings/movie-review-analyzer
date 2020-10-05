using GoogleCSE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IBM_WATSON_LAB_1_2
{
    public partial class Form1 : Form
    {
        BusinessLogic bs;
        
        public Form1()
        {
            InitializeComponent();
            // в этом классе вся логика приложения
            bs = new BusinessLogic();

            // тест метода
            //bs.GetInnerTextOfHTML(bs.LoadHTML("https://www.rottentomatoes.com/m/inception/reviews?type=user"));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void информацияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Movie Review Analyzer - 2020\n\n" +
                "Разработчики: Скибин К.С, Сухарев Р.В\n\n" +
                "Описание: данная программа производит анализ\n" +
                "эмоционального спектра отзывов зрителей о фильмах,\n" +
                "оставленных на англоязычных ресурсах:\n" +
                "   1. imdb.com\n" +
                "   2. letterboxd.com\n" +
                "   3. rottentomatoes.com",
                "Информация");
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // нажатие кнопочки
            toolStripStatusLabel1.Text = "Обработка...";
            // тут собсна всё работа ----------------------------------------------------------------------
            bs.ExtractReviews(textBox1.Text,
                label5,
                label6,
                label7,
                label8,
                label9,
                label10);
            // все эти label'ы в параметрах отвечают за текст во вкладочках в эмоциях и мнении для каждого ресурса
            // пример label5 - эмоции в imdb, label6 - сентимент в imdb, label7 - эмоции в letterboxd и.т.д
            // этот метод сам заполняет эти label'ы
            // --------------------------------------------------------------------------------------------

            // косметика
            if(bs.ExistsOnIMDB())
            {
                tabControl1.Visible = true;
                label2.Text = bs.imdb_title;
            }
            else
            {
                tabControl1.Visible = false;
                label2.Text = "Фильм не найден!";
            }

            if (bs.ExistsOnLetterboxd())
            {
                tabControl2.Visible = true;
                label3.Text = bs.letterboxd_title;
            }
            else
            {
                tabControl2.Visible = false;
                label3.Text = "Фильм не найден!";
            }

            if (bs.ExistsOnRT())
            {
                tabControl3.Visible = true;
                label4.Text = bs.rt_title;
            }
            else
            {
                tabControl3.Visible = false;
                label4.Text = "Фильм не найден!";
            }

            label2.Visible = true;
            label3.Visible = true;
            label4.Visible = true;
            toolStripStatusLabel1.Text = "Анализ произведён!";
        }
    }
}
