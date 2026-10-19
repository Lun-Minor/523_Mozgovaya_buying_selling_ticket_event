
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

namespace _523_Mozgovaya.AdminPages 
{
    /// <summary>
    /// Логика взаимодействия для DiagrammPage.xaml
    /// </summary>
    public partial class DiagrammPage : Page 
    {
        public DiagrammPage() 
        {
            InitializeComponent(); 


            ChartPayments.ChartAreas.Add(new ChartArea("Main")); // область для рисования графика 
            var currentSeries = new Series("Траты на билеты") // серия - сами данные, которые будут рисоваться
            {
                IsValueShownAsLabel = true // Указываем, что над каждым столбиком/сектором нужно писать число
            };
            ChartPayments.Series.Add(currentSeries); 

            CmbUser.ItemsSource = Core.entities.User.ToList(); 
            CmbDiagram.ItemsSource = Enum.GetValues(typeof(SeriesChartType)); 
        }

        private static string Fio(User u)
        {
            return $"{u.surname} {u.name} {u.last_name}".Trim(); 
        }

        // подсчет суммы, потраченной конкретным юзером на конкретную категорию 
        private double SumByCategory(User user, Category category)
        {
            return Core.entities.Order.Where(o => o.id_user == user.id_user).ToList() 
                .SelectMany(o => o.Ticket) 
                .Where(t => t.TicketCategory.Session.Event.id_category == category.id_category) 
                .Sum(t => t.TicketCategory.price * (1 - (t.Lgot?.skidka ?? 0))); 
        }

        
        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            
            if (CmbUser.SelectedItem is User currentUser && CmbDiagram.SelectedItem is SeriesChartType currentType)
            {
                Series currentSeries = ChartPayments.Series.FirstOrDefault(); // Берем серию данных
                currentSeries.ChartType = currentType; // Меняем форму графика, которую выбрал админ 
                currentSeries.Points.Clear(); // Очищаем старые точки

                foreach (var category in Core.entities.Category.ToList()) 
                    currentSeries.Points.AddXY(category.name, SumByCategory(currentUser, category)); // Добавляем точку: по X — название категории, по Y — сумма трат
            }
        }


        private void btWord_Click(object sender, RoutedEventArgs e) 
        {
            var allUsers = Core.entities.User.ToList(); 
            var allCategories = Core.entities.Category.ToList(); 

            var application = new Word.Application(); // Создаем (невидимое) приложение Word
            Word.Document document = application.Documents.Add(); // новый пустой документ

            foreach (var user in allUsers) //для каждого юзера создаем страницу 
            {
                // заголовок с фио
                Word.Paragraph userParagraph = document.Paragraphs.Add(); // новый абзац
                Word.Range userRange = userParagraph.Range; // диапазон этого абзаца (для форматирования)
                userRange.Text = Fio(user); // Вставляем ФИО
                userRange.Font.Name = "Times New Roman"; // Шрифт
                userRange.Font.Size = 20; // Размер
                userRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter; // Выравниваем по центру
                userRange.InsertParagraphAfter(); // символ абзаца после текста
                document.Paragraphs.Add(); // Добавляем пустой абзац как отступ

                // таблица
                Word.Paragraph tableParagraph = document.Paragraphs.Add(); // Абзац для таблицы
                Word.Range tableRange = tableParagraph.Range; // Диапазон для вставки
                Word.Table paymentsTable = document.Tables.Add(tableRange, allCategories.Count() + 1, 2); // таблица (Строк = категории + 1 шапка, Столбцов = 2)
                paymentsTable.Borders.InsideLineStyle = paymentsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle; // обычные сплошные линии для всех границ
                paymentsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter; // Выравнивание текст в ячейках по центру по вертикали

                // шапка таблицы
                Word.Range cellRange; // Переменная для доступа к ячейкам
                cellRange = paymentsTable.Cell(1, 1).Range; // Берем 1-ю строку, 1-й столбец
                cellRange.Text = "Категория"; // Текст заголовка
                cellRange = paymentsTable.Cell(1, 2).Range; // 1-я строка, 2-й столбец
                cellRange.Text = "Сумма расходов"; // Текст заголовка
                paymentsTable.Rows[1].Range.Font.Name = "Times New Roman"; // Шрифт для шапки
                paymentsTable.Rows[1].Range.Font.Size = 14; // Размер шрифта
                paymentsTable.Rows[1].Range.Bold = 1; // Делаем шапку жирной (1 = true)
                paymentsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter; // Выравниваем шапку по центру

                // заполнение таблицы данными
                for (int i = 0; i < allCategories.Count(); i++) {
                    var currentCategory = allCategories[i]; 
                    cellRange = paymentsTable.Cell(i + 2, 1).Range; // Берем ячейку (начиная со 2 строки, 1 — шапка)
                    cellRange.Text = currentCategory.name; // Вписываем название категории
                    cellRange.Font.Name = "Times New Roman"; // Шрифт
                    cellRange.Font.Size = 12; // Размер

                    cellRange = paymentsTable.Cell(i + 2, 2).Range; //соседнюю ячейку (2-й столбец)
                    cellRange.Text = SumByCategory(user, currentCategory).ToString("N2") + " руб."; // Считаем сумму и форматируем (N2 добавляет запятые и 2 знака после точки)
                    cellRange.Font.Name = "Times New Roman"; // Шрифт
                    cellRange.Font.Size = 12; // Размер
                }
                document.Paragraphs.Add(); 

                // самый дорогой и дешевый заказ
                var orders = Core.entities.Order.Where(o => o.id_user == user.id_user).ToList(); 
                Order maxOrder = orders.OrderByDescending(o => o.total_summ).FirstOrDefault(); 
                Order minOrder = orders.OrderBy(o => o.total_summ).FirstOrDefault(); 

                if (maxOrder != null) 
                {
                    Word.Paragraph maxParagraph = document.Paragraphs.Add(); // Новый абзац
                    Word.Range maxRange = maxParagraph.Range; // Диапазон
                    maxRange.Text = $"Самый дорогой заказ - №{maxOrder.id_order} на {maxOrder.total_summ:N2} " + $"руб. от {maxOrder.order_date:dd.MM.yyyy}"; // Формируем текст
                    
                    maxRange.InsertParagraphAfter(); // Переход на новую строку
                }
                document.Paragraphs.Add(); // Пустая строка

                if (minOrder != null) // для дешевого заказа
                {
                    Word.Paragraph minParagraph = document.Paragraphs.Add();
                    Word.Range minRange = minParagraph.Range;
                    minRange.Text = $"Самый дешёвый заказ - №{minOrder.id_order} на {minOrder.total_summ:N2} " + $"руб. от {minOrder.order_date:dd.MM.yyyy}";
                    minRange.InsertParagraphAfter();
                }

                // Если это не последний пользователь в списке, ставим разрыв страницы 
                if (user != allUsers.LastOrDefault())
                    document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
            }

         
            application.Visible = true; // вызываем окно Word 

            string folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // путь к Рабочему столу
            document.SaveAs2(System.IO.Path.Combine(folder, "Orders.docx")); // Сохраняем документ в формате Word
            
        }

        // Экспорт в Excel
        private void btExcel_Click(object sender, RoutedEventArgs e) 
        {
            var allUsers = Core.entities.User.ToList(); 
            var allCategories = Core.entities.Category.ToList(); 

            var application = new Excel.Application(); // Создаем приложение Excel
            application.SheetsInNewWorkbook = allUsers.Count(); // Говорим Excel создать столько листов, сколько у нас юзеров
            Excel.Workbook workbook = application.Workbooks.Add(Type.Missing); // новая книга

            for (int i = 0; i < allUsers.Count(); i++) // для каждого листа по юзеру
            {
                var user = allUsers[i]; 
                Excel.Worksheet worksheet = application.Worksheets.Item[i + 1]; 
                string title = user.surname + "_" + user.id_user; 
                worksheet.Name = title.Length > 31 ? title.Substring(0, 31) : title; // обрезаем, если имена листов длиннее 31 символа

                // запись данных в ячейки
                worksheet.Cells[1][1] = Fio(user); 
                worksheet.Range["A1"].Font.Bold = true; 
                worksheet.Range["A1"].Font.Size = 14; 
                

                worksheet.Cells[1][3] = "Категория"; // заголовок первого столбца
                worksheet.Cells[2][3] = "Сумма расходов, руб."; // Заголовок второго столбца
                worksheet.Range["A3:B3"].Font.Bold = true; 

                int row = 4; 
                double total = 0; 
                foreach (var category in allCategories) 
                {
                    double sum = SumByCategory(user, category); 
                    worksheet.Cells[1][row] = category.name; 
                    worksheet.Cells[2][row] = sum; 
                    total += sum; 
                    row++; 
                }
                worksheet.Cells[1][row] = "Итого"; 
                worksheet.Cells[2][row] = total; 

                worksheet.Range["A3:B" + row].Borders.LineStyle = Excel.XlLineStyle.xlContinuous; // границы таблицы 
                worksheet.Columns.AutoFit(); // автоматически ширина столбцов под текст
            }

            application.Visible = true; // Показываем Excel пользователю
        }

        private void btBack_Click(object sender, RoutedEventArgs e) 
        {
            NavigationService?.Navigate(new AdminProfilePage()); 
        }
    }
}