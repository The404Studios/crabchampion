using CrabChampionsTrainer.UI;

namespace CrabChampionsTrainer;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Enable visual styles for modern look
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.SystemAware);

        // Check for single instance
        using var mutex = new Mutex(true, "CrabChampionsTrainer", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "Crab Champions Trainer is already running!",
                "Already Running",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        // Handle unhandled exceptions
        Application.ThreadException += (s, e) =>
        {
            MessageBox.Show(
                $"An error occurred:\n\n{e.Exception.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        };

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show(
                    $"A fatal error occurred:\n\n{ex.Message}",
                    "Fatal Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        };

        // Run the application
        Application.Run(new MainForm());
    }
}
