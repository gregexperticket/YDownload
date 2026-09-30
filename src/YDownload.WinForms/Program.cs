namespace YDownload.WinForms;

static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        if (args.Length > 0)
            return Headless.Run(args);

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
        return 0;
    }
}
