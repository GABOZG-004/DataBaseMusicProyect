using Gtk;

class Program
{
    static void Main(string[] args)
    {
        Application.Init();
        MusicView view = new MusicView();
        view.DeleteEvent += delegate { Application.Quit(); };
        view.Run();
    }
}


