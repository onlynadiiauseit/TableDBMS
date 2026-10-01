using TableDBMS.Models;
using TableDBMS.Services;
using TableDBMS.Forms;
namespace TableDBMS
{
    public partial class MainForm : Form
    {
        private readonly TableUnionService _tableUnionService = new();
        private readonly StorageService _storageService = new();
        private readonly RemoteStorageService _remoteStorageService =
    new("http://192.168.0.100:5080");
        private Database? _database;
        private Table? _selectedTable;
        private string? _currentFilePath;

        private TreeView _tablesTree = null!;
        private DataGridView _dataGrid = null!;
        private Label _databaseLabel = null!;
        private Label _tableLabel = null!;
        private ToolStripStatusLabel _statusLabel = null!;
        private Button _addRowButton = null!;
        private Button _editRowButton = null!;
        private Button _deleteRowButton = null!;

        public MainForm()
        {
            InitializeComponent();
            BuildInterface();
            RefreshDatabaseView();
        }
        private void AutoSaveDatabase()
        {
            if (_database == null)
                return;

            if (string.IsNullOrWhiteSpace(_currentFilePath))
            {
                _statusLabel.Text =
                    "Є незбережені зміни. Натисніть Ctrl+S.";

                return;
            }

            try
            {
                _storageService.SaveDatabase(
                    _database,
                    _currentFilePath
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не вдалося автоматично зберегти базу даних.\n\n{ex.Message}",
                    "Помилка автозбереження",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private void UnionTables()
        {
            if (_database == null)
            {
                MessageBox.Show(
                    "Спочатку створіть або відкрийте базу даних.",
                    "Немає бази даних",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            if (_database.Tables.Count < 2)
            {
                MessageBox.Show(
                    "Для об'єднання потрібно щонайменше дві таблиці.",
                    "Недостатньо таблиць",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            using var dialog =
                new UnionTablesForm(_database);

            if (dialog.ShowDialog(this) !=
                DialogResult.OK)
            {
                return;
            }

            if (dialog.FirstTable == null ||
                dialog.SecondTable == null)
            {
                return;
            }

            try
            {
                Table result =
                    _tableUnionService.Union(
                        dialog.FirstTable,
                        dialog.SecondTable,
                        dialog.ResultTableName
                    );

                _database.AddTable(result);

                AutoSaveDatabase();

                RefreshDatabaseView();

                SelectTable(result);

                _statusLabel.Text =
                    $"Створено таблицю '{result.Name}' " +
                    $"шляхом об'єднання.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка об'єднання таблиць",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }
        private void AddRow()
        {
            if (_selectedTable == null)
            {
                MessageBox.Show(
                    "Спочатку оберіть таблицю.",
                    "Таблиця не обрана",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            using var dialog =
                new RowEditForm(_selectedTable);

            if (dialog.ShowDialog(this) !=
                DialogResult.OK)
            {
                return;
            }

            if (dialog.ResultRow == null)
                return;

            try
            {
                _selectedTable.AddRow(
    dialog.ResultRow
);

                AutoSaveDatabase();

                ShowTable(_selectedTable);

                _statusLabel.Text =
                    "Рядок додано та збережено.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void EditRow()
        {
            if (_selectedTable == null)
            {
                MessageBox.Show(
                    "Спочатку оберіть таблицю.",
                    "Таблиця не обрана",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            if (_dataGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Оберіть рядок для редагування.",
                    "Рядок не обрано",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            int index =
                _dataGrid.SelectedRows[0].Index;

            if (index < 0 ||
                index >= _selectedTable.Rows.Count)
            {
                return;
            }

            Row existingRow =
                _selectedTable.Rows[index];

            using var dialog =
                new RowEditForm(
                    _selectedTable,
                    existingRow
                );

            if (dialog.ShowDialog(this) !=
                DialogResult.OK)
            {
                return;
            }

            if (dialog.ResultRow == null)
                return;

            try
            {
                _selectedTable.UpdateRow(
    index,
    dialog.ResultRow
);

                AutoSaveDatabase();

                ShowTable(_selectedTable);

                _statusLabel.Text =
                    "Рядок змінено та збережено.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private void EditTableStructure()
        {
            if (_selectedTable == null)
            {
                MessageBox.Show(
                    "Спочатку оберіть таблицю.",
                    "Таблиця не обрана",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            Table table =
                _selectedTable;

            using var dialog =
                new EditTableStructureForm(
                    table
                );

            if (dialog.ShowDialog(this) !=
                DialogResult.OK)
            {
                return;
            }

            if (dialog.ResultTable == null)
                return;

            table.Schema =
                dialog.ResultTable.Schema;

            table.Rows =
                dialog.ResultTable.Rows;

            AutoSaveDatabase();

            RefreshDatabaseView();

            SelectTable(table);

            _statusLabel.Text =
                $"Структуру таблиці '{table.Name}' змінено.";
        }
        private void RenameTable()
        {
            if (_database == null ||
                _selectedTable == null)
            {
                MessageBox.Show(
                    "Спочатку оберіть таблицю.",
                    "Таблиця не обрана",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            string? newName = PromptForText(
                "Перейменування таблиці",
                "Введіть нову назву таблиці:"
            );

            if (string.IsNullOrWhiteSpace(newName))
                return;

            newName = newName.Trim();

            Table? existingTable =
                _database.GetTable(newName);

            if (existingTable != null &&
                !ReferenceEquals(
                    existingTable,
                    _selectedTable))
            {
                MessageBox.Show(
                    $"Таблиця '{newName}' вже існує.",
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            _selectedTable.Name = newName;

            AutoSaveDatabase();

            RefreshDatabaseView();

            SelectTable(_selectedTable);

            _statusLabel.Text =
                $"Таблицю перейменовано: {newName}";
        }
        private void DeleteTable()
        {
            if (_database == null ||
                _selectedTable == null)
            {
                MessageBox.Show(
                    "Спочатку оберіть таблицю.",
                    "Таблиця не обрана",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            string tableName =
                _selectedTable.Name;

            DialogResult answer =
                MessageBox.Show(
                    $"Видалити таблицю '{tableName}'?\n\n" +
                    "Усі рядки цієї таблиці також будуть видалені.",
                    "Підтвердження видалення",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

            if (answer != DialogResult.Yes)
                return;

            try
            {
                _database.RemoveTable(tableName);

                _selectedTable = null;

                AutoSaveDatabase();

                RefreshDatabaseView();

                _statusLabel.Text =
                    $"Таблицю '{tableName}' видалено.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка видалення",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private void DeleteRow()
        {
            if (_selectedTable == null)
                return;

            if (_dataGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Оберіть рядок для видалення.",
                    "Рядок не обрано",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            int index =
                _dataGrid.SelectedRows[0].Index;

            DialogResult answer =
                MessageBox.Show(
                    "Видалити вибраний рядок?",
                    "Підтвердження",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (answer != DialogResult.Yes)
                return;

            try
            {
                _selectedTable.DeleteRow(index);

                AutoSaveDatabase();

                ShowTable(_selectedTable);

                _statusLabel.Text =
                    "Рядок видалено та збережено.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void BuildInterface()
        {
            Text = "TableDBMS — СУБД табличних баз даних";
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1000, 650);

            BuildMenu();

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 6, 0, 0)
            };

            Controls.Add(mainPanel);

            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterWidth = 5,
                FixedPanel = FixedPanel.Panel1
            };

            mainPanel.Controls.Add(splitContainer);

            BuildLeftPanel(splitContainer.Panel1);
            BuildRightPanel(splitContainer.Panel2);
            BuildStatusBar();

            MainMenuStrip?.BringToFront();

            Shown += (_, _) =>
            {
                SetSafeSplitterDistance(splitContainer);
            };

            splitContainer.SizeChanged += (_, _) =>
            {
                SetSafeSplitterDistance(splitContainer);
            };
        }
        private static void SetSafeSplitterDistance(
    SplitContainer splitContainer)
        {
            if (splitContainer.Width <= 0)
                return;

            int desiredLeftWidth = 280;
            int minimumLeftWidth = 220;
            int minimumRightWidth = 500;

            int maximumLeftWidth =
                splitContainer.Width
                - splitContainer.SplitterWidth
                - minimumRightWidth;

            if (maximumLeftWidth < minimumLeftWidth)
            {
                maximumLeftWidth =
                    splitContainer.Width
                    - splitContainer.SplitterWidth
                    - 100;
            }

            if (maximumLeftWidth <= 0)
                return;

            splitContainer.SplitterDistance =
                Math.Min(
                    desiredLeftWidth,
                    maximumLeftWidth
                );
        }
        private void BuildMenu()
        {
            var menuStrip = new MenuStrip
            {
                Dock = DockStyle.Top
            };

            var fileMenu = new ToolStripMenuItem("Файл");

            var newDatabaseItem =
                new ToolStripMenuItem("Нова база даних");

            newDatabaseItem.ShortcutKeys =
                Keys.Control | Keys.N;

            newDatabaseItem.Click += (_, _) =>
                CreateNewDatabase();

            var openItem =
                new ToolStripMenuItem("Відкрити...");

            openItem.ShortcutKeys =
                Keys.Control | Keys.O;

            openItem.Click += (_, _) =>
                OpenDatabase();

            var saveItem =
                new ToolStripMenuItem("Зберегти");

            saveItem.ShortcutKeys =
                Keys.Control | Keys.S;

            saveItem.Click += (_, _) =>
                SaveDatabase();

            var saveAsItem =
                new ToolStripMenuItem("Зберегти як...");

            saveAsItem.Click += (_, _) =>
                SaveDatabaseAs();

            var exitItem =
                new ToolStripMenuItem("Вихід");

            exitItem.Click += (_, _) =>
                Close();

            fileMenu.DropDownItems.Add(newDatabaseItem);
            fileMenu.DropDownItems.Add(openItem);
            fileMenu.DropDownItems.Add(
                new ToolStripSeparator()
            );
            fileMenu.DropDownItems.Add(saveItem);
            fileMenu.DropDownItems.Add(saveAsItem);
            fileMenu.DropDownItems.Add(
                new ToolStripSeparator()
            );
            fileMenu.DropDownItems.Add(exitItem);


            var tableMenu =
    new ToolStripMenuItem("Таблиця");

            var createTableItem =
                new ToolStripMenuItem(
                    "Створити таблицю..."
                );

            createTableItem.Click += (_, _) =>
                CreateTable();

            var editStructureItem =
                new ToolStripMenuItem(
                    "Редагувати структуру..."
                );

            editStructureItem.Click += (_, _) =>
                EditTableStructure();

            var renameTableItem =
                new ToolStripMenuItem(
                    "Перейменувати таблицю..."
                );

            renameTableItem.Click += (_, _) =>
                RenameTable();

            var deleteTableItem =
                new ToolStripMenuItem(
                    "Видалити таблицю"
                );

            deleteTableItem.Click += (_, _) =>
                DeleteTable();

            tableMenu.DropDownItems.Add(
                createTableItem
            );

            tableMenu.DropDownItems.Add(
                new ToolStripSeparator()
            );

            tableMenu.DropDownItems.Add(
                editStructureItem
            );

            tableMenu.DropDownItems.Add(
                renameTableItem
            );

            tableMenu.DropDownItems.Add(
                deleteTableItem
            );

            var operationsMenu =
                new ToolStripMenuItem("Операції");
            var unionTablesItem =
    new ToolStripMenuItem(
        "Об'єднання таблиць..."
    );

            unionTablesItem.Click += (_, _) =>
                UnionTables();

            operationsMenu.DropDownItems.Add(
                unionTablesItem
            );
            var serverMenu =
    new ToolStripMenuItem("Сервер");

            var checkConnectionItem =
                new ToolStripMenuItem(
                    "Перевірити з'єднання"
                );

            checkConnectionItem.Click +=
                async (_, _) =>
                    await CheckServerConnectionAsync();

            var saveToServerItem =
                new ToolStripMenuItem(
                    "Зберегти на сервері"
                );

            saveToServerItem.Click +=
                async (_, _) =>
                    await SaveDatabaseToServerAsync();

            var openFromServerItem =
                new ToolStripMenuItem(
                    "Відкрити з сервера..."
                );

            openFromServerItem.Click +=
                async (_, _) =>
                    await OpenDatabaseFromServerAsync();

            serverMenu.DropDownItems.Add(
                checkConnectionItem
            );

            serverMenu.DropDownItems.Add(
                new ToolStripSeparator()
            );

            serverMenu.DropDownItems.Add(
                saveToServerItem
            );
            menuStrip.Items.Add(serverMenu);
            serverMenu.DropDownItems.Add(
                openFromServerItem
            );
            menuStrip.Items.Add(fileMenu);

            menuStrip.Items.Add(tableMenu);
            menuStrip.Items.Add(operationsMenu);

            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);
        }

        private void BuildLeftPanel(
     SplitterPanel panel)
        {
            panel.Padding = new Padding(12);

            _databaseLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 38,
                AutoSize = false,

                Text = "База: не відкрита",

                Font = new Font(
         Font.FontFamily,
         10,
         FontStyle.Bold
     ),

                TextAlign = ContentAlignment.MiddleLeft,

                Padding = new Padding(0, 16, 0, 0),

                AutoEllipsis = true
            };

            var tablesLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                AutoSize = false,

                Text = "Таблиці",

                Font = new Font(
                    Font.FontFamily,
                    10,
                    FontStyle.Regular
                ),

                TextAlign = ContentAlignment.MiddleLeft
            };

            _tablesTree = new TreeView
            {
                Dock = DockStyle.Fill,
                HideSelection = false,

                Font = new Font(
                    Font.FontFamily,
                    10
                )
            };

            _tablesTree.AfterSelect +=
                TablesTree_AfterSelect;

            panel.Controls.Add(_tablesTree);
            panel.Controls.Add(tablesLabel);
            panel.Controls.Add(_databaseLabel);
        }
        private void BuildRightPanel(
            SplitterPanel panel)
        {
            panel.Padding = new Padding(10);

            _tableLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 42,
                Text = "Оберіть таблицю",
                Font = new Font(
                    Font.FontFamily,
                    14,
                    FontStyle.Bold
                ),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 8, 0, 0)
            };

            _addRowButton = new Button
            {
                Text = "Додати рядок",
                Width = 130,
                Height = 32,
                Enabled = false
            };

            _addRowButton.Click += (_, _) =>
                AddRow();

            _editRowButton = new Button
            {
                Text = "Редагувати",
                Width = 120,
                Height = 32,
                Enabled = false
            };

            _editRowButton.Click += (_, _) =>
                EditRow();

            _deleteRowButton = new Button
            {
                Text = "Видалити",
                Width = 120,
                Height = 32,
                Enabled = false
            };

            _deleteRowButton.Click += (_, _) =>
                DeleteRow();

            buttonsPanel.Controls.Add(_addRowButton);
            buttonsPanel.Controls.Add(_editRowButton);
            buttonsPanel.Controls.Add(_deleteRowButton);

            _dataGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode =
                    DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };

            panel.Controls.Add(_dataGrid);
            panel.Controls.Add(buttonsPanel);
            panel.Controls.Add(_tableLabel);
        }

        private void BuildStatusBar()
        {
            var statusStrip = new StatusStrip();

            _statusLabel =
                new ToolStripStatusLabel(
                    "Готово"
                );

            statusStrip.Items.Add(_statusLabel);

            Controls.Add(statusStrip);
        }
        private void CreateTable()
        {
            if (_database == null)
            {
                MessageBox.Show(
                    "Спочатку створіть або відкрийте базу даних.",
                    "Немає бази даних",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            using var dialog =
                new CreateTableForm();

            if (dialog.ShowDialog(this) !=
                DialogResult.OK)
            {
                return;
            }

            if (dialog.CreatedTable == null)
                return;

            try
            {
                _database.AddTable(
                    dialog.CreatedTable
                );
                AutoSaveDatabase();

                RefreshDatabaseView();

                SelectTable(
                    dialog.CreatedTable
                );

                _statusLabel.Text =
                    $"Створено таблицю: " +
                    $"{dialog.CreatedTable.Name}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка створення таблиці",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private void SelectTable(Table table)
        {
            if (_tablesTree.Nodes.Count == 0)
                return;

            TreeNode databaseNode =
                _tablesTree.Nodes[0];

            foreach (TreeNode node
                     in databaseNode.Nodes)
            {
                if (ReferenceEquals(
                        node.Tag,
                        table))
                {
                    _tablesTree.SelectedNode =
                        node;

                    node.EnsureVisible();

                    break;
                }
            }
        }
        private void CreateNewDatabase()
        {
            string? name = PromptForText(
                "Нова база даних",
                "Введіть назву бази даних:"
            );

            if (string.IsNullOrWhiteSpace(name))
                return;

            _database = new Database(name.Trim());
            _selectedTable = null;
            _currentFilePath = null;

            RefreshDatabaseView();

            _statusLabel.Text =
                $"Створено базу даних: {_database.Name}";
        }

        private void OpenDatabase()
        {
            using var dialog =
                new OpenFileDialog
                {
                    Title =
                        "Відкрити базу даних",

                    Filter =
                        "TableDBMS database (*.tdb)|*.tdb|" +
                        "JSON files (*.json)|*.json|" +
                        "All files (*.*)|*.*"
                };

            if (dialog.ShowDialog() !=
                DialogResult.OK)
            {
                return;
            }

            try
            {
                _database =
                    _storageService.LoadDatabase(
                        dialog.FileName
                    );

                _currentFilePath =
                    dialog.FileName;

                _selectedTable = null;

                RefreshDatabaseView();

                _statusLabel.Text =
                    $"Відкрито: {_database.Name}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка відкриття",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void SaveDatabase()
        {
            if (_database == null)
            {
                MessageBox.Show(
                    "Спочатку створіть або відкрийте базу даних.",
                    "Немає бази даних",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    _currentFilePath))
            {
                SaveDatabaseAs();
                return;
            }

            try
            {
                _storageService.SaveDatabase(
                    _database,
                    _currentFilePath
                );

                _statusLabel.Text =
                    "Базу даних збережено.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка збереження",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void SaveDatabaseAs()
        {
            if (_database == null)
            {
                MessageBox.Show(
                    "Спочатку створіть або відкрийте базу даних.",
                    "Немає бази даних",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            using var dialog =
                new SaveFileDialog
                {
                    Title =
                        "Зберегти базу даних",

                    Filter =
                        "TableDBMS database (*.tdb)|*.tdb",

                    DefaultExt = "tdb",
                    AddExtension = true,
                    FileName =
                        $"{_database.Name}.tdb"
                };

            if (dialog.ShowDialog() !=
                DialogResult.OK)
            {
                return;
            }

            try
            {
                _storageService.SaveDatabase(
                    _database,
                    dialog.FileName
                );

                _currentFilePath =
                    dialog.FileName;

                _statusLabel.Text =
                    $"Збережено: {dialog.FileName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка збереження",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private async Task CheckServerConnectionAsync()
        {
            try
            {
                _statusLabel.Text =
                    "Перевірка з'єднання із сервером...";

                bool connected =
                    await _remoteStorageService
                        .CheckConnectionAsync();

                if (!connected)
                {
                    throw new InvalidOperationException(
                        "Сервер повернув помилку."
                    );
                }

                _statusLabel.Text =
                    "З'єднання із сервером встановлено.";

                MessageBox.Show(
                    "З'єднання із сервером успішно встановлено.",
                    "Сервер",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                _statusLabel.Text =
                    "Сервер недоступний.";

                MessageBox.Show(
                    $"Не вдалося підключитися до сервера.\n\n{ex.Message}",
                    "Помилка з'єднання",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private async Task SaveDatabaseToServerAsync()
        {
            if (_database == null)
            {
                MessageBox.Show(
                    "Спочатку створіть або відкрийте базу даних.",
                    "Немає бази даних",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            try
            {
                _statusLabel.Text =
                    "Збереження бази даних на сервері...";

                await _remoteStorageService
                    .SaveDatabaseAsync(_database);

                _statusLabel.Text =
                    $"Базу '{_database.Name}' збережено на сервері.";

                MessageBox.Show(
                    $"Базу даних '{_database.Name}' успішно збережено на сервері.",
                    "Сервер",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                _statusLabel.Text =
                    "Помилка збереження на сервері.";

                MessageBox.Show(
                    $"Не вдалося зберегти базу даних на сервері.\n\n{ex.Message}",
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private async Task OpenDatabaseFromServerAsync()
        {
            try
            {
                _statusLabel.Text =
                    "Отримання списку баз із сервера...";

                List<string> databaseNames =
                    await _remoteStorageService
                        .GetDatabaseNamesAsync();

                if (databaseNames.Count == 0)
                {
                    MessageBox.Show(
                        "На сервері немає збережених баз даних.",
                        "Сервер",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    _statusLabel.Text =
                        "На сервері немає баз даних.";

                    return;
                }

                string? selectedName =
                    SelectRemoteDatabase(databaseNames);

                if (string.IsNullOrWhiteSpace(selectedName))
                    return;

                _statusLabel.Text =
                    $"Завантаження '{selectedName}'...";

                _database =
                    await _remoteStorageService
                        .LoadDatabaseAsync(selectedName);

                _selectedTable = null;
                _currentFilePath = null;

                RefreshDatabaseView();

                _statusLabel.Text =
                    $"Базу '{_database.Name}' завантажено із сервера.";
            }
            catch (Exception ex)
            {
                _statusLabel.Text =
                    "Помилка завантаження із сервера.";

                MessageBox.Show(
                    $"Не вдалося відкрити базу даних із сервера.\n\n{ex.Message}",
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private static string? SelectRemoteDatabase(
            IReadOnlyList<string> databaseNames)
        {
            using var dialog = new Form
            {
                Text = "Відкрити з сервера",
                Width = 430,
                Height = 330,
                StartPosition =
                    FormStartPosition.CenterParent,
                FormBorderStyle =
                    FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label
            {
                Left = 20,
                Top = 20,
                Width = 370,
                Height = 25,
                Text = "Оберіть базу даних:"
            };

            var listBox = new ListBox
            {
                Left = 20,
                Top = 50,
                Width = 370,
                Height = 180
            };

            foreach (string databaseName in databaseNames)
            {
                listBox.Items.Add(databaseName);
            }

            if (listBox.Items.Count > 0)
            {
                listBox.SelectedIndex = 0;
            }

            var openButton = new Button
            {
                Text = "Відкрити",
                Left = 210,
                Top = 245,
                Width = 85,
                DialogResult = DialogResult.OK
            };

            var cancelButton = new Button
            {
                Text = "Скасувати",
                Left = 305,
                Top = 245,
                Width = 85,
                DialogResult = DialogResult.Cancel
            };

            listBox.DoubleClick += (_, _) =>
            {
                if (listBox.SelectedItem != null)
                {
                    dialog.DialogResult =
                        DialogResult.OK;

                    dialog.Close();
                }
            };

            dialog.Controls.Add(label);
            dialog.Controls.Add(listBox);
            dialog.Controls.Add(openButton);
            dialog.Controls.Add(cancelButton);

            dialog.AcceptButton = openButton;
            dialog.CancelButton = cancelButton;

            return dialog.ShowDialog() ==
                   DialogResult.OK
                ? listBox.SelectedItem?.ToString()
                : null;
        }
        private void RefreshDatabaseView()
        {
            _tablesTree.Nodes.Clear();

            if (_database == null)
            {
                _databaseLabel.Text =
     "База: не відкрита";

                ClearTableView();
                return;
            }

            _databaseLabel.Text =
                $"База: {_database.Name}";

            var databaseNode =
                new TreeNode(_database.Name)
                {
                    Tag = _database
                };

            foreach (Table table in
                     _database.Tables)
            {
                var tableNode =
                    new TreeNode(table.Name)
                    {
                        Tag = table
                    };

                databaseNode.Nodes.Add(
                    tableNode
                );
            }

            _tablesTree.Nodes.Add(databaseNode);
            databaseNode.Expand();
        }

        private void TablesTree_AfterSelect(
    object? sender,
    TreeViewEventArgs e)
        {
            if (e.Node?.Tag is not Table table)
            {
                _selectedTable = null;
                ClearTableView();
                return;
            }

            _selectedTable = table;
            ShowTable(table);
        }
        private void ShowTable(Table table)
        {
            _dataGrid.Columns.Clear();
            _dataGrid.Rows.Clear();

            _tableLabel.Text =
                $"Таблиця: {table.Name}";

            _addRowButton.Enabled = true;
            _editRowButton.Enabled = true;
            _deleteRowButton.Enabled = true;

            foreach (Column column in
                     table.Schema.Columns)
            {
                _dataGrid.Columns.Add(
                    column.Name,
                    $"{column.Name}\n" +
                    $"({column.DataTypeName})"
                );
            }

            foreach (Row row in table.Rows)
            {
                object[] values =
                    row.Values
                        .Select(
                            value =>
                                (object?)value
                                ?? string.Empty
                        )
                        .ToArray();

                _dataGrid.Rows.Add(values);
            }

            _statusLabel.Text =
                $"Рядків: {table.Rows.Count}, " +
                $"стовпців: " +
                $"{table.Schema.Columns.Count}";
        }

        private void ClearTableView()
        {
            _selectedTable = null;

            _dataGrid.Columns.Clear();
            _dataGrid.Rows.Clear();

            _tableLabel.Text =
                "Оберіть таблицю";

            _addRowButton.Enabled = false;
            _editRowButton.Enabled = false;
            _deleteRowButton.Enabled = false;
        }

        private static string? PromptForText(
            string title,
            string message)
        {
            using var dialog = new Form
            {
                Text = title,
                Width = 420,
                Height = 180,
                FormBorderStyle =
                    FormBorderStyle.FixedDialog,
                StartPosition =
                    FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label
            {
                Left = 20,
                Top = 20,
                Width = 360,
                Text = message
            };

            var textBox = new TextBox
            {
                Left = 20,
                Top = 50,
                Width = 360
            };

            var okButton = new Button
            {
                Text = "OK",
                Left = 215,
                Width = 80,
                Top = 90,
                DialogResult =
                    DialogResult.OK
            };

            var cancelButton = new Button
            {
                Text = "Скасувати",
                Left = 300,
                Width = 80,
                Top = 90,
                DialogResult =
                    DialogResult.Cancel
            };

            dialog.Controls.Add(label);
            dialog.Controls.Add(textBox);
            dialog.Controls.Add(okButton);
            dialog.Controls.Add(cancelButton);

            dialog.AcceptButton = okButton;
            dialog.CancelButton = cancelButton;

            return dialog.ShowDialog() ==
                   DialogResult.OK
                ? textBox.Text
                : null;
        }
    }
}



