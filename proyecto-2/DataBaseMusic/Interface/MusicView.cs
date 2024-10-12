using Gtk;

/// <summary>
/// Ventana principal de la aplicación MusicDataBaseApp.
/// Muestra la interfaz gráfica, permite seleccionar un directorio y muestra las canciones.
/// </summary>
public class MusicView : Window
{
    /// <summary>
    /// Evento que se dispara cuando se selecciona un directorio.
    /// </summary>
    public event Action<string>? OnDirectorySelected;

    /// <summary>
    /// Evento que se dispara cuando se selecciona un álbum.
    /// </summary>
    public event Action<string>? OnAlbumSelected;

    // Componentes de la interfaz gráfica
    private TreeView songView;
    private ListStore songListStore;
    private MusicMiner miner;
    private Label statusLabel;
    private ListBox albumListBox;
    private Entry searchEntry;
    private List<Song> allSongs;

    /// <summary>
    /// Constructor de la ventana principal.
    /// Inicializa los componentes de la interfaz y la base de datos.
    /// </summary>
    public MusicView() : base("MusicDataBaseApp")
    {
        miner = new MusicMiner();
        SetDefaultSize(800, 600); // Tamaño de la ventana
        SetPosition(WindowPosition.Center); // Centra la ventana en la pantalla

        VBox vbox = new VBox();
        HBox hbox = new HBox();

        // Botón para seleccionar el directorio
        Button selectFolderButton = new Button("Select Directory");
        selectFolderButton.Clicked += OnSelectFolderClicked;

        // Campo de texto para buscar canciones
        searchEntry = new Entry();
        searchEntry.PlaceholderText = "Buscar canción...";
        searchEntry.Changed += OnSearchTextChanged;

        // Vista de árbol para mostrar las canciones
        songView = new TreeView();
        songListStore = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string));

        songView.Model = songListStore;

        // Crear las columnas de la tabla para título, artista, álbum y año
        TreeViewColumn titleColumn = new TreeViewColumn { Title = "Title" };
        TreeViewColumn artistColumn = new TreeViewColumn { Title = "Artist" };
        TreeViewColumn albumColumn = new TreeViewColumn { Title = "Album" };
        TreeViewColumn yearColumn = new TreeViewColumn { Title = "Year" };

        // Renderizadores de texto para cada columna
        CellRendererText titleCell = new CellRendererText();
        CellRendererText artistCell = new CellRendererText();
        CellRendererText albumCell = new CellRendererText();
        CellRendererText yearCell = new CellRendererText();

        // Añadir las celdas a las columnas correspondientes
        titleColumn.PackStart(titleCell, true);
        artistColumn.PackStart(artistCell, true);
        albumColumn.PackStart(albumCell, true);
        yearColumn.PackStart(yearCell, true);

        // Definir qué datos mostrar en cada columna
        titleColumn.AddAttribute(titleCell, "text", 0);
        artistColumn.AddAttribute(artistCell, "text", 1);
        albumColumn.AddAttribute(albumCell, "text", 2);
        yearColumn.AddAttribute(yearCell, "text", 3);

        // Añadir columnas a la vista de canciones
        songView.AppendColumn(titleColumn);
        songView.AppendColumn(artistColumn);
        songView.AppendColumn(albumColumn);
        songView.AppendColumn(yearColumn);

        // Añadir componentes al diseño de la interfaz
        hbox.PackStart(selectFolderButton, false, false, 5);
        vbox.PackStart(hbox, false, false, 5);
        vbox.PackStart(searchEntry, false, false, 5);
        ScrolledWindow scrolledWindow = new ScrolledWindow();
        scrolledWindow.Add(songView);
        vbox.PackStart(scrolledWindow, true, true, 0);

        // Añadir componentes al diseño de la interfaz
        albumListBox = new ListBox();
        albumListBox.SelectedRowsChanged += OnAlbumSelectedChanged;  // Evento cuando se selecciona un álbum
        vbox.PackStart(albumListBox, false, false, 5);

        // Etiqueta de estado
        statusLabel = new Label("Estado: Esperando acción");
        vbox.PackStart(statusLabel, false, false, 5);

        // Añadir el diseño principal a la ventana
        Add(vbox);
        ShowAll();

        //Inicialización de la lista de todas las canciones
        allSongs = new List<Song>(); 
    }

    /// <summary>
    /// Inicia la aplicación GTK.
    /// </summary>
    public void Run() => Application.Run();

    /// <summary>
    /// Controlador de evento para la selección de un directorio.
    /// Carga las canciones del directorio seleccionado y las inserta en la base de datos.
    /// </summary>
    private void OnSelectFolderClicked(object? sender, EventArgs e)
    {
        FileChooserDialog fileChooser = new FileChooserDialog(
            "Choose the directory",
            this,
            FileChooserAction.SelectFolder,
            "Cancel", ResponseType.Cancel,
            "Select", ResponseType.Accept);

        if (fileChooser.Run() == (int)ResponseType.Accept)
        {
            string directory = fileChooser.Filename;
            OnDirectorySelected?.Invoke(directory);

            Console.WriteLine($"Directorio seleccionado: {directory}");

            // Iniciar el proceso de minado y mostrar progreso en la terminal
            Console.WriteLine("Iniciando el proceso de minado de archivos...");
            LoadSongsFromDirectory(directory);  // Cargar las canciones y mostrarlas en la interfaz

            Console.WriteLine("Minado completado. Iniciando inserción de datos en la base de datos...");
        
            // Inserción de las canciones en la base de datos
            InsertSongsToDatabase(directory);

            Console.WriteLine("Inserción de datos completada.");

            LoadSongsFromDirectory(fileChooser.Filename);
            UpdateStatus("Procesamiento completado.");
        }

        fileChooser.Dispose();
    }

    /// <summary>
    /// Inserta las canciones de un directorio en la base de datos.
    /// </summary>
    /// <param name="directory">Directorio seleccionado para minar canciones.</param>
    private void InsertSongsToDatabase(string directory)
    {
        var songs = miner.MineDirectory(directory);  // Minar las canciones desde el directorio

        foreach (var song in songs)
        {
            // Verificar si el álbum ya existe en la base de datos, si no, insertarlo
            Console.WriteLine($"Insertando álbum: {song.Album}");

            // Usar el método AddMp3ToDatabase para insertar el archivo MP3
            miner.AddMp3ToDatabase(song.FilePath);

            // Mostrar el progreso en la consola
            Console.WriteLine($"Canción insertada: {song.Title} - {song.Artist} - {song.Album} - {song.Year}");
        }
    }

    /// <summary>
    /// Controlador del evento que se dispara cuando se selecciona un álbum.
    /// Actualiza la interfaz y dispara el evento OnAlbumSelected.
    /// </summary>
    private void OnAlbumSelectedChanged(object? sender, EventArgs e)
    {
        if (albumListBox.SelectedRow != null)
        {
            var selectedAlbum = ((Label)albumListBox.SelectedRow.Child).Text;
            OnAlbumSelected?.Invoke(selectedAlbum);  // Dispara el evento para el controlador
        }
    }

    /// <summary>
    /// Actualiza el texto del estado en la interfaz.
    /// </summary>
    /// <param name="message">Mensaje de estado.</param>
    public void UpdateStatus(string message)
    {
        statusLabel.Text = $"Estado: {message}";
    }
    
    /// <summary>
    /// Carga las canciones de un directorio en la interfaz.
    /// </summary>
    /// <param name="directory">Directorio seleccionado.</param>
    private void LoadSongsFromDirectory(string directory)
    {
        songListStore.Clear();
        allSongs = miner.MineDirectory(directory).ToList();

        var songs = miner.MineDirectory(directory);
        foreach (var song in songs)
        {
            Console.WriteLine($"Canción encontrada: {song.Title} - {song.Artist} - {song.Album} - {song.Year}");
            songListStore.AppendValues(song.Title, song.Artist, song.Album, song.Year);
        }
    }

    /// <summary>
    /// Filtra las canciones según el texto ingresado en la barra de búsqueda.
    /// </summary>
    private void OnSearchTextChanged(object? sender, EventArgs e)
    {
        string searchText = searchEntry.Text.ToLower();
        songListStore.Clear();

        var filteredSongs = allSongs.Where(song =>
            song.Title.ToLower().Contains(searchText) ||
            song.Artist.ToLower().Contains(searchText) ||
            song.Album.ToLower().Contains(searchText)).ToList();

        foreach (var song in filteredSongs)
        {
            songListStore.AppendValues(song.Title, song.Artist, song.Album, song.Year);
        }
    }
}