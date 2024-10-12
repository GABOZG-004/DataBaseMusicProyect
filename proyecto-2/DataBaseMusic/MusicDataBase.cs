using System.Data.SQLite;

namespace MusicDatabaseApp
{
    /// <summary>
    /// Clase que maneja la base de datos SQLite para la aplicación MusicDatabaseApp.
    /// Implementa el patrón Singleton para asegurar que solo una instancia de la base de datos esté activa.
    /// </summary>
    public class MusicDatabase
    {
        // Instancia única de la clase, implementada usando Lazy para inicialización diferida.
        private static readonly Lazy<MusicDatabase> _instance = new Lazy<MusicDatabase>(() => new MusicDatabase());

        // Cadena de conexión a la base de datos SQLite.
        private string connectionString = "Data Source=DataBaseMusic.db;Version=3;";

        // Constructor privado para el patrón Singleton.
        private MusicDatabase() { }

        /// <summary>
        /// Obtiene la instancia única de la clase MusicDatabase.
        /// </summary>
        public static MusicDatabase Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        /// <summary>
        /// Crea y abre una nueva conexión a la base de datos SQLite.
        /// </summary>
        /// <returns>Una conexión SQLite abierta.</returns>
        private SQLiteConnection GetConnection()
        {
            var connection = new SQLiteConnection(connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Crea las tablas necesarias para la aplicación si no existen.
        /// </summary>
        public void CreateTables()
        {
            using (var _connection = GetConnection())
            using (var cmd = new SQLiteCommand(_connection))
            {
                // Crear tabla 'types' si no existe
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS types (
                                        id_type INTEGER PRIMARY KEY,
                                        description TEXT)";
                cmd.ExecuteNonQuery();

                // Crear tabla 'performers' si no existe
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS performers (
                                        id_performer INTEGER PRIMARY KEY,
                                        id_type INTEGER,
                                        name TEXT,
                                        FOREIGN KEY (id_type) REFERENCES types(id_type))";
                cmd.ExecuteNonQuery();

                // Crear tabla 'persons' si no existe
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS persons (
                                        id_person INTEGER PRIMARY KEY,
                                        stage_name TEXT,
                                        real_name TEXT,
                                        birth_date TEXT,

                                        death_date TEXT);";

                cmd.ExecuteNonQuery();

                // Crear tabla 'groups' si no existe
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS groups (
                                        id_group INTEGER PRIMARY KEY,
                                        name TEXT,
                                        start_date TEXT,
                                        end_date TEXT)";

                cmd.ExecuteNonQuery();

                // Crear tabla 'in_group' si no existe
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS in_group (
                                        id_person INTEGER,
                                        id_group INTEGER,
                                        PRIMARY KEY(id_person, id_group),
                                        FOREIGN KEY(id_person) REFERENCES persons(id_person),
                                        FOREIGN KEY(id_group) REFERENCES groups(id_group))";
                cmd.ExecuteNonQuery();

                // Crear tabla 'albums' si no existe
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS albums (
                                        id_album INTEGER PRIMARY KEY,
                                        path TEXT,
                                        name TEXT,
                                        year INTEGER)";
                cmd.ExecuteNonQuery();

                // Crear tabla 'rolas' si no existe
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS rolas (
                                        id_rola INTEGER PRIMARY KEY,
                                        id_performer INTEGER,
                                        id_album INTEGER,
                                        path TEXT,
                                        title TEXT,
                                        track INTEGER,
                                        year INTEGER,
                                        genre TEXT,
                                        FOREIGN KEY (id_performer) REFERENCES performers(id_performer),
                                        FOREIGN KEY (id_album) REFERENCES albums(id_album))";
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Inserta un nuevo intérprete en la tabla 'performers'.
        /// </summary>
        /// <param name="id_type">ID del tipo de intérprete (persona/grupo).</param>
        /// <param name="name">Nombre del intérprete.</param>
        public void InsertPerformer(int id_type, string name)
        {
            using (var _connection = GetConnection())
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO performers (id_type, name) VALUES (@id_type, @name)";
                cmd.Parameters.AddWithValue("@id_type", id_type);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Inserta una nueva persona en la tabla 'persons'.
        /// </summary>
        /// <param name="stageName">Nombre artístico de la persona.</param>
        /// <param name="realName">Nombre real de la persona.</param>
        /// <param name="birthDate">Fecha de nacimiento de la persona.</param>
        /// <param name="deathDate">Fecha de fallecimiento de la persona (si aplica).</param>
        public void InsertPerson(string stageName, string realName, string birthDate, string deathDate)
        {
            using (var _connection = GetConnection())
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO persons (stage_name, real_name, birth_date, death_date) " +
                                  "VALUES (@stage_name, @real_name, @birth_date, @death_date)";
                cmd.Parameters.AddWithValue("@stage_name", stageName);
                cmd.Parameters.AddWithValue("@real_name", realName);
                cmd.Parameters.AddWithValue("@birth_date", birthDate);
                cmd.Parameters.AddWithValue("@death_date", deathDate ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Inserta un nuevo grupo en la tabla 'groups'.
        /// </summary>
        /// <param name="name">Nombre del grupo.</param>
        /// <param name="startDate">Fecha de inicio del grupo.</param>
        /// <param name="endDate">Fecha de fin del grupo (puede ser nulo).</param>
        public void InsertGroup(string name, string startDate, string endDate)
        {
            using (var _connection = GetConnection())
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO groups (name, start_date, end_date) VALUES (@name, @start_date, @end_date)";
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@start_date", startDate);
                cmd.Parameters.AddWithValue("@end_date", endDate ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Inserta un nuevo álbum en la tabla 'albums'.
        /// </summary>
        /// <param name="path">Ruta del archivo del álbum.</param>
        /// <param name="name">Nombre del álbum.</param>
        /// <param name="year">Año de lanzamiento del álbum.</param>
        public void InsertAlbum(string path, string name, int year)
        {
            using (var _connection = GetConnection())
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO albums (path, name, year) VALUES (@path, @name, @year)";
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@year", year);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Inserta una nueva canción en la tabla 'rolas'.
        /// </summary>
        /// <param name="id_performer">ID del intérprete de la canción.</param>
        /// <param name="id_album">ID del álbum al que pertenece la canción.</param>
        /// <param name="path">Ruta del archivo de la canción.</param>
        /// <param name="title">Título de la canción.</param>
        /// <param name="track">Número de pista de la canción.</param>
        /// <param name="year">Año de lanzamiento de la canción.</param>
        /// <param name="genre">Género de la canción.</param>
        public void InsertRola(int id_performer, int id_album, string path, string title, int track, int year, string genre)
        {
            using (var _connection = GetConnection())
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO rolas (id_performer, id_album, path, title, track, year, genre) " +
                                  "VALUES (@id_performer, @id_album, @path, @title, @track, @year, @genre)";
                cmd.Parameters.AddWithValue("@id_performer", id_performer);
                cmd.Parameters.AddWithValue("@id_album", id_album);
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@track", track);
                cmd.Parameters.AddWithValue("@year", year);
                cmd.Parameters.AddWithValue("@genre", genre);
                cmd.ExecuteNonQuery();
            }
        }
    }
}

