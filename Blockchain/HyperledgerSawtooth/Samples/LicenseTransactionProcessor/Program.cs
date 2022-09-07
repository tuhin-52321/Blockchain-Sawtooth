using log4net.Appender;
using Sawtooth.Sdk.Net.Utils;

class LicenseTP
{
    private static Logger log = Logger.GetLogger(typeof(LicenseTP));
    private static LicenseTransactionProcessor.Tally.LicenseProcessor? processor;

    public static void Main(string[] args)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        log4net.Config.XmlConfigurator.Configure(new FileInfo("Log4Net.Config"));

        var validatorAddress = args.Any() ? args.First() : "tcp://127.0.0.1:4004";

        processor = new LicenseTransactionProcessor.Tally.LicenseProcessor(validatorAddress);

        processor.Start();

        Console.CancelKeyPress += new ConsoleCancelEventHandler(CloseProcess);
    }

    private static void CloseProcess(object? sender, ConsoleCancelEventArgs e)
    {
        if (processor != null)
        {
            log.Info("Stop requested ...");
            processor.Stop();
        }
    }
}
