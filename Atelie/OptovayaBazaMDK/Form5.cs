using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OptovayaBazaMDK
{
    public partial class Form5 : Form
    {
        private SQLiteConnection connection;
        private string NamePLH = "Название товара";
        private string PricePLH= "0";
        public Form5()
        {
            InitializeComponent();


            // Инициализация подключения к базе данных MenuPrice.db
            string menuDbPath = "MenuPrice.db";
            bool createMenuTable = false;

            if (!System.IO.File.Exists(menuDbPath))
            {
                createMenuTable = true;
                SQLiteConnection.CreateFile(menuDbPath);
            }

            connection = new SQLiteConnection($"Data Source={menuDbPath};");

            connection.Open();

            if (createMenuTable)
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS Menu (Id INTEGER PRIMARY KEY, Name TEXT, Price REAL, Image BLOB);";
                    cmd.ExecuteNonQuery();

                    // Вставляем начальные данные в таблицу Menu
                    InsertInitialMenuData(cmd);
                }
            }

            // Загрузка данных из Menu в DataGridView
            LoadMenuDataToDataGridView();
        }

        // Метод для вставки начальных данных в таблицу Menu
        private void InsertInitialMenuData(SQLiteCommand cmd)
        {
            string[] initialItems = {
                "Нити", "Набор игол", "Спец одежда на заказ", "Набор швеи", "Шелк за 1м", "Лен за 1м","Шерсть за 1м", "Костюм на заказ"
            };

            double[] initialPrices = { 250, 500, 3500, 2500, 220, 170, 190,10000 };

            byte[] emptyImage = new byte[0]; // Пустое изображение (пустой BLOB)

            for (int i = 0; i < initialItems.Length; i++)
            {
                cmd.CommandText = "INSERT INTO Menu (Name, Price, Image) VALUES (@Name, @Price, @Image);";
                cmd.Parameters.AddWithValue("@Name", initialItems[i]);
                cmd.Parameters.AddWithValue("@Price", initialPrices[i]);
                cmd.Parameters.AddWithValue("@Image", emptyImage);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
        }

        private void LoadMenuDataToDataGridView()
        {
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM Menu", connection))
            {
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Создаем столбец DataGridViewButtonColumn
                    DataGridViewButtonColumn buttonColumn = new DataGridViewButtonColumn();
                    buttonColumn.HeaderText = "Добавить";
                    buttonColumn.Text = "Добавить";
                    buttonColumn.UseColumnTextForButtonValue = true;
                    dataGridView1.Columns.Add(buttonColumn);

                    // Устанавливаем источник данных для DataGridView
                    dataGridView1.DataSource = dataTable;
                }
            }

            // Установите ширину каждого столбца в DataGridView
            dataGridView1.Columns[0].Width = 90;
            dataGridView1.Columns[1].Width = 40;
            dataGridView1.Columns[2].Width = 190;
            dataGridView1.Columns[3].Width = 40;
            dataGridView1.Columns[4].Width = 250;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        }

        private void Form5_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Проверяем, что событие произошло в столбце кнопок (колонке с индексом 0)
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                // Получаем значение столбца "ID" из ячейки
                int recordID = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["ID"].Value);

                // Удалите строку из базы данных
                using (SQLiteCommand deleteCommand = new SQLiteCommand("DELETE FROM Menu WHERE ID = @id", connection))
                {
                    deleteCommand.Parameters.AddWithValue("@id", recordID);
                    deleteCommand.ExecuteNonQuery();
                }

                // Удалите строку из DataGridView
                dataGridView1.Rows.RemoveAt(e.RowIndex);
            }
        }



        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (tbName.Text != NamePLH || tbPrice.Text != PricePLH)
            {
                // Получите значения из текстовых полей
                string name = tbName.Text;
                decimal price;

                if (decimal.TryParse(tbPrice.Text, out price))
                {
                    // SQL-запрос для вставки данных
                    string query = "INSERT INTO Menu (Name, Price) VALUES (@name, @price)";

                    // Создайте команду с параметрами
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@price", price);

                        // Выполните запрос
                        command.ExecuteNonQuery();

                        // Очистите текстовые поля после вставки
                        tbName.Clear();
                        tbPrice.Clear();

                        // Обновите DataGridView, чтобы отобразить новые данные
                        RefreshDataGridView();
                    }
                }
                else
                {
                    MessageBox.Show("Неверный формат цены. Введите числовое значение.");
                }
            }
            else
            {
                MessageBox.Show("Измените значениz.");
            }
        }


        // Метод для обновления DataGridView с новыми данными из базы данных
        private void RefreshDataGridView()
        {
            // Предполагая, что dataGridView1 у вас уже связан с источником данных (например, DataTable)
            // Очистите существующие данные
            DataTable dataTable = (DataTable)dataGridView1.DataSource;
            dataTable.Clear();

            // Загрузите данные из базы данных в DataTable
            string query = "SELECT * FROM Menu";
            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection))
            {
                adapter.Fill(dataTable);
            }

            // Обновите DataGridView
            dataGridView1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Если запись найдена, откройте Form2
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Обработка ошибки данных в DataGridView
            if (e.Context == DataGridViewDataErrorContexts.Commit)
            {
                MessageBox.Show("Ошибка при сохранении данных.");
            }
            else if (e.Context == DataGridViewDataErrorContexts.CurrentCellChange)
            {
                MessageBox.Show("Ошибка в текущей ячейке.");
            }
            else if (e.Context == DataGridViewDataErrorContexts.Parsing)
            {
                MessageBox.Show("Ошибка при разборе данных.");
            }
            else if (e.Context == DataGridViewDataErrorContexts.Formatting)
            {
                MessageBox.Show("Ошибка при форматировании данных.");
            }
            else
            {
                MessageBox.Show("Произошла ошибка данных.");
            }
        }

    }
}
