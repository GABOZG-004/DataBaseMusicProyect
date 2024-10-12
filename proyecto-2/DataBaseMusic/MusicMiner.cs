using MusicDatabaseApp;

/// <summary>
/// Clase encargada de procesar directorios y extraer información musical (metadata) de los archivos encontrados.
/// Interactúa con la base de datos para almacenar la información extraída.
/// </summary>
public class MusicMiner
{
    // Instancia de la base de datos
    private MusicDatabase _db;

    /// <summary>
    /// Constructor de MusicMiner. Inicializa la instancia de la base de datos y asegura que las tablas necesarias estén creadas.
    /// </summary>
    public MusicMiner()
    {
        _db = MusicDatabase.Instance;
        _db.CreateTables(); // Asegura que las tablas de la base de datos están creadas
    }

    /// <summary>
    /// Minar un directorio dado, extrayendo información de los archivos de música soportados (.mp3, .flac).
    /// </summary>
    /// <param name="directory">Ruta del directorio a procesar.</param>
    /// <returns>Una lista de objetos Song con la información extraída de los archivos.</returns>
    public List<Song> MineDirectory(string directory)
    {
        List<Song> songs = new List<Song>();

        // Verifica si el directorio existe
        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"El directorio {directory} no existe.");
            return songs; // Retorna lista vacía si el directorio no existe
        }

        try
        {
            // Filtros de extensiones soportadas
            string[] supportedExtensions = new[] { ".mp3", ".flac" };

            // Busca recursivamente los archivos soportados en el directorio
            var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)
                                 .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLower()));

            foreach (var file in files)
            {
                try
                {
                    // Extrae metadata del archivo y lo añade a la lista de canciones si es válido
                    var metadata = ExtractMetadata(file);
                    if(metadata != null)
                    {
                        var song = new Song
                        {
                            Title = metadata.Title,
                            Artist = metadata.Performer,
                            Album = metadata.AlbumName,
                            FilePath = metadata.AlbumPath,
                            TrackNumber = metadata.TrackNumber,
                            Year = metadata.Year,
                            Genre = metadata.Genre 
                        };

                        songs.Add(song);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error procesando el archivo {file}: {ex.Message}");
                    // Continua con el siguiente archivo en caso de error
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accediendo al directorio: {ex.Message}");
        }

        return songs;
    }

    /// <summary>
    /// Agrega archivos MP3 encontrados en un directorio dado a la base de datos.
    /// </summary>
    /// <param name="directoryPath">Ruta del directorio que contiene archivos MP3.</param>
    /// <returns>El número de archivos MP3 procesados.</returns>
    public int AddMp3sFromDirectory(string directoryPath)
    {
        int processedFiles = 0;

        if (Directory.Exists(directoryPath))
        {
            string[] mp3Files = Directory.GetFiles(directoryPath, "*.mp3", SearchOption.AllDirectories);

            foreach (string mp3FilePath in mp3Files)
            {
                AddMp3ToDatabase(mp3FilePath);
                processedFiles++;
            }
        }
    
        return processedFiles;
    }

    /// <summary>
    /// Inserta la metadata de un archivo MP3 en la base de datos.
    /// </summary>
    /// <param name="mp3FilePath">Ruta del archivo MP3 a procesar.</param>
    public void AddMp3ToDatabase(string mp3FilePath)
    {
        // Extrae metadata del archivo MP3
        var metadata = ExtractMetadata(mp3FilePath);
        if (metadata != null)
        {
            // Inserta la metadata en las tablas correspondientes de la base de datos
            _db.InsertAlbum(metadata.AlbumPath, metadata.AlbumName, metadata.Year);
            _db.InsertRola(1, 1, metadata.AlbumPath, metadata.Title, metadata.TrackNumber, metadata.Year, metadata.Genre);
            _db.InsertPerformer(1, metadata.Performer); // Se usa un id_type de 1 como ejemplo
            _db.InsertPerson(metadata.Performer, metadata.Performer, metadata.Birth_Date, metadata.Death_Date); // Simulamos una fecha de nacimiento
            _db.InsertGroup(metadata.GroupName, "Unknown", "Unknown");
        }
    }

    /// <summary>
    /// Extrae metadata de un archivo MP3 usando la biblioteca TagLib.
    /// </summary>
    /// <param name="mp3FilePath">Ruta del archivo MP3 del cual extraer metadata.</param>
    /// <returns>Un objeto Mp3Metadata con la información extraída o null si ocurre un error.</returns>
    public Mp3Metadata? ExtractMetadata(string mp3FilePath)
    {
        try
        {
            // Utiliza TagLib para leer la metadata del archivo
            var file = TagLib.File.Create(mp3FilePath);
            var title = file.Tag.Title ?? "Unknown";
            var performer = file.Tag.Performers.Length > 0 ? file.Tag.Performers[0] : "Unknown";
            var album = file.Tag.Album ?? "Unknown";
            var year = (int)(file.Tag.Year > 0 ? file.Tag.Year : 0);
            var genre = file.Tag.Genres.Length > 0 ? file.Tag.Genres[0] : "Unknown";
            var trackNumber = (int)(file.Tag.Track > 0 ? file.Tag.Track : 0);
            var group = file.Tag.Performers.Length > 0 ? file.Tag.Performers[0] : "Unknown";
            return new Mp3Metadata
            {
                // Retorna la metadata extraída
                Title = title,
                Performer = performer,
                AlbumName = album,
                Year = year,
                Genre = genre,
                TrackNumber = trackNumber,
                AlbumPath = mp3FilePath,
                GroupName = group
            };
        }
        catch (Exception e)
        {
            // Maneja cualquier excepción ocurrida durante la extracción de metadata
            Console.WriteLine($"Error processing {mp3FilePath}: {e.Message}");
            return null;
        }
    }
}
