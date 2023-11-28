using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text;
using vr_challenge.Data;
using vr_challenge.Errors;
using vr_challenge.Parser;

namespace vr_challenge
{
    internal class Program
    {
        // setup
        private const string dbName = "shipment.db";
        private const string dataFileName = "data.txt";
        private const int maxBoxBatchSize = 500;

        static void Main(string[] args)
        {
            string executablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            InitDB(executablePath);

            using var watcher = new FileSystemWatcher(executablePath);

            watcher.Filter = dataFileName;
            watcher.Created += new FileSystemEventHandler((sender, e) => OnCreated(sender, e, executablePath));
            watcher.Error += OnError;

            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Initialization successfull, drop data.txt file in the same folder with executable to start processing");
            Console.WriteLine($"Executable folder location: {executablePath}");
            Console.ReadLine();
        }

        private static void OnCreated(object sender, FileSystemEventArgs e, string executablePath)
        {
            Console.WriteLine($"Found new data file to process: {e.FullPath}");
            ExecuteFile(executablePath);
        }

        private static void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"Watcher exception: {e.GetException()}");
        }

        private static void InitDB(string dbPath)
        {
            try
            {
                using var dbContext = new ShipmentContext(Path.Combine(dbPath, dbName));
                dbContext.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on database initialization, please check configuration! Error: {ex}");
            }
        }

        private static void ExecuteFile(string directoryPath)
        {
            var processedBoxIds = new HashSet<string>();
            var savedBoxIds = new HashSet<string>();

            string dbPath = Path.Combine(directoryPath, dbName);

            ulong dataFileLineNumber = 1;
            try
            {
                using var dbContext = new ShipmentContext(dbPath);

                using var fs = new FileStream(Path.Combine(directoryPath, dataFileName), FileMode.Open, FileAccess.Read);
                using var sr = new StreamReader(fs, Encoding.UTF8);

                string currLine;
                var preParsedBoxes = new List<ShipmentBox>();
                ShipmentBox currShipmentBox = null;

                do
                {
                    currLine = sr.ReadLine();

                    if (string.IsNullOrEmpty(currLine))
                    {
                        dataFileLineNumber++;
                        continue;
                    }

                    Console.WriteLine($"Processing line {dataFileLineNumber}:  {currLine}");

                    var lineParams = currLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    var lineType = ShipmentParser.GetLineType(lineParams);

                    if (lineType == LineType.HDR)
                    {
                        if (currShipmentBox == null)
                        {
                            currShipmentBox = ShipmentParser.ProcessHDRParams(lineParams);
                        }
                        else
                        {
                            preParsedBoxes.Add(currShipmentBox);
                            currShipmentBox = ShipmentParser.ProcessHDRParams(lineParams);
                        }

                        if (processedBoxIds.Contains(currShipmentBox.BoxId))
                            throw new ShipmentParserException($"Found duplicate box with BoxId: {currShipmentBox.BoxId}.");

                        processedBoxIds.Add(currShipmentBox.BoxId);

                        // if we've exceeded max batch size we should sink all collected data to db and continue
                        if (preParsedBoxes.Count() >= maxBoxBatchSize)
                        {
                            SaveBoxes(dbContext, preParsedBoxes);
                            savedBoxIds.UnionWith(preParsedBoxes.Select(b => b.BoxId));
                            preParsedBoxes = [];
                        }

                        continue;
                    }

                    if (lineType == LineType.LINE)
                    {
                        if (currShipmentBox == null)
                        {
                            throw new ShipmentParserException($"No box found for \"LINE\" shipment data!");
                        }

                        var productShipment = ShipmentParser.ProcessLineParams(lineParams);
                        productShipment.BoxId = currShipmentBox.BoxId;
                        currShipmentBox.BoxContents.Add(productShipment);
                    }
                } while (!sr.EndOfStream);

                if (preParsedBoxes.Count == 0 && currShipmentBox != null)
                {
                    preParsedBoxes.Add(currShipmentBox);
                }

                SaveBoxes(dbContext, preParsedBoxes);
                savedBoxIds.UnionWith(preParsedBoxes.Select(b => b.BoxId));
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database update exception, most probably box duplicated already found in database, unrolling previous saved changes: {dbEx}");
                UnrollProcessedBoxes(dbPath, savedBoxIds);

            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"Error while parsing file on line: {dataFileLineNumber}, unrolling previous saved changes: {ioEx}");
                UnrollProcessedBoxes(dbPath, savedBoxIds);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine($"Processing finished, processed {dataFileLineNumber} lines, saved {savedBoxIds.Count()} boxes.");
            Console.WriteLine($"Press any key to exit, or delete data.txt file and add new one to process!");
        }

        private static void SaveBoxes(ShipmentContext dbContext, ICollection<ShipmentBox> shipmentBoxes, bool useBulk = false)
        {
            if (useBulk)
            {
                throw new NotImplementedException("Bulk insert is not currently implemented!");
                
                // Bulk insert will throw "Missing BoxId column" error, due to lack of time I was unable to solve it in time
                //dbContext.BulkInsert(shipmentBoxes, options => options.AutoMapOutputDirection = false);
            }
            
            dbContext.Box.AddRange(shipmentBoxes);
            dbContext.SaveChanges();
        }

        private static void UnrollProcessedBoxes(string dbPath, HashSet<string> savedBoxIds)
        {
            using var dbContext = new ShipmentContext(dbPath);
            dbContext.Box.Where(b => savedBoxIds.Contains(b.BoxId)).ExecuteDelete();
        }
    }
}
