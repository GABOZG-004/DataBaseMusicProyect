# Instalar .NET en Ubuntu y Debian
sudo apt-get update && \
sudo apt-get install -y dotnet-sdk-8.0

# Instalar.NET en Fedora
sudo dnf install dotnet-sdk-8.0

# Checar la version
dotnet --version

# Instalar las dependencias mostradas en el archivo .csproj
dotnet restore

# Compilacion y ejecucion
dotnet build
dotnet run
